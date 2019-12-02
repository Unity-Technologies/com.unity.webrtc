#include "pch.h"
#include "VulkanGraphicsDevice.h"
#include "VulkanTexture2D.h"

#include "IUnityGraphicsVulkan.h"
#include "vulkan/vulkan.h"
#include "VulkanUtility.h"

namespace WebRTC {

VulkanGraphicsDevice::VulkanGraphicsDevice( IUnityGraphicsVulkan* unityVulkan, const VkInstance instance,
    const VkPhysicalDevice physicalDevice,
    const VkDevice device, const VkQueue graphicsQueue, const uint32_t queueFamilyIndex)
    : m_unityVulkan(unityVulkan)
    , m_instance (instance)
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

    //Transition to dest
    if (VK_SUCCESS!= VulkanUtility::DoImageLayoutTransition(m_device, m_commandPool, m_graphicsQueue, 
            vulkanTexture->GetImage(), vulkanTexture->GetTextureFormat(), 
            VK_IMAGE_LAYOUT_UNDEFINED, VK_PIPELINE_STAGE_TOP_OF_PIPE_BIT,
            VK_IMAGE_LAYOUT_TRANSFER_DST_OPTIMAL, VK_PIPELINE_STAGE_TRANSFER_BIT))
    {
        vulkanTexture->Shutdown();
        delete (vulkanTexture);
        return nullptr;       
    }

    return vulkanTexture;
}


//---------------------------------------------------------------------------------------------------------------------
bool VulkanGraphicsDevice::CopyResourceV(ITexture2D* dest, ITexture2D* src) {

    VulkanTexture2D* destTexture = reinterpret_cast<VulkanTexture2D*>(dest);
    VulkanTexture2D* srcTexture = reinterpret_cast<VulkanTexture2D*>(src);
    if (destTexture == srcTexture)
        return false;
    if (destTexture == nullptr || srcTexture == nullptr)
        return false;

    //Transition the src texture layout. 
    VULKAN_CHECK_FAILVALUE(
        VulkanUtility::DoImageLayoutTransition(m_device, m_commandPool, m_graphicsQueue, 
            srcTexture->GetImage(), srcTexture->GetTextureFormat(), 
            VK_IMAGE_LAYOUT_TRANSFER_DST_OPTIMAL, VK_PIPELINE_STAGE_TRANSFER_BIT,
            VK_IMAGE_LAYOUT_TRANSFER_SRC_OPTIMAL, VK_PIPELINE_STAGE_TRANSFER_BIT
        ),
        false
    );

    //[TODO-sin: 2019-11-21] Optimize so that we don't do vkQueueWaitIdle multiple times here
    //The layouts of All VulkanTexture2D should be VK_IMAGE_LAYOUT_TRANSFER_DST_OPTIMAL, so no transition for destTex
    VULKAN_CHECK_FAILVALUE(
        VulkanUtility::CopyImage(m_device, m_commandPool, m_graphicsQueue, srcTexture->GetImage(),
            destTexture->GetImage(), destTexture->GetWidth(), destTexture->GetHeight()
        ),
        false
    );

    //transition the src texture layout back to VK_IMAGE_LAYOUT_TRANSFER_DST_OPTIMAL
    VULKAN_CHECK_FAILVALUE(
        VulkanUtility::DoImageLayoutTransition(m_device, m_commandPool, m_graphicsQueue, 
            srcTexture->GetImage(), srcTexture->GetTextureFormat(), 
            VK_IMAGE_LAYOUT_TRANSFER_SRC_OPTIMAL, VK_PIPELINE_STAGE_TRANSFER_BIT,
            VK_IMAGE_LAYOUT_TRANSFER_DST_OPTIMAL, VK_PIPELINE_STAGE_TRANSFER_BIT
        ),
        false
    );

    return true;
}

//---------------------------------------------------------------------------------------------------------------------
bool VulkanGraphicsDevice::CopyResourceFromNativeV(ITexture2D* dest, void* nativeTexturePtr) {
    if (nullptr == dest || nullptr == nativeTexturePtr)
        return false;

    VulkanTexture2D* destTexture = reinterpret_cast<VulkanTexture2D*>(dest);
    UnityVulkanImage unityVulkanImage;
    VkImageSubresource subResource { VK_IMAGE_ASPECT_COLOR_BIT, 0, 0 };

    if (!m_unityVulkan->AccessTexture(nativeTexturePtr, &subResource, VK_IMAGE_LAYOUT_TRANSFER_SRC_OPTIMAL,
        VK_PIPELINE_STAGE_TRANSFER_BIT, VK_ACCESS_TRANSFER_READ_BIT, kUnityVulkanResourceAccess_PipelineBarrier,
        &unityVulkanImage))
    {
        return false;
    }

    if (destTexture->GetImage() == unityVulkanImage.image)
        return false;

    //The layouts of All VulkanTexture2D should be VK_IMAGE_LAYOUT_TRANSFER_DST_OPTIMAL, so no transition for destTex
    VULKAN_CHECK_FAILVALUE(
        VulkanUtility::CopyImage(m_device, m_commandPool, m_graphicsQueue,
            unityVulkanImage.image, destTexture->GetImage(), destTexture->GetWidth(), destTexture->GetHeight()),
        false
    );

    return true;
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
