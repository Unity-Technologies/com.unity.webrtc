#include "pch.h"
#include "VulkanGraphicsDevice.h"
#include "VulkanTexture2D.h"

#include "vulkan/vulkan.h"

namespace WebRTC {

VulkanGraphicsDevice::VulkanGraphicsDevice( const VkInstance instance, const VkPhysicalDevice physicalDevice,
    const VkDevice device, const VkQueue graphicsQueue)
    : m_instance (instance)
    , m_physicalDevice(physicalDevice)
    , m_device(device)
    , m_graphicsQueue(graphicsQueue)
{
}

//---------------------------------------------------------------------------------------------------------------------
bool VulkanGraphicsDevice::InitV() {
    if (CUDA_SUCCESS!=m_cudaContext.Init(m_instance, m_physicalDevice))
        return false;

    return true;
}

//---------------------------------------------------------------------------------------------------------------------

void VulkanGraphicsDevice::ShutdownV() {
    m_cudaContext.Shutdown();
}

//---------------------------------------------------------------------------------------------------------------------

//Returns null if failed
ITexture2D* VulkanGraphicsDevice::CreateDefaultTextureV(const uint32_t w, const uint32_t h) {

    VulkanTexture2D* vulkanTexture = new VulkanTexture2D(w, h);
    if (!vulkanTexture->Init(m_physicalDevice, m_device)) {
        vulkanTexture->Shutdown();
        delete (vulkanTexture);
        return nullptr;
    }
    return vulkanTexture;
}

//---------------------------------------------------------------------------------------------------------------------
ITexture2D* VulkanGraphicsDevice::CreateDefaultTextureFromNativeV(uint32_t w, uint32_t h, void* nativeTexturePtr) {
    //[TODO-sin: 2019-11-20] Fix this
    return CreateDefaultTextureV(w,h);
}


//---------------------------------------------------------------------------------------------------------------------
bool VulkanGraphicsDevice::CopyResourceV(ITexture2D* dest, ITexture2D* src) {
    //[TODO-sin: 2019-11-20] Fix this
    return false;
}

//---------------------------------------------------------------------------------------------------------------------
bool VulkanGraphicsDevice::CopyResourceFromNativeV(ITexture2D* dest, void* nativeTexturePtr) {
    //[TODO-sin: 2019-11-20] Fix this
    return false;
}

} //end namespace
