#pragma once

#include "GraphicsDevice/IGraphicsDevice.h"
#include "WebRTCConstants.h"
#include "Cuda/CudaContext.h"

namespace WebRTC {


class VulkanGraphicsDevice : public IGraphicsDevice{
public:
    VulkanGraphicsDevice( const VkInstance instance, const VkPhysicalDevice physicalDevice, const VkDevice device,
        const VkQueue graphicsQueue);

    virtual ~VulkanGraphicsDevice() = default;
    virtual bool InitV() override;
    virtual void ShutdownV() override;
    inline virtual void* GetEncodeDevicePtrV() override;
    virtual ITexture2D* CreateDefaultTextureV(const uint32_t w, const uint32_t h) override;
    virtual ITexture2D* CreateDefaultTextureFromNativeV(uint32_t w, uint32_t h, void* nativeTexturePtr) override;
    virtual bool CopyResourceV(ITexture2D* dest, ITexture2D* src) override;
    virtual bool CopyResourceFromNativeV(ITexture2D* dest, void* nativeTexturePtr) override;
private:
    const UnityVulkanInstance* m_unityVulkan;

    VkInstance  m_instance;
    VkPhysicalDevice m_physicalDevice;
    VkDevice    m_device;
    VkQueue     m_graphicsQueue;

    CudaContext m_cudaContext;

};

//---------------------------------------------------------------------------------------------------------------------

void* VulkanGraphicsDevice::GetEncodeDevicePtrV() { return reinterpret_cast<void*>(m_cudaContext.GetContext()); }



}
