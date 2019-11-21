﻿#pragma once

#include "GraphicsDevice/IGraphicsDevice.h"
#include "WebRTCConstants.h"
#include "Cuda/CudaContext.h"

namespace WebRTC {


class VulkanGraphicsDevice : public IGraphicsDevice{
public:
    VulkanGraphicsDevice( const VkInstance instance, const VkPhysicalDevice physicalDevice, const VkDevice device,
        const VkQueue graphicsQueue, const uint32_t queueFamilyIndex);

    virtual ~VulkanGraphicsDevice() = default;
    virtual bool InitV() override;
    virtual void ShutdownV() override;
    inline virtual void* GetEncodeDevicePtrV() override;
    virtual ITexture2D* CreateDefaultTextureV(const uint32_t w, const uint32_t h) override;
    virtual bool CopyResourceV(ITexture2D* dest, ITexture2D* src) override;
    virtual bool CopyResourceFromNativeV(ITexture2D* dest, void* nativeTexturePtr) override;
    inline virtual GraphicsDeviceType GetDeviceType() override;
private:

    VkResult CreateCommandPool();

    VkInstance      m_instance;
    VkPhysicalDevice m_physicalDevice;
    VkDevice        m_device;
    VkQueue         m_graphicsQueue;
    VkCommandPool   m_commandPool;

    CudaContext m_cudaContext;
    uint32_t m_queueFamilyIndex;

    const VkAllocationCallbacks* m_allocator = nullptr;

};

//---------------------------------------------------------------------------------------------------------------------

void* VulkanGraphicsDevice::GetEncodeDevicePtrV() { return reinterpret_cast<void*>(m_cudaContext.GetContext()); }
GraphicsDeviceType VulkanGraphicsDevice::GetDeviceType() { return GRAPHICS_DEVICE_VULKAN; }



}
