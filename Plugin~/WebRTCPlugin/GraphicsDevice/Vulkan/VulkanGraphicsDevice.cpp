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
        const VkInstance instance,
        const VkPhysicalDevice physicalDevice,
        const VkDevice device,
        const VkQueue graphicsQueue,
        const uint32_t queueFamilyIndex,
        UnityGfxRenderer renderer,
        ProfilerMarkerFactory* profiler)
        : IGraphicsDevice(renderer, profiler)
        , m_unityVulkan(unityVulkan)
        , m_physicalDevice(physicalDevice)
        , m_device(device)
        , m_graphicsQueue(graphicsQueue)
        , m_commandPool(nullptr)
        , m_queueFamilyIndex(queueFamilyIndex)
        , m_allocator(nullptr)
#if CUDA_PLATFORM
        , m_instance(instance)
        , m_isCudaSupport(false)
#endif
    {
        if (profiler)
            m_maker = profiler->CreateMarker(
                "VulkanGraphicsDevice.CopyImage", kUnityProfilerCategoryOther, kUnityProfilerMarkerFlagDefault, 0);
    }

    //---------------------------------------------------------------------------------------------------------------------
    bool VulkanGraphicsDevice::InitV()
    {
#if CUDA_PLATFORM
        m_isCudaSupport = InitCudaContext();
#endif
        return VK_SUCCESS == CreateCommandPool();
    }

#if CUDA_PLATFORM
    bool VulkanGraphicsDevice::InitCudaContext()
    {
        if (!VulkanUtility::LoadInstanceFunctions(m_instance))
            return false;
        if (!VulkanUtility::LoadDeviceFunctions(m_device))
            return false;
        CUresult result = m_cudaContext.Init(m_instance, m_physicalDevice);
        if (CUDA_SUCCESS != result)
            return false;
        return true;
    }
#endif

    //---------------------------------------------------------------------------------------------------------------------

    void VulkanGraphicsDevice::ShutdownV()
    {
#if CUDA_PLATFORM
        m_cudaContext.Shutdown();
#endif
        VULKAN_SAFE_DESTROY_COMMAND_POOL(m_device, m_commandPool, m_allocator)
    }

    //---------------------------------------------------------------------------------------------------------------------

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

    static VkResult DoImageLayoutTransition(
        const VkCommandBuffer commandBuffer,
        const VkDevice device,
        const VkCommandPool commandPool,
        const VkQueue queue,
        const VkImage image,
        VkFormat format,
        const VkImageLayout oldLayout,
        const VkPipelineStageFlags oldStage,
        const VkImageLayout newLayout,
        const VkPipelineStageFlags newStage)
    {
        if (commandBuffer)
        {
            return VulkanUtility::DoImageLayoutTransition(
                commandBuffer, image, format, oldLayout, oldStage, newLayout, newStage);
        }
        else
        {
            return VulkanUtility::DoImageLayoutTransition(
                device, commandPool, queue, image, format, oldLayout, oldStage, newLayout, newStage);
        }
    }

    static VkResult CopyImage(
        const VkCommandBuffer commandBuffer,
        const VkDevice device,
        const VkCommandPool commandPool,
        const VkQueue queue,
        const VkImage srcImage,
        const VkImage dstImage,
        const uint32_t width,
        const uint32_t height)
    {
        if (commandBuffer)
        {
            return VulkanUtility::CopyImage(commandBuffer, srcImage, dstImage, width, height);
        }
        else
        {
            return VulkanUtility::CopyImage(device, commandPool, queue, srcImage, dstImage, width, height);
        }
    }

    VkCommandBuffer VulkanGraphicsDevice::GetCurrentCommandBuffer()
    {
        if (!m_unityVulkan)
            return nullptr;

        UnityVulkanRecordingState recordingState;
        if (!m_unityVulkan->CommandRecordingState(&recordingState, kUnityVulkanGraphicsQueueAccess_DontCare))
            return nullptr;
        return recordingState.commandBuffer;
    }

    // Returns null if failed
    ITexture2D* VulkanGraphicsDevice::CreateDefaultTextureV(
        const uint32_t w, const uint32_t h, UnityRenderingExtTextureFormat textureFormat)
    {
        VkCommandBuffer commandBuffer = GetCurrentCommandBuffer();
        std::unique_ptr<VulkanTexture2D> vulkanTexture = std::make_unique<VulkanTexture2D>(w, h);
        if (!vulkanTexture->Init(m_physicalDevice, m_device))
        {
            RTC_LOG(LS_ERROR) << "VulkanTexture2D::Init failed.";
            return nullptr;
        }

        // Transition to dest
        VkResult result = DoImageLayoutTransition(
            commandBuffer,
            m_device,
            m_commandPool,
            m_graphicsQueue,
            vulkanTexture->GetImage(),
            vulkanTexture->GetTextureFormat(),
            VK_IMAGE_LAYOUT_UNDEFINED,
            VK_PIPELINE_STAGE_TOP_OF_PIPE_BIT,
            VK_IMAGE_LAYOUT_TRANSFER_DST_OPTIMAL,
            VK_PIPELINE_STAGE_TRANSFER_BIT);

        if (result != VK_SUCCESS)
        {
            RTC_LOG(LS_ERROR) << "DoImageLayoutTransition failed. result:" << result;
            return nullptr;
        }

        return vulkanTexture.release();
    }

    //---------------------------------------------------------------------------------------------------------------------
    ITexture2D*
    VulkanGraphicsDevice::CreateCPUReadTextureV(uint32_t w, uint32_t h, UnityRenderingExtTextureFormat textureFormat)
    {
        VkCommandBuffer commandBuffer = GetCurrentCommandBuffer();
        std::unique_ptr<VulkanTexture2D> vulkanTexture = std::make_unique<VulkanTexture2D>(w, h);
        if (!vulkanTexture->InitCpuRead(m_physicalDevice, m_device))
        {
            return nullptr;
        }

        // Transition to dest
        VkResult result = DoImageLayoutTransition(
            commandBuffer,
            m_device,
            m_commandPool,
            m_graphicsQueue,
            vulkanTexture->GetImage(),
            vulkanTexture->GetTextureFormat(),
            VK_IMAGE_LAYOUT_UNDEFINED,
            VK_PIPELINE_STAGE_TOP_OF_PIPE_BIT,
            VK_IMAGE_LAYOUT_TRANSFER_DST_OPTIMAL,
            VK_PIPELINE_STAGE_TRANSFER_BIT);
        if (result != VK_SUCCESS)
        {
            RTC_LOG(LS_ERROR) << "DoImageLayoutTransition failed. result:" << result;
            return nullptr;
        }
        return vulkanTexture.release();
    }

    //---------------------------------------------------------------------------------------------------------------------
    bool VulkanGraphicsDevice::CopyResourceV(ITexture2D* dest, ITexture2D* src)
    {
        VkCommandBuffer commandBuffer = GetCurrentCommandBuffer();
        VulkanTexture2D* destTexture = reinterpret_cast<VulkanTexture2D*>(dest);
        VulkanTexture2D* srcTexture = reinterpret_cast<VulkanTexture2D*>(src);
        if (destTexture == srcTexture)
            return false;
        if (destTexture == nullptr || srcTexture == nullptr)
            return false;

        // Transition the src texture layout.
        VkResult result = DoImageLayoutTransition(
            commandBuffer,
            m_device,
            m_commandPool,
            m_graphicsQueue,
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

        result = CopyImage(
            commandBuffer,
            m_device,
            m_commandPool,
            m_graphicsQueue,
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
        result = DoImageLayoutTransition(
            commandBuffer,
            m_device,
            m_commandPool,
            m_graphicsQueue,
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

        return true;
    }

    //---------------------------------------------------------------------------------------------------------------------
    bool VulkanGraphicsDevice::CopyResourceFromNativeV(ITexture2D* dest, void* nativeTexturePtr)
    {
        if (nullptr == dest || nullptr == nativeTexturePtr)
            return false;

        VkCommandBuffer commandBuffer = GetCurrentCommandBuffer();
        VulkanTexture2D* destTexture = reinterpret_cast<VulkanTexture2D*>(dest);
        UnityVulkanImage* unityVulkanImage = static_cast<UnityVulkanImage*>(nativeTexturePtr);

        // Transition the src texture layout.
        VkResult result = DoImageLayoutTransition(
            commandBuffer,
            m_device,
            m_commandPool,
            m_graphicsQueue,
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

        VkImage image = unityVulkanImage->image;
        if (destTexture->GetImage() == image)
            return false;

        {
            std::unique_ptr<const ScopedProfiler> profiler;
            if (m_profiler)
                profiler = m_profiler->CreateScopedProfiler(*m_maker);

            // The layouts of All VulkanTexture2D should be VK_IMAGE_LAYOUT_TRANSFER_DST_OPTIMAL,
            // so no transition for destTex
            result = CopyImage(
                commandBuffer,
                m_device,
                m_commandPool,
                m_graphicsQueue,
                image,
                destTexture->GetImage(),
                destTexture->GetWidth(),
                destTexture->GetHeight());
            if (result != VK_SUCCESS)
            {
                RTC_LOG(LS_ERROR) << "CopyImage failed. result:" << result;
                return false;
            }
        }

        return true;
    }

    //---------------------------------------------------------------------------------------------------------------------
    VkResult VulkanGraphicsDevice::CreateCommandPool()
    {
        VkCommandPoolCreateInfo poolInfo = {};
        poolInfo.sType = VK_STRUCTURE_TYPE_COMMAND_POOL_CREATE_INFO;
        poolInfo.queueFamilyIndex = m_queueFamilyIndex;
        poolInfo.flags = 0;

        return vkCreateCommandPool(m_device, &poolInfo, m_allocator, &m_commandPool);
    }

    //---------------------------------------------------------------------------------------------------------------------
    rtc::scoped_refptr<webrtc::I420Buffer> VulkanGraphicsDevice::ConvertRGBToI420(ITexture2D* tex)
    {
        VulkanTexture2D* vulkanTexture = static_cast<VulkanTexture2D*>(tex);
        const int32_t width = static_cast<int32_t>(tex->GetWidth());
        const int32_t height = static_cast<int32_t>(tex->GetHeight());
        const VkDeviceMemory dstImageMemory = vulkanTexture->GetTextureImageMemory();
        VkImageSubresource subresource { VK_IMAGE_ASPECT_COLOR_BIT, 0, 0 };
        VkSubresourceLayout subresourceLayout;
        vkGetImageSubresourceLayout(m_device, vulkanTexture->GetImage(), &subresource, &subresourceLayout);
        const int32_t rowPitch = static_cast<int32_t>(subresourceLayout.rowPitch);

        void* data;
        std::vector<uint8_t> dst;
        dst.resize(vulkanTexture->GetTextureImageMemorySize());
        const VkResult result = vkMapMemory(m_device, dstImageMemory, 0, VK_WHOLE_SIZE, 0, &data);
        if (result != VK_SUCCESS)
        {
            return nullptr;
        }
        std::memcpy(static_cast<void*>(dst.data()), data, dst.size());

        vkUnmapMemory(m_device, dstImageMemory);

        // convert format to i420
        rtc::scoped_refptr<webrtc::I420Buffer> i420Buffer = webrtc::I420Buffer::Create(width, height);
        libyuv::ARGBToI420(
            dst.data(),
            rowPitch,
            i420Buffer->MutableDataY(),
            i420Buffer->StrideY(),
            i420Buffer->MutableDataU(),
            i420Buffer->StrideU(),
            i420Buffer->MutableDataV(),
            i420Buffer->StrideV(),
            width,
            height);

        return i420Buffer;
    }

    std::unique_ptr<GpuMemoryBufferHandle> VulkanGraphicsDevice::Map(ITexture2D* texture)
    {
#if CUDA_PLATFORM
        if (!IsCudaSupport())
            return nullptr;

        // set context on the thread.
        cuCtxPushCurrent(GetCUcontext());

        VulkanTexture2D* vulkanTexture = static_cast<VulkanTexture2D*>(texture);

        void* exportHandle = VulkanUtility::GetExportHandle(m_device, vulkanTexture->GetTextureImageMemory());

        if (!exportHandle)
        {
            RTC_LOG(LS_ERROR) << "cannot get export handle";
            throw;
        }

        CUDA_EXTERNAL_MEMORY_HANDLE_DESC memDesc = {};
#ifndef _WIN32
        memDesc.type = CU_EXTERNAL_MEMORY_HANDLE_TYPE_OPAQUE_FD;
#else
        memDesc.type = CU_EXTERNAL_MEMORY_HANDLE_TYPE_OPAQUE_WIN32;
#endif
        memDesc.handle.fd = static_cast<int>(reinterpret_cast<uintptr_t>(exportHandle));
        memDesc.size = vulkanTexture->GetTextureImageMemorySize();

        CUresult result;
        CUexternalMemory externalMemory;
        result = cuImportExternalMemory(&externalMemory, &memDesc);
        if (result != CUDA_SUCCESS)
        {
            RTC_LOG(LS_ERROR) << "cuImportExternalMemory error:" << result;
            throw;
        }

        const VkExtent2D extent = { texture->GetWidth(), texture->GetHeight() };
        CUDA_ARRAY3D_DESCRIPTOR arrayDesc = {};
        arrayDesc.Width = extent.width;
        arrayDesc.Height = extent.height;
        arrayDesc.Depth = 0; /* CUDA 2D arrays are defined to have depth 0 */
        arrayDesc.Format = CU_AD_FORMAT_UNSIGNED_INT32;
        arrayDesc.NumChannels = 1;
        arrayDesc.Flags = CUDA_ARRAY3D_SURFACE_LDST | CUDA_ARRAY3D_COLOR_ATTACHMENT;

        CUDA_EXTERNAL_MEMORY_MIPMAPPED_ARRAY_DESC mipmapArrayDesc = {};
        mipmapArrayDesc.arrayDesc = arrayDesc;
        mipmapArrayDesc.numLevels = 1;

        CUmipmappedArray mipmappedArray;
        result = cuExternalMemoryGetMappedMipmappedArray(&mipmappedArray, externalMemory, &mipmapArrayDesc);
        if (result != CUDA_SUCCESS)
        {
            RTC_LOG(LS_ERROR) << "cuExternalMemoryGetMappedMipmappedArray error:" << result;
            throw;
        }

        CUarray array;
        result = cuMipmappedArrayGetLevel(&array, mipmappedArray, 0);
        if (result != CUDA_SUCCESS)
        {
            RTC_LOG(LS_ERROR) << "cuMipmappedArrayGetLevel error:" << result;
            throw;
        }

        cuCtxPopCurrent(nullptr);

        std::unique_ptr<GpuMemoryBufferCudaHandle> handle = std::make_unique<GpuMemoryBufferCudaHandle>();
        handle->context = GetCUcontext();
        handle->mappedArray = array;
        handle->externalMemory = externalMemory;
        return std::move(handle);
#else
        return nullptr;
#endif
    }

} // end namespace webrtc
} // end namespace unity
