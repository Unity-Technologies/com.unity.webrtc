#pragma once

#include "WebRTCConstants.h"
#if CUDA_PLATFORM
#include "GraphicsDevice/Cuda/CudaContext.h"
#endif
#include "GraphicsDevice/IGraphicsDevice.h"

namespace unity
{
namespace webrtc
{

namespace webrtc = ::webrtc;

class VulkanGraphicsDevice : public IGraphicsDevice{
public:
    VulkanGraphicsDevice( IUnityGraphicsVulkan* unityVulkan, const VkInstance instance,
        const VkPhysicalDevice physicalDevice, const VkDevice device,
        const VkQueue graphicsQueue, const uint32_t queueFamilyIndex);

    virtual ~VulkanGraphicsDevice() = default;
    virtual bool InitV() override;
    virtual void ShutdownV() override;
    inline virtual void* GetEncodeDevicePtrV() override;
    virtual ITexture2D* CreateDefaultTextureV(const uint32_t w, const uint32_t h, UnityRenderingExtTextureFormat textureFormat) override;
    virtual ITexture2D* CreateCPUReadTextureV(uint32_t width, uint32_t height, UnityRenderingExtTextureFormat textureFormat) override;

    std::unique_ptr<UnityVulkanImage> AccessTexture(void* ptr) const;

    virtual bool CopyResourceV(ITexture2D* dest, ITexture2D* src) override;
    virtual bool CopyResourceFromNativeV(ITexture2D* dest, void* nativeTexturePtr) override;
    inline virtual GraphicsDeviceType GetDeviceType() const override;
    virtual rtc::scoped_refptr<webrtc::I420Buffer> ConvertRGBToI420(ITexture2D* tex) override;

#if CUDA_PLATFORM
    virtual bool IsCudaSupport() override { return m_isCudaSupport; }
    virtual CUcontext GetCuContext() override { return m_cudaContext.GetContext(); }
#endif
private:

    VkResult CreateCommandPool();

    IUnityGraphicsVulkan*   m_unityVulkan;
    VkInstance              m_instance;
    VkPhysicalDevice        m_physicalDevice;
    VkDevice                m_device;
    VkQueue                 m_graphicsQueue;
    VkCommandPool           m_commandPool;
    uint32_t m_queueFamilyIndex;

#if CUDA_PLATFORM
    CudaContext m_cudaContext;
    bool m_isCudaSupport;
#endif
    const VkAllocationCallbacks* m_allocator = nullptr;
};

//---------------------------------------------------------------------------------------------------------------------

void* VulkanGraphicsDevice::GetEncodeDevicePtrV()
{
#if CUDA_PLATFORM
    return reinterpret_cast<void*>(m_cudaContext.GetContext());
#else
    return nullptr;
#endif
}
GraphicsDeviceType VulkanGraphicsDevice::GetDeviceType() const { return GRAPHICS_DEVICE_VULKAN; }

} // end namespace webrtc
} // end namespace unity
