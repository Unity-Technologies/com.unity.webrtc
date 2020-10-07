#include "pch.h"
#include "VulkanTexture2D.h"

#include "GraphicsDevice/Vulkan/VulkanUtility.h"

namespace unity
{
namespace webrtc
{

//---------------------------------------------------------------------------------------------------------------------

VulkanTexture2D::VulkanTexture2D(const uint32_t w, const uint32_t h) : ITexture2D(w,h),
    m_textureImage(VK_NULL_HANDLE), m_textureImageMemory(VK_NULL_HANDLE),
    m_textureImageMemorySize(0), m_device(VK_NULL_HANDLE),
    m_textureFormat(VK_FORMAT_B8G8R8A8_UNORM)
{
}

//---------------------------------------------------------------------------------------------------------------------

VulkanTexture2D::~VulkanTexture2D() {
    Shutdown();

}

//---------------------------------------------------------------------------------------------------------------------

void VulkanTexture2D::Shutdown()
{
    //[TODO-sin: 2019-11-20] Create an explicit Shutdown(device) function
    VULKAN_SAFE_DESTROY_IMAGE(m_device, m_textureImage, m_allocator);
    VULKAN_SAFE_FREE_MEMORY(m_device, m_textureImageMemory, m_allocator);
    m_textureImageMemorySize = 0;
    m_device = VK_NULL_HANDLE;

    m_cudaImage.Shutdown();    
}

//---------------------------------------------------------------------------------------------------------------------

bool VulkanTexture2D::Init(const VkPhysicalDevice physicalDevice, const VkDevice device) {
    m_device = device;

    const bool EXPORT_HANDLE = true;
    VkResult result = VulkanUtility::CreateImage(
        physicalDevice,device,m_allocator, m_width, m_height,
        VK_IMAGE_TILING_OPTIMAL,
        VK_IMAGE_USAGE_TRANSFER_SRC_BIT | VK_IMAGE_USAGE_TRANSFER_DST_BIT,
        VK_MEMORY_PROPERTY_DEVICE_LOCAL_BIT,
        m_textureFormat, &m_unityVulkanImage,
        EXPORT_HANDLE
    );

    if (result != VK_SUCCESS) {
        return false;
    }

    m_textureImage = m_unityVulkanImage.image;
    m_textureImageMemory = m_unityVulkanImage.memory.memory;
    m_textureImageMemorySize = m_unityVulkanImage.memory.size;

    return (CUDA_SUCCESS == m_cudaImage.Init(m_device, this));
}

//---------------------------------------------------------------------------------------------------------------------
bool VulkanTexture2D::InitCpuRead(const VkPhysicalDevice physicalDevice, const VkDevice device) {
    m_device = device;

    const bool EXPORT_HANDLE = false;
    VkResult result = VulkanUtility::CreateImage(
        physicalDevice, device, m_allocator, m_width, m_height,
        VK_IMAGE_TILING_LINEAR,
        VK_IMAGE_USAGE_TRANSFER_DST_BIT,
        VK_MEMORY_PROPERTY_HOST_VISIBLE_BIT | VK_MEMORY_PROPERTY_HOST_COHERENT_BIT,
        m_textureFormat, &m_unityVulkanImage,
        EXPORT_HANDLE
    );

    if (result != VK_SUCCESS) {
        return false;
    }

    m_textureImage = m_unityVulkanImage.image;
    m_textureImageMemory = m_unityVulkanImage.memory.memory;
    m_textureImageMemorySize = m_unityVulkanImage.memory.size;
    return true;
}

} // end namespace webrtc
} // end namespace unity
