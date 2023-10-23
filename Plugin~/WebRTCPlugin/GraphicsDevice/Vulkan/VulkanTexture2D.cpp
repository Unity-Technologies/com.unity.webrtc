#include "pch.h"

#include "GraphicsDevice/Vulkan/VulkanUtility.h"
#include "VulkanTexture2D.h"

namespace unity
{
namespace webrtc
{
    VulkanTexture2D::VulkanTexture2D(const uint32_t w, const uint32_t h)
        : ITexture2D(w, h)
        , m_textureFormat(VK_FORMAT_B8G8R8A8_UNORM)
    {
    }

    VulkanTexture2D::~VulkanTexture2D() { Shutdown(); }

    void VulkanTexture2D::Shutdown()
    {
        if (m_unityVulkanImage.image != VK_NULL_HANDLE)
        {
            vkDestroyImage(m_Instance.device, m_unityVulkanImage.image, m_allocator);
            m_unityVulkanImage.image = VK_NULL_HANDLE;
        }
        if (m_unityVulkanImage.memory.memory != VK_NULL_HANDLE)
        {
            vkFreeMemory(m_Instance.device, m_unityVulkanImage.memory.memory, m_allocator);
            m_unityVulkanImage.memory.memory = VK_NULL_HANDLE;
        }
        m_unityVulkanImage.memory.size = 0;
        m_Instance.device = nullptr;
    }

    bool VulkanTexture2D::Init(const UnityVulkanInstance* instance)
    {
        m_Instance = *instance;

        const bool EXPORT_HANDLE = true;
        VkResult result = VulkanUtility::CreateImage(
            m_Instance,
            m_allocator,
            m_width,
            m_height,
            VK_IMAGE_TILING_OPTIMAL,
            VK_IMAGE_USAGE_TRANSFER_SRC_BIT | VK_IMAGE_USAGE_TRANSFER_DST_BIT,
            VK_MEMORY_PROPERTY_DEVICE_LOCAL_BIT,
            m_textureFormat,
            &m_unityVulkanImage,
            EXPORT_HANDLE);

        if (result != VK_SUCCESS)
        {
            return false;
        }

        return true;
    }

    bool VulkanTexture2D::InitStaging(const UnityVulkanInstance* instance, bool writable, bool hasHostCachedMemory)
    {
        m_Instance = *instance;

        VkMemoryPropertyFlags properties = VK_MEMORY_PROPERTY_HOST_VISIBLE_BIT;

        if (writable)
            properties |= VK_MEMORY_PROPERTY_HOST_COHERENT_BIT;
        else
            properties |=
                (hasHostCachedMemory ? VK_MEMORY_PROPERTY_HOST_CACHED_BIT : VK_MEMORY_PROPERTY_HOST_COHERENT_BIT);

        const bool EXPORT_HANDLE = false;
        VkResult result = VulkanUtility::CreateImage(
            m_Instance,
            m_allocator,
            m_width,
            m_height,
            VK_IMAGE_TILING_LINEAR,
            (writable ? VK_IMAGE_USAGE_TRANSFER_SRC_BIT : VK_IMAGE_USAGE_TRANSFER_DST_BIT),
            properties,
            m_textureFormat,
            &m_unityVulkanImage,
            EXPORT_HANDLE);

        if (result != VK_SUCCESS)
        {
            return false;
        }

        VkImageSubresource subresource { VK_IMAGE_ASPECT_COLOR_BIT, 0, 0 };
        VkSubresourceLayout subresourceLayout;
        vkGetImageSubresourceLayout(m_Instance.device, m_unityVulkanImage.image, &subresource, &subresourceLayout);
        m_rowPitch = static_cast<size_t>(subresourceLayout.rowPitch);

        return true;
    }
} // end namespace webrtc
} // end namespace unity
