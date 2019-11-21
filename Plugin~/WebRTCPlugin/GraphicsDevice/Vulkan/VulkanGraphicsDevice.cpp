#include "pch.h"
#include "VulkanGraphicsDevice.h"
#include "VulkanTexture2D.h"

#include "vulkan/vulkan.h"

namespace WebRTC {

VulkanGraphicsDevice::VulkanGraphicsDevice( const VkInstance instance, const VkPhysicalDevice physicalDevice,
    const VkDevice device, const VkQueue graphicsQueue, const uint32_t queueFamilyIndex)
    : m_instance (instance)
    , m_physicalDevice(physicalDevice)
    , m_device(device)
    , m_graphicsQueue(graphicsQueue)
    , m_commandPool(VK_NULL_HANDLE)
    , m_queueFamilyIndex(queueFamilyIndex)
{
}

//---------------------------------------------------------------------------------------------------------------------
bool VulkanGraphicsDevice::InitV() {
    if (CUDA_SUCCESS!=m_cudaContext.Init(m_instance, m_physicalDevice))
        return false;

    return (VK_SUCCESS == CreateCommandPool());

}

//---------------------------------------------------------------------------------------------------------------------

void VulkanGraphicsDevice::ShutdownV() {
    VULKAN_SAFE_DESTROY_COMMAND_POOL(m_device, m_commandPool, m_allocator);

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
bool VulkanGraphicsDevice::CopyResourceV(ITexture2D* dest, ITexture2D* src) {
    //[TODO-sin: 2019-11-20] Fix this
    return false;
}

//---------------------------------------------------------------------------------------------------------------------
bool VulkanGraphicsDevice::CopyResourceFromNativeV(ITexture2D* dest, void* nativeTexturePtr) {
    //[TODO-sin: 2019-11-20] Fix this
    return false;
}

//---------------------------------------------------------------------------------------------------------------------
VkResult VulkanGraphicsDevice::CreateCommandPool() {
    VkCommandPoolCreateInfo poolInfo = {};
    poolInfo.sType = VK_STRUCTURE_TYPE_COMMAND_POOL_CREATE_INFO;
    poolInfo.queueFamilyIndex = m_queueFamilyIndex;
    poolInfo.flags = 0; 

    return vkCreateCommandPool(m_device, &poolInfo, m_allocator, &m_commandPool);
}

} //end namespace
