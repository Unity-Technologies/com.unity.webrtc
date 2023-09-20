#include "pch.h"

#include <third_party/libyuv/include/libyuv/convert.h>

#include "GraphicsDevice/IGraphicsDevice.h"
#include "UnityVulkanInterfaceFunctions.h"
#include "VulkanGraphicsDevice.h"
#include "VulkanTexture2D.h"
#include "VulkanUtility.h"
#include "WebRTCMacros.h"

#if CUDA_PLATFORM
#include "GraphicsDevice/Cuda/GpuMemoryBufferCudaHandle.h"
#else
#include "GpuMemoryBuffer.h"
#endif

namespace unity
{
namespace webrtc
{
    VulkanGraphicsDevice::VulkanGraphicsDevice(
        UnityGraphicsVulkan* unityVulkan,
        const UnityVulkanInstance* unityVulkanInstance,
        UnityGfxRenderer renderer,
        ProfilerMarkerFactory* profiler)
        : IGraphicsDevice(renderer, profiler)
        , m_unityVulkan(unityVulkan)
        , m_Instance(*unityVulkanInstance)
        , m_hasHostCachedMemory(false)
        , m_commandPool(VK_NULL_HANDLE)
#if VULKAN_USE_CRS
        , m_commandBuffer(VK_NULL_HANDLE)
        , m_fence(VK_NULL_HANDLE)
#endif
#if CUDA_PLATFORM
        , m_isCudaSupport(false)
#endif
    {
    }

    bool VulkanGraphicsDevice::InitV()
    {
        VkPhysicalDeviceMemoryProperties memory;
        vkGetPhysicalDeviceMemoryProperties(m_Instance.physicalDevice, &memory);

        for (uint32_t i = 0; i < memory.memoryTypeCount; ++i)
        {
            const VkMemoryPropertyFlags propertyFlags = memory.memoryTypes[i].propertyFlags;
            if ((propertyFlags & (VK_MEMORY_PROPERTY_HOST_VISIBLE_BIT | VK_MEMORY_PROPERTY_HOST_CACHED_BIT)) ==
                (VK_MEMORY_PROPERTY_HOST_VISIBLE_BIT | VK_MEMORY_PROPERTY_HOST_CACHED_BIT))
                m_hasHostCachedMemory = true;
        }

#if CUDA_PLATFORM
        m_isCudaSupport = InitCudaContext();
#endif
#if VULKAN_USE_CRS
        return true;
#else
        return VK_SUCCESS == CreateCommandPool();
#endif
    }

#if CUDA_PLATFORM
    bool VulkanGraphicsDevice::InitCudaContext()
    {
        if (!VulkanUtility::LoadInstanceFunctions(m_Instance.instance))
            return false;
        if (!VulkanUtility::LoadDeviceFunctions(m_Instance.device))
            return false;
        CUresult result = m_cudaContext.Init(m_Instance.instance, m_Instance.physicalDevice);
        if (CUDA_SUCCESS != result)
            return false;
        return true;
    }
#endif

    void VulkanGraphicsDevice::ShutdownV()
    {
#if CUDA_PLATFORM
        m_cudaContext.Shutdown();
#endif
#if VULKAN_USE_CRS
        if (m_fence)
        {
            vkDestroyFence(m_Instance.device, m_fence, nullptr);
            m_fence = VK_NULL_HANDLE;
        }
        if (m_commandBuffer)
        {
            vkFreeCommandBuffers(m_Instance.device, m_commandPool, 1, &m_commandBuffer);
            m_commandBuffer = VK_NULL_HANDLE;
        }
        VULKAN_SAFE_DESTROY_COMMAND_POOL(m_Instance.device, m_commandPool, VK_NULL_HANDLE)
#else
        VULKAN_SAFE_DESTROY_COMMAND_POOL(m_Instance.device, m_commandPool, VK_NULL_HANDLE)
#endif
    }

#if !VULKAN_USE_CRS
    static VkResult QueueSubmit(VkQueue queue, VkCommandBuffer commandBuffer, VkFence fence)
    {
        VkSubmitInfo submitInfo = {};
        submitInfo.sType = VK_STRUCTURE_TYPE_SUBMIT_INFO;
        submitInfo.commandBufferCount = 1;
        submitInfo.pCommandBuffers = &commandBuffer;
        return vkQueueSubmit(queue, 1, &submitInfo, fence);
    }

    VkResult VulkanGraphicsDevice::CreateCommandPool()
    {
        VkCommandPoolCreateInfo poolInfo = {};
        poolInfo.sType = VK_STRUCTURE_TYPE_COMMAND_POOL_CREATE_INFO;
        poolInfo.queueFamilyIndex = m_Instance.queueFamilyIndex;
        poolInfo.flags = VK_COMMAND_POOL_CREATE_RESET_COMMAND_BUFFER_BIT;

        return vkCreateCommandPool(m_Instance.device, &poolInfo, VK_NULL_HANDLE, &m_commandPool);
    }
#endif

    VkCommandBuffer VulkanGraphicsDevice::GetCommandBuffer(VulkanTexture2D* texture)
    {
#if VULKAN_USE_CRS
        if (m_unityVulkan)
        {
            UnityVulkanRecordingState recordingState;

            if (m_unityVulkan->CommandRecordingState(&recordingState, kUnityVulkanGraphicsQueueAccess_DontCare))
            {
                return recordingState.commandBuffer;
            }
            return nullptr;
        }
        else
        {
            // Only used for unit tests
            if (m_commandPool == VK_NULL_HANDLE)
            {
                VkCommandPoolCreateInfo poolInfo = {};
                poolInfo.sType = VK_STRUCTURE_TYPE_COMMAND_POOL_CREATE_INFO;
                poolInfo.queueFamilyIndex = m_Instance.queueFamilyIndex;
                poolInfo.flags = VK_COMMAND_POOL_CREATE_RESET_COMMAND_BUFFER_BIT;

                VULKAN_API_CALL_ARG(
                    vkCreateCommandPool(m_Instance.device, &poolInfo, VK_NULL_HANDLE, &m_commandPool), nullptr);
            }
            if (m_commandBuffer == VK_NULL_HANDLE)
            {
                // Create a command buffer to copy
                VkCommandBufferAllocateInfo allocInfo = {};
                allocInfo.sType = VK_STRUCTURE_TYPE_COMMAND_BUFFER_ALLOCATE_INFO;
                allocInfo.level = VK_COMMAND_BUFFER_LEVEL_PRIMARY;
                allocInfo.commandPool = m_commandPool;
                allocInfo.commandBufferCount = 1;

                VULKAN_API_CALL_ARG(vkAllocateCommandBuffers(m_Instance.device, &allocInfo, &m_commandBuffer), nullptr);
            }
            if (m_fence == VK_NULL_HANDLE)
            {
                VkFenceCreateInfo fenceInfo;
                fenceInfo.sType = VK_STRUCTURE_TYPE_FENCE_CREATE_INFO;
                fenceInfo.pNext = nullptr;
                fenceInfo.flags = 0;

                VULKAN_API_CALL_ARG(vkCreateFence(m_Instance.device, &fenceInfo, nullptr, &m_fence), nullptr);
            }

            VkCommandBufferBeginInfo beginInfo = {};
            beginInfo.sType = VK_STRUCTURE_TYPE_COMMAND_BUFFER_BEGIN_INFO;

            VULKAN_API_CALL_ARG(vkBeginCommandBuffer(m_commandBuffer, &beginInfo), nullptr);
            return m_commandBuffer;
        }
#else
        VkCommandBuffer commandBuffer = texture->GetCommandBuffer();
        if (!commandBuffer)
            return nullptr;

        VkCommandBufferBeginInfo beginInfo = {};
        beginInfo.sType = VK_STRUCTURE_TYPE_COMMAND_BUFFER_BEGIN_INFO;
        VkResult result = vkBeginCommandBuffer(commandBuffer, &beginInfo);

        if (result != VK_SUCCESS)
            return nullptr;
        return commandBuffer;
#endif
    }

    void VulkanGraphicsDevice::SubmitCommandBuffer(VulkanTexture2D* texture)
    {
#if VULKAN_USE_CRS
        if (m_unityVulkan)
            return;

        // Only used for unit tests
        if (m_commandBuffer)
        {
            VULKAN_API_CALL(vkEndCommandBuffer(m_commandBuffer));

            VkSubmitInfo submitInfo = {};
            submitInfo.sType = VK_STRUCTURE_TYPE_SUBMIT_INFO;
            submitInfo.commandBufferCount = 1;
            submitInfo.pCommandBuffers = &m_commandBuffer;

            VULKAN_API_CALL(vkQueueSubmit(m_Instance.graphicsQueue, 1, &submitInfo, m_fence));
            VULKAN_API_CALL(vkWaitForFences(m_Instance.device, 1, &m_fence, VK_TRUE, 200000000));

            VULKAN_API_CALL(vkResetFences(m_Instance.device, 1, &m_fence));
            VULKAN_API_CALL(vkResetCommandBuffer(m_commandBuffer, 0));
        }
#else
        VkCommandBuffer commandBuffer = texture->GetCommandBuffer();
        if (!commandBuffer)
            return;

        VkResult result = vkEndCommandBuffer(commandBuffer);
        if (result != VK_SUCCESS)
        {
            RTC_LOG(LS_ERROR) << "vkEndCommandBuffer failed. result:" << result;
            return;
        }

        result = QueueSubmit(m_Instance.graphicsQueue, commandBuffer, texture->GetFence());
        if (result != VK_SUCCESS)
        {
            RTC_LOG(LS_ERROR) << "vkQueueSubmit failed. result:" << result;
            return;
        }
#endif
    }

    // Returns null if failed
    ITexture2D* VulkanGraphicsDevice::CreateDefaultTextureV(
        const uint32_t w, const uint32_t h, UnityRenderingExtTextureFormat textureFormat)
    {
        std::unique_ptr<VulkanTexture2D> vulkanTexture = std::make_unique<VulkanTexture2D>(w, h);
        if (!vulkanTexture->Init(&m_Instance, m_commandPool))
        {
            RTC_LOG(LS_ERROR) << "VulkanTexture2D::Init failed.";
            return nullptr;
        }

        VkCommandBuffer commandBuffer = GetCommandBuffer(vulkanTexture.get());
        if (!commandBuffer)
        {
            RTC_LOG(LS_ERROR) << "GetCommandBuffer failed";
            return nullptr;
        }

        VulkanUtility::DoImageLayoutTransition(
            commandBuffer,
            vulkanTexture->GetUnityVulkanImage(),
            VK_IMAGE_LAYOUT_UNDEFINED,
            VK_PIPELINE_STAGE_TOP_OF_PIPE_BIT,
            VK_IMAGE_LAYOUT_TRANSFER_DST_OPTIMAL,
            VK_PIPELINE_STAGE_TRANSFER_BIT,
            true);

        SubmitCommandBuffer(vulkanTexture.get());
        return vulkanTexture.release();
    }

    ITexture2D*
    VulkanGraphicsDevice::CreateCPUReadTextureV(uint32_t w, uint32_t h, UnityRenderingExtTextureFormat textureFormat)
    {
        bool writable = false;
        std::unique_ptr<VulkanTexture2D> vulkanTexture = std::make_unique<VulkanTexture2D>(w, h);
        if (!vulkanTexture->InitStaging(&m_Instance, m_commandPool, writable, m_hasHostCachedMemory))
        {
            RTC_LOG(LS_ERROR) << "VulkanTexture2D::InitCpuRead failed.";
            return nullptr;
        }

        VkCommandBuffer commandBuffer = GetCommandBuffer(vulkanTexture.get());
        if (!commandBuffer)
        {
            RTC_LOG(LS_ERROR) << "GetCommandBuffer failed";
            return nullptr;
        }

        VulkanUtility::DoImageLayoutTransition(
            commandBuffer,
            vulkanTexture->GetUnityVulkanImage(),
            VK_IMAGE_LAYOUT_UNDEFINED,
            VK_PIPELINE_STAGE_TOP_OF_PIPE_BIT,
            (writable ? VK_IMAGE_LAYOUT_TRANSFER_SRC_OPTIMAL : VK_IMAGE_LAYOUT_TRANSFER_DST_OPTIMAL),
            VK_PIPELINE_STAGE_TRANSFER_BIT,
            true);

        SubmitCommandBuffer(vulkanTexture.get());
        return vulkanTexture.release();
    }

    bool VulkanGraphicsDevice::LookupUnityVulkanImage(
        VkImage src, UnityVulkanImage* outImage, bool* setLayout, VkImageLayout layout)
    {
        VulkanTexture2D* vulkanTexture = static_cast<VulkanTexture2D*>(ITexture2D::GetTexturePtr(src));

        *setLayout = false;
        if (vulkanTexture != nullptr)
        {
            *outImage = *vulkanTexture->GetUnityVulkanImage();
            // We need to do this after CommandRecordingState and AccessTexture
            *setLayout = true;

#if VULKAN_USE_CRS
            if (layout == VK_IMAGE_LAYOUT_TRANSFER_DST_OPTIMAL && m_unityVulkan)
                vulkanTexture->currentFrameNumber = m_LastState.currentFrameNumber;
#endif
            return true;
        }

        VkAccessFlagBits access = (VkAccessFlagBits)0; // VK_ACCESS_NONE is VK_VERSION_1_3
        if (layout == VK_IMAGE_LAYOUT_TRANSFER_DST_OPTIMAL)
            access = VK_ACCESS_TRANSFER_WRITE_BIT;
        else if (layout == VK_IMAGE_LAYOUT_TRANSFER_SRC_OPTIMAL)
            access = VK_ACCESS_TRANSFER_READ_BIT;

        // IUnityGraphicsVulkan bugs:
        //
        // UnityVulkanImage.layout is not filled by the plugin interface.
        //
        // Calling AccessTexture SHOULD be enough, but current Unity versions have a bug where pipeline barriers
        // are not flushed/restored. This means even if you do AccessTexture before calling out CommandRecordingState,
        // the validation layer will complain during custom call to vkCmdCopyImage.
        //
        // To avoid Vulkan validation layer from complaining, you could call DoImageLayoutTransition after
        // AccessTexture and restore the state after CopyImage, but it seems to prevent copy texture from
        // working with GTX 1080 while it works ok with RTX series.
        //
        // AccessTexture flushing pipeline barriers should be fixed with Unity 2022.1+
        if (m_unityVulkan &&
            m_unityVulkan->AccessTexture(
                (void*)src,
                UnityVulkanWholeImage,
                layout,
                VK_PIPELINE_STAGE_TRANSFER_BIT,
                access,
                kUnityVulkanResourceAccess_PipelineBarrier,
                outImage))
        {
            return true;
        }
        return false;
    }

    bool VulkanGraphicsDevice::CopyTexture(void* dst, void* src, uint32_t width, uint32_t height)
    {
        if (!dst || !src)
            return false;

        VkImage nativeDst = reinterpret_cast<VkImage>(dst);
        VkImage nativeSrc = reinterpret_cast<VkImage>(src);

        if (nativeDst == nativeSrc)
            return false;

        UnityVulkanImage nativeDstUnity;
        bool setDstLayout = false;
        if (!LookupUnityVulkanImage(nativeDst, &nativeDstUnity, &setDstLayout, VK_IMAGE_LAYOUT_TRANSFER_DST_OPTIMAL))
            return false;

        UnityVulkanImage nativeSrcUnity;
        bool setSrcLayout = false;
        if (!LookupUnityVulkanImage(nativeSrc, &nativeSrcUnity, &setSrcLayout, VK_IMAGE_LAYOUT_TRANSFER_SRC_OPTIMAL))
            return false;

        VulkanTexture2D* vulkanTexture = nullptr;
#if !VULKAN_USE_CRS
        vulkanTexture = static_cast<VulkanTexture2D*>(ITexture2D::GetTexturePtr(nativeDst));
        if (!vulkanTexture)
            vulkanTexture = static_cast<VulkanTexture2D*>(ITexture2D::GetTexturePtr(nativeSrc));
        if (!vulkanTexture)
            return false;
#endif

        VkCommandBuffer commandBuffer = GetCommandBuffer(vulkanTexture);
        if (!commandBuffer)
        {
            RTC_LOG(LS_ERROR) << "GetCommandBuffer failed";
            return false;
        }

        if (setDstLayout)
        {
            VulkanUtility::DoImageLayoutTransition(
                commandBuffer,
                &nativeDstUnity,
                nativeDstUnity.layout,
                VK_PIPELINE_STAGE_TRANSFER_BIT,
                VK_IMAGE_LAYOUT_TRANSFER_DST_OPTIMAL,
                VK_PIPELINE_STAGE_TRANSFER_BIT);
        }
        if (setSrcLayout)
        {
            VulkanUtility::DoImageLayoutTransition(
                commandBuffer,
                &nativeSrcUnity,
                nativeSrcUnity.layout,
                VK_PIPELINE_STAGE_TRANSFER_BIT,
                VK_IMAGE_LAYOUT_TRANSFER_SRC_OPTIMAL,
                VK_PIPELINE_STAGE_TRANSFER_BIT);
        }
        VulkanUtility::CopyImage(commandBuffer, nativeSrcUnity.image, nativeDstUnity.image, width, height);
        SubmitCommandBuffer(vulkanTexture);
        return true;
    }

    bool VulkanGraphicsDevice::CopyResourceV(ITexture2D* dest, ITexture2D* src)
    {
        VulkanTexture2D* destTexture = reinterpret_cast<VulkanTexture2D*>(dest);
        VulkanTexture2D* srcTexture = reinterpret_cast<VulkanTexture2D*>(src);

        if (!destTexture || !srcTexture)
            return false;

        if (destTexture == srcTexture)
            return false;

        return CopyTexture(
            destTexture->GetImage(), srcTexture->GetImage(), destTexture->GetWidth(), destTexture->GetHeight());
    }

    bool VulkanGraphicsDevice::CopyResourceFromNativeV(ITexture2D* dest, void* nativeTexturePtr)
    {
        VulkanTexture2D* destTexture = reinterpret_cast<VulkanTexture2D*>(dest);
        VkImage nativeSrc = reinterpret_cast<VkImage>(nativeTexturePtr);

        return CopyTexture(destTexture->GetImage(), nativeSrc, destTexture->GetWidth(), destTexture->GetHeight());
    }

    rtc::scoped_refptr<webrtc::I420Buffer> VulkanGraphicsDevice::ConvertRGBToI420(ITexture2D* tex)
    {
        VulkanTexture2D* vulkanTexture = static_cast<VulkanTexture2D*>(tex);
        const int32_t width = static_cast<int32_t>(tex->GetWidth());
        const int32_t height = static_cast<int32_t>(tex->GetHeight());
        const int32_t rowPitch = static_cast<int32_t>(vulkanTexture->GetPitch());

        VkDeviceMemory textureImageMemory = vulkanTexture->GetTextureImageMemory();
        void* data = nullptr;

        if (vkMapMemory(m_Instance.device, textureImageMemory, 0, VK_WHOLE_SIZE, 0, &data) != VK_SUCCESS)
            return nullptr;

        // convert format to i420
        rtc::scoped_refptr<webrtc::I420Buffer> i420Buffer = webrtc::I420Buffer::Create(width, height);
        libyuv::ARGBToI420(
            (const uint8_t*)data,
            rowPitch,
            i420Buffer->MutableDataY(),
            i420Buffer->StrideY(),
            i420Buffer->MutableDataU(),
            i420Buffer->StrideU(),
            i420Buffer->MutableDataV(),
            i420Buffer->StrideV(),
            width,
            height);
        vkUnmapMemory(m_Instance.device, textureImageMemory);
        return i420Buffer;
    }

    std::unique_ptr<GpuMemoryBufferHandle> VulkanGraphicsDevice::Map(ITexture2D* texture)
    {
#if CUDA_PLATFORM
        if (!IsCudaSupport())
            return nullptr;

        VulkanTexture2D* vulkanTexture = static_cast<VulkanTexture2D*>(texture);
        void* exportHandle = VulkanUtility::GetExportHandle(m_Instance.device, vulkanTexture->GetTextureImageMemory());

        if (!exportHandle)
        {
            RTC_LOG(LS_ERROR) << "cannot get export handle";
            return nullptr;
        }
        size_t memorySize = vulkanTexture->GetTextureImageMemorySize();
        Size size(static_cast<int>(texture->GetWidth()), static_cast<int>(texture->GetHeight()));
        return GpuMemoryBufferCudaHandle::CreateHandle(GetCUcontext(), exportHandle, memorySize, size);
#else
        return nullptr;
#endif
    }

    bool VulkanGraphicsDevice::WaitIdleForTest()
    {
        VkResult result = vkQueueWaitIdle(m_Instance.graphicsQueue);
        if (result != VK_SUCCESS)
        {
            RTC_LOG(LS_INFO) << "vkQueueWaitIdle failed. result:" << result;
            return false;
        }
        return true;
    }

    bool VulkanGraphicsDevice::UpdateState()
    {
#if VULKAN_USE_CRS
        if (m_unityVulkan)
        {
            std::unique_lock<std::mutex> lock(m_LastStateMtx);
            if (m_unityVulkan->CommandRecordingState(&m_LastState, kUnityVulkanGraphicsQueueAccess_DontCare))
            {
                m_LastStateCond.notify_all();
                return true;
            }
        }
        m_LastState = {};
        return false;
#else
        return true;
#endif
    }

    bool VulkanGraphicsDevice::WaitSync(const ITexture2D* texture, uint64_t nsTimeout)
    {
#if VULKAN_USE_CRS
        if (!m_unityVulkan)
            return true;

        const VulkanTexture2D* vulkanTexture = static_cast<const VulkanTexture2D*>(texture);
        std::unique_lock<std::mutex> lock(m_LastStateMtx);

        bool ret = m_LastStateCond.wait_until(
            lock, std::chrono::system_clock::now() + std::chrono::nanoseconds(nsTimeout), [vulkanTexture, this] {
                return vulkanTexture->currentFrameNumber <= m_LastState.safeFrameNumber;
            });
        return ret;
#else
        const VulkanTexture2D* vulkanTexture = static_cast<const VulkanTexture2D*>(texture);
        VkFence fence = vulkanTexture->GetFence();
        VkResult result = vkWaitForFences(m_Instance.device, 1, &fence, true, nsTimeout);
        if (result != VK_SUCCESS)
        {
            RTC_LOG(LS_INFO) << "vkWaitForFences failed. result:" << result;
            return false;
        }
        return true;
#endif
    }

    bool VulkanGraphicsDevice::ResetSync(const ITexture2D* texture)
    {
#if VULKAN_USE_CRS
        const VulkanTexture2D* vulkanTexture = static_cast<const VulkanTexture2D*>(texture);
        vulkanTexture->ResetFrameNumber();
        return true;
#else
        const VulkanTexture2D* vulkanTexture = static_cast<const VulkanTexture2D*>(texture);
        VkCommandBuffer commandBuffer = vulkanTexture->GetCommandBuffer();
        VkFence fence = vulkanTexture->GetFence();

        VkResult result = vkGetFenceStatus(m_Instance.device, fence);
        if (result != VK_SUCCESS)
        {
            RTC_LOG(LS_INFO) << "vkGetFenceStatus failed. result:" << result;
            return false;
        }
        result = vkResetFences(m_Instance.device, 1, &fence);
        if (result != VK_SUCCESS)
        {
            RTC_LOG(LS_INFO) << "vkResetFences failed. result:" << result;
            return false;
        }
        result = vkResetCommandBuffer(commandBuffer, 0);
        if (result != VK_SUCCESS)
        {
            RTC_LOG(LS_INFO) << "vkResetCommandBuffer failed. result:" << result;
            return false;
        }
        return true;
#endif
    }

} // end namespace webrtc
} // end namespace unity
