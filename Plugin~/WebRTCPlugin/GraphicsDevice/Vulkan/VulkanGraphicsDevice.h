#pragma once

#include <IUnityGraphicsVulkan.h>
#include <api/video/i420_buffer.h>
#include <memory>
#include <vulkan/vulkan.h>

#include "PlatformBase.h"

#if CUDA_PLATFORM
#include "GraphicsDevice/Cuda/CudaContext.h"
#endif
#include "GraphicsDevice/IGraphicsDevice.h"

namespace unity
{
namespace webrtc
{

    using namespace ::webrtc;

    class UnityGraphicsVulkan;
    class VulkanGraphicsDevice : public IGraphicsDevice
    {
    public:
        VulkanGraphicsDevice(
            UnityGraphicsVulkan* unityVulkan,
            const VkInstance instance,
            const VkPhysicalDevice physicalDevice,
            const VkDevice device,
            const VkQueue graphicsQueue,
            const uint32_t queueFamilyIndex,
            UnityGfxRenderer renderer,
            ProfilerMarkerFactory* profiler);

        ~VulkanGraphicsDevice() override = default;
        bool InitV() override;
        void ShutdownV() override;
        inline void* GetEncodeDevicePtrV() override;
        ITexture2D* CreateDefaultTextureV(
            const uint32_t w, const uint32_t h, UnityRenderingExtTextureFormat textureFormat) override;
        ITexture2D*
        CreateCPUReadTextureV(uint32_t width, uint32_t height, UnityRenderingExtTextureFormat textureFormat) override;

        std::unique_ptr<UnityVulkanImage> AccessTexture(void* ptr) const;

        bool CopyResourceV(ITexture2D* dest, ITexture2D* src) override;

        /// <summary>
        ///
        /// </summary>
        /// <param name="dest"></param>
        /// <param name="nativeTexturePtr"> a pointer of UnityVulkanImage </param>
        /// <returns></returns>
        bool CopyResourceFromNativeV(ITexture2D* dest, void* nativeTexturePtr) override;
        std::unique_ptr<GpuMemoryBufferHandle> Map(ITexture2D* texture) override;
        bool WaitSync(const ITexture2D* texture, uint64_t nsTimeout = 0) override;
        bool ResetSync(const ITexture2D* texture) override;
        bool WaitIdleForTest() override;
        rtc::scoped_refptr<I420Buffer> ConvertRGBToI420(ITexture2D* tex) override;

#if CUDA_PLATFORM
        bool IsCudaSupport() override { return m_isCudaSupport; }
        CUcontext GetCUcontext() override { return m_cudaContext.GetContext(); }
        NV_ENC_BUFFER_FORMAT GetEncodeBufferFormat() override { return NV_ENC_BUFFER_FORMAT_ARGB; }
#endif
    private:
        VkResult CreateCommandPool();

        UnityGraphicsVulkan* m_unityVulkan;
        VkPhysicalDevice m_physicalDevice;
        VkDevice m_device;
        VkQueue m_graphicsQueue;
        VkCommandPool m_commandPool;
        uint32_t m_queueFamilyIndex;
        VkAllocationCallbacks* m_allocator;
        const UnityProfilerMarkerDesc* m_maker;

#if CUDA_PLATFORM
        bool InitCudaContext();
        VkInstance m_instance;
        CudaContext m_cudaContext;
        bool m_isCudaSupport;
#endif
    };

    void* VulkanGraphicsDevice::GetEncodeDevicePtrV()
    {
#if CUDA_PLATFORM
        return reinterpret_cast<void*>(m_cudaContext.GetContext());
#else
        return nullptr;
#endif
    }
} // end namespace webrtc
} // end namespace unity
