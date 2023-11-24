#include "pch.h"

#include <third_party/libyuv/include/libyuv/convert.h>

#include "GraphicsDevice/GraphicsUtility.h"
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
        , m_commandBuffer(VK_NULL_HANDLE)
        , m_fence(VK_NULL_HANDLE)
#if CUDA_PLATFORM
        , m_isCudaSupport(false)
#endif
    {
        if (profiler)
            m_maker = profiler->CreateMarker(
                "VulkanGraphicsDevice.CopyImage", kUnityProfilerCategoryOther, kUnityProfilerMarkerFlagDefault, 0);
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
        return true;
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
    }

    std::unique_ptr<UnityVulkanImage> VulkanGraphicsDevice::AccessTexture(void* ptr) const
    {
        // cannot do resource uploads inside renderpass
        m_unityVulkan->EnsureOutsideRenderPass();

        std::unique_ptr<UnityVulkanImage> unityVulkanImage = std::make_unique<UnityVulkanImage>();

        VkImageSubresource subResource { VK_IMAGE_ASPECT_COLOR_BIT, 0, 0 };
        if (!m_unityVulkan->AccessTexture(
                ptr,
                &subResource,
                VK_IMAGE_LAYOUT_TRANSFER_SRC_OPTIMAL,
                VK_PIPELINE_STAGE_TRANSFER_BIT,
                VK_ACCESS_TRANSFER_READ_BIT,
                kUnityVulkanResourceAccess_PipelineBarrier,
                unityVulkanImage.get()))
        {
            return nullptr;
        }
        return unityVulkanImage;
    }

    VkCommandBuffer VulkanGraphicsDevice::GetCommandBuffer()
    {
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
    }

    void VulkanGraphicsDevice::SubmitCommandBuffer()
    {
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
    }

    // Returns null if failed
    ITexture2D* VulkanGraphicsDevice::CreateDefaultTextureV(
        const uint32_t w, const uint32_t h, UnityRenderingExtTextureFormat textureFormat)
    {
        std::unique_ptr<VulkanTexture2D> vulkanTexture = std::make_unique<VulkanTexture2D>(w, h);
        if (!vulkanTexture->Init(&m_Instance))
        {
            RTC_LOG(LS_ERROR) << "VulkanTexture2D::Init failed.";
            return nullptr;
        }
        return vulkanTexture.release();
    }

    ITexture2D*
    VulkanGraphicsDevice::CreateCPUReadTextureV(uint32_t w, uint32_t h, UnityRenderingExtTextureFormat textureFormat)
    {
        bool writable = false;
        std::unique_ptr<VulkanTexture2D> vulkanTexture = std::make_unique<VulkanTexture2D>(w, h);
        if (!vulkanTexture->InitStaging(&m_Instance, writable, m_hasHostCachedMemory))
        {
            RTC_LOG(LS_ERROR) << "VulkanTexture2D::InitCpuRead failed.";
            return nullptr;
        }
        return vulkanTexture.release();
    }

    bool VulkanGraphicsDevice::CopyResourceV(ITexture2D* dest, ITexture2D* src)
    {
        VulkanTexture2D* destTexture = reinterpret_cast<VulkanTexture2D*>(dest);
        VulkanTexture2D* srcTexture = reinterpret_cast<VulkanTexture2D*>(src);
        if (destTexture == srcTexture)
            return false;
        if (!destTexture || !srcTexture)
            return false;

        VkCommandBuffer commandBuffer = GetCommandBuffer();
        if (!commandBuffer)
        {
            RTC_LOG(LS_ERROR) << "GetCommandBuffer failed";
            return false;
        }

        // Transition the src texture layout.
        VkResult result = VulkanUtility::DoImageLayoutTransition(
            commandBuffer,
            srcTexture->GetImage(),
            srcTexture->GetTextureFormat(),
            VK_IMAGE_LAYOUT_TRANSFER_DST_OPTIMAL,
            VK_PIPELINE_STAGE_TRANSFER_BIT,
            VK_IMAGE_LAYOUT_TRANSFER_SRC_OPTIMAL,
            VK_PIPELINE_STAGE_TRANSFER_BIT);
        if (result != VK_SUCCESS)
        {
            RTC_LOG(LS_ERROR) << "DoImageLayoutTransition failed. result:" << result;
            return false;
        }

        // Transition the dst texture layout.
        result = VulkanUtility::DoImageLayoutTransition(
            commandBuffer,
            destTexture->GetImage(),
            destTexture->GetTextureFormat(),
            VK_IMAGE_LAYOUT_UNDEFINED,
            VK_PIPELINE_STAGE_TOP_OF_PIPE_BIT,
            VK_IMAGE_LAYOUT_TRANSFER_DST_OPTIMAL,
            VK_PIPELINE_STAGE_TRANSFER_BIT);
        if (result != VK_SUCCESS)
        {
            RTC_LOG(LS_ERROR) << "DoImageLayoutTransition failed. result:" << result;
            return false;
        }

        result = VulkanUtility::CopyImage(
            commandBuffer,
            srcTexture->GetImage(),
            destTexture->GetImage(),
            destTexture->GetWidth(),
            destTexture->GetHeight());
        if (result != VK_SUCCESS)
        {
            RTC_LOG(LS_ERROR) << "CopyImage failed. result:" << result;
            return false;
        }
        // transition the src texture layout back to VK_IMAGE_LAYOUT_TRANSFER_DST_OPTIMAL
        result = VulkanUtility::DoImageLayoutTransition(
            commandBuffer,
            srcTexture->GetImage(),
            srcTexture->GetTextureFormat(),
            VK_IMAGE_LAYOUT_TRANSFER_SRC_OPTIMAL,
            VK_PIPELINE_STAGE_TRANSFER_BIT,
            VK_IMAGE_LAYOUT_TRANSFER_DST_OPTIMAL,
            VK_PIPELINE_STAGE_TRANSFER_BIT);
        if (result != VK_SUCCESS)
        {
            RTC_LOG(LS_ERROR) << "DoImageLayoutTransition failed. result:" << result;
            return false;
        }

        destTexture->currentFrameNumber = m_LastState.currentFrameNumber;
        SubmitCommandBuffer();
        return true;
    }

    bool VulkanGraphicsDevice::CopyResourceFromNativeV(ITexture2D* dest, void* nativeTexturePtr)
    {
        if (!nativeTexturePtr)
        {
            RTC_LOG(LS_ERROR) << "nativeTexturePtr is nullptr.";
            return false;
        }

        if (!dest)
        {
            RTC_LOG(LS_ERROR) << "dest is nullptr.";
            return false;
        }
        VulkanTexture2D* destTexture = reinterpret_cast<VulkanTexture2D*>(dest);
        UnityVulkanImage* unityVulkanImage = static_cast<UnityVulkanImage*>(nativeTexturePtr);

        VkCommandBuffer commandBuffer = GetCommandBuffer();
        if (!commandBuffer)
        {
            RTC_LOG(LS_ERROR) << "GetCommandBuffer failed";
            return false;
        }

        // Transition the src texture layout.
        VkResult result = VulkanUtility::DoImageLayoutTransition(
            commandBuffer,
            unityVulkanImage->image,
            unityVulkanImage->format,
            VK_IMAGE_LAYOUT_TRANSFER_DST_OPTIMAL,
            VK_PIPELINE_STAGE_TRANSFER_BIT,
            VK_IMAGE_LAYOUT_TRANSFER_SRC_OPTIMAL,
            VK_PIPELINE_STAGE_TRANSFER_BIT);
        if (result != VK_SUCCESS)
        {
            RTC_LOG(LS_ERROR) << "DoImageLayoutTransition failed. result:" << result;
            return false;
        }

        // Transition the dst texture layout.
        result = VulkanUtility::DoImageLayoutTransition(
            commandBuffer,
            destTexture->GetImage(),
            destTexture->GetTextureFormat(),
            VK_IMAGE_LAYOUT_UNDEFINED,
            VK_PIPELINE_STAGE_TOP_OF_PIPE_BIT,
            VK_IMAGE_LAYOUT_TRANSFER_DST_OPTIMAL,
            VK_PIPELINE_STAGE_TRANSFER_BIT);
        if (result != VK_SUCCESS)
        {
            RTC_LOG(LS_ERROR) << "DoImageLayoutTransition failed. result:" << result;
            return false;
        }

        VkImage image = unityVulkanImage->image;
        if (destTexture->GetImage() == image)
            return false;
        {
            std::unique_ptr<const ScopedProfiler> profiler;
            if (m_profiler)
                profiler = m_profiler->CreateScopedProfiler(*m_maker);

            // The layouts of All VulkanTexture2D should be VK_IMAGE_LAYOUT_TRANSFER_DST_OPTIMAL,
            // so no transition for destTex
            VkResult result = VulkanUtility::CopyImage(
                commandBuffer, image, destTexture->GetImage(), destTexture->GetWidth(), destTexture->GetHeight());
            if (result != VK_SUCCESS)
            {
                RTC_LOG(LS_ERROR) << "CopyImage failed. result:" << result;
                return false;
            }
        }

        destTexture->currentFrameNumber = m_LastState.currentFrameNumber;
        SubmitCommandBuffer();
        return true;
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
        {
            RTC_LOG(LS_INFO) << "vkMapMemory failed.";
            return nullptr;
        }

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
    }

    bool VulkanGraphicsDevice::WaitSync(const ITexture2D* texture)
    {
        if (!m_unityVulkan)
            return true;

        const VulkanTexture2D* vulkanTexture = static_cast<const VulkanTexture2D*>(texture);
        std::unique_lock<std::mutex> lock(m_LastStateMtx);

        bool ret =
            m_LastStateCond.wait_until(lock, std::chrono::system_clock::now() + m_syncTimeout, [vulkanTexture, this] {
                return vulkanTexture->currentFrameNumber <= m_LastState.safeFrameNumber;
            });
        return ret;
    }

    bool VulkanGraphicsDevice::ResetSync(const ITexture2D* texture)
    {
        const VulkanTexture2D* vulkanTexture = static_cast<const VulkanTexture2D*>(texture);
        vulkanTexture->ResetFrameNumber();
        return true;
    }

} // end namespace webrtc
} // end namespace unity
