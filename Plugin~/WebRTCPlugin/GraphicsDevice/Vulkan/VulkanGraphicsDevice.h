#pragma once

#include <memory>
#include <vulkan/vulkan.h>
#include <api/video/i420_buffer.h>

#include "IUnityGraphicsVulkan.h"
#include "IUnityRenderingExtensions.h"
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

    class VulkanGraphicsDevice : public IGraphicsDevice
    {
    public:
        VulkanGraphicsDevice(
            IUnityGraphicsVulkan* unityVulkan,
            const VkInstance instance,
            const VkPhysicalDevice physicalDevice,
            const VkDevice device,
            const VkQueue graphicsQueue,
            const uint32_t queueFamilyIndex,
            UnityGfxRenderer renderer,
            ProfilerMarkerFactory* profiler);

        virtual ~VulkanGraphicsDevice() override = default;
        virtual bool InitV() override;
        virtual void ShutdownV() override;
        inline virtual void* GetEncodeDevicePtrV() override;
        virtual ITexture2D* CreateDefaultTextureV(
            const uint32_t w, const uint32_t h, UnityRenderingExtTextureFormat textureFormat) override;
        virtual ITexture2D*
        CreateCPUReadTextureV(uint32_t width, uint32_t height, UnityRenderingExtTextureFormat textureFormat) override;

        std::unique_ptr<UnityVulkanImage> AccessTexture(void* ptr) const;

        virtual bool CopyResourceV(ITexture2D* dest, ITexture2D* src) override;

        virtual NativeTexPtr ConvertNativeFromUnityPtr(void* tex) override;

        /// <summary>
        ///
        /// </summary>
        /// <param name="dest"></param>
        /// <param name="nativeTexturePtr"> a pointer of UnityVulkanImage </param>
        /// <returns></returns>
        virtual bool CopyResourceFromNativeV(ITexture2D* dest, void* nativeTexturePtr) override;
        std::unique_ptr<GpuMemoryBufferHandle> Map(ITexture2D* texture) override;

        virtual rtc::scoped_refptr<I420Buffer> ConvertRGBToI420(ITexture2D* tex) override;

#if CUDA_PLATFORM
        bool IsCudaSupport() override { return m_isCudaSupport; }
        CUcontext GetCUcontext() override { return m_cudaContext.GetContext(); }
        NV_ENC_BUFFER_FORMAT GetEncodeBufferFormat() override { return NV_ENC_BUFFER_FORMAT_ARGB; }
#endif
    private:
        VkResult CreateCommandPool();

        IUnityGraphicsVulkan* m_unityVulkan;
        VkPhysicalDevice m_physicalDevice;
        VkDevice m_device;
        VkQueue m_graphicsQueue;
        VkCommandPool m_commandPool;
        uint32_t m_queueFamilyIndex;
        VkAllocationCallbacks* m_allocator;
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
