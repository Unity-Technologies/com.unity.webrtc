#include "pch.h"

#include <system_wrappers/include/sleep.h>
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
    static VulkanGraphicsDevice* s_GraphicsDevice = nullptr;

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
#if CUDA_PLATFORM
        , m_instance(instance)
        , m_isCudaSupport(false)
#endif
    {
        if (profiler)
            m_maker = profiler->CreateMarker(
                "VulkanGraphicsDevice.CopyImage", kUnityProfilerCategoryOther, kUnityProfilerMarkerFlagDefault, 0);
    }

    bool VulkanGraphicsDevice::InitV()
    {
#if CUDA_PLATFORM
        m_isCudaSupport = InitCudaContext();
#endif
        s_GraphicsDevice = this;
        return true;
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

    void VulkanGraphicsDevice::ShutdownV()
    {
#if CUDA_PLATFORM
        m_cudaContext.Shutdown();
#endif
        s_GraphicsDevice = nullptr;
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

    // Returns null if failed
    ITexture2D* VulkanGraphicsDevice::CreateDefaultTextureV(
        const uint32_t w, const uint32_t h, UnityRenderingExtTextureFormat textureFormat)
    {
        std::unique_ptr<VulkanTexture2D> vulkanTexture = std::make_unique<VulkanTexture2D>(w, h);
        if (!vulkanTexture->Init(m_physicalDevice, m_device))
        {
            RTC_LOG(LS_ERROR) << "VulkanTexture2D::Init failed.";
            return nullptr;
        }
        return vulkanTexture.release();
    }

    ITexture2D*
    VulkanGraphicsDevice::CreateCPUReadTextureV(uint32_t w, uint32_t h, UnityRenderingExtTextureFormat textureFormat)
    {
        std::unique_ptr<VulkanTexture2D> vulkanTexture = std::make_unique<VulkanTexture2D>(w, h);
        if (!vulkanTexture->InitCpuRead(m_physicalDevice, m_device))
        {
            RTC_LOG(LS_ERROR) << "VulkanTexture2D::InitCpuRead failed.";
            return nullptr;
        }
        return vulkanTexture.release();
    }

    //---------------------------------------------------------------------------------------------------------------------
    bool VulkanGraphicsDevice::CopyResourceV(ITexture2D* dest, ITexture2D* src)
    {
        VulkanTexture2D* destTexture = reinterpret_cast<VulkanTexture2D*>(dest);
        VulkanTexture2D* srcTexture = reinterpret_cast<VulkanTexture2D*>(src);
        if (destTexture == srcTexture)
            return false;
        if (!destTexture || !srcTexture)
            return false;

        UnityVulkanRecordingState recordingState;
        if (!m_unityVulkan->CommandRecordingState(&recordingState, kUnityVulkanGraphicsQueueAccess_DontCare))
            return false;
        VkCommandBuffer commandBuffer = recordingState.commandBuffer;

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
        destTexture->currentFrameNumber = recordingState.currentFrameNumber;
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

        UnityVulkanRecordingState recordingState;
        if (!m_unityVulkan->CommandRecordingState(&recordingState, kUnityVulkanGraphicsQueueAccess_DontCare))
            return false;

        VkCommandBuffer commandBuffer = recordingState.commandBuffer;

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
                recordingState.commandBuffer,
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
        destTexture->currentFrameNumber = recordingState.currentFrameNumber;
        return true;
    }

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

        VulkanTexture2D* vulkanTexture = static_cast<VulkanTexture2D*>(texture);
        void* exportHandle = VulkanUtility::GetExportHandle(m_device, vulkanTexture->GetTextureImageMemory());

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
        VkResult result = vkQueueWaitIdle(m_graphicsQueue);
        if (result != VK_SUCCESS)
        {
            RTC_LOG(LS_INFO) << "vkQueueWaitIdle failed. result:" << result;
            return false;
        }
        return true;
    }

    static bool IsFinished(UnityGraphicsVulkan* unityVulkan, const VulkanTexture2D* vulkanTexture)
    {
        UnityVulkanRecordingState state;
        if (!unityVulkan->CommandRecordingState(&state, kUnityVulkanGraphicsQueueAccess_DontCare))
        {
            return false;
        }
        return vulkanTexture->currentFrameNumber <= state.safeFrameNumber;
    }

    bool VulkanGraphicsDevice::WaitSync(const ITexture2D* texture, uint64_t nsTimeout)
    {
        const int msecs = 3;
        const auto t0 = std::chrono::high_resolution_clock::now();
        const VulkanTexture2D* vulkanTexture = static_cast<const VulkanTexture2D*>(texture);

        uint64_t total = 0;

        while (nsTimeout > total)
        {
            if (IsFinished(m_unityVulkan, vulkanTexture))
                return true;

            SleepMs(msecs);
            total = std::chrono::duration_cast<std::chrono::nanoseconds>(
                        std::chrono::high_resolution_clock::now().time_since_epoch() - t0.time_since_epoch())
                        .count();
            continue;
        }
        RTC_LOG(LS_INFO) << "VulkanGraphicsDevice::WaitSync failed.";
        return false;
    }

    bool VulkanGraphicsDevice::ResetSync(const ITexture2D* texture) { return true; }

    // void VulkanGraphicsDevice::AccessQueueCallback(int eventID, void* data)
    //{
    //     VulkanTexture2D* texture = reinterpret_cast<VulkanTexture2D*>(data);

    //    VkResult qResult =
    //        QueueSubmit(s_GraphicsDevice->m_graphicsQueue, texture->GetCommandBuffer(), texture->GetFence());
    //    if (qResult != VK_SUCCESS)
    //    {
    //        RTC_LOG(LS_ERROR) << "vkQueueSubmit failed. result:" << qResult;
    //    }
    //}

} // end namespace webrtc
} // end namespace unity
