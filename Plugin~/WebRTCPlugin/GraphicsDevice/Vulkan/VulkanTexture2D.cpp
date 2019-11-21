﻿#include "pch.h"
#include "VulkanTexture2D.h"

#include "GraphicsDevice/Vulkan/VulkanUtility.h"

namespace WebRTC {

//---------------------------------------------------------------------------------------------------------------------

VulkanTexture2D::VulkanTexture2D(const uint32_t w, const uint32_t h) : ITexture2D(w,h),
    m_textureImage(VK_NULL_HANDLE), m_textureImageMemory(VK_NULL_HANDLE), m_textureImageView(VK_NULL_HANDLE),
    m_textureImageMemorySize(0), m_device(VK_NULL_HANDLE)
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
    VULKAN_SAFE_DESTROY_IMAGE_VIEW(m_device, m_textureImageView, m_allocator);
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
    m_textureImageMemorySize = VulkanUtility::CreateImage(physicalDevice,device,m_allocator, m_width, m_height,
        VK_IMAGE_TILING_OPTIMAL,
        VK_IMAGE_USAGE_COLOR_ATTACHMENT_BIT | VK_IMAGE_USAGE_SAMPLED_BIT,
        VK_MEMORY_PROPERTY_DEVICE_LOCAL_BIT,VK_FORMAT_R8G8B8A8_UNORM, &m_textureImage,&m_textureImageMemory,
        EXPORT_HANDLE
    );

    if (m_textureImageMemorySize <= 0) {
        return false;
    }

    m_textureImageView = VulkanUtility::CreateImageView(device, 
        m_allocator, m_textureImage, VK_FORMAT_R8G8B8A8_UNORM);

    if (VK_NULL_HANDLE == m_textureImageView)
        return false;

    return (CUDA_SUCCESS == m_cudaImage.Init(m_device, this));

}


} //end namespace
