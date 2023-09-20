#include "pch.h"

#include "VulkanTexture2D.h"

namespace unity
{
namespace webrtc
{
    VulkanTexture2D::VulkanTexture2D(const uint32_t w, const uint32_t h)
        : ITexture2D(w, h)
#if !VULKAN_USE_CRS
        , m_commandPool(VK_NULL_HANDLE)
        , m_commandBuffer(VK_NULL_HANDLE)
        , m_fence(VK_NULL_HANDLE)
#endif
        , m_textureFormat(VK_FORMAT_B8G8R8A8_UNORM)
        , m_rowPitch(w * 4)
    {
    }

    VulkanTexture2D::~VulkanTexture2D() { Shutdown(); }

    void VulkanTexture2D::Shutdown()
    {
        RemoveTextureBinding(GetImage());

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
#if !VULKAN_USE_CRS
        if (m_commandBuffer != VK_NULL_HANDLE)
        {
            vkFreeCommandBuffers(m_Instance.device, m_commandPool, 1, &m_commandBuffer);
            m_commandBuffer = VK_NULL_HANDLE;
        }
        if (m_fence != VK_NULL_HANDLE)
        {
            vkDestroyFence(m_Instance.device, m_fence, nullptr);
            m_fence = VK_NULL_HANDLE;
        }
        m_commandPool = VK_NULL_HANDLE;
#endif
        m_unityVulkanImage.memory.size = 0;
        m_Instance.device = nullptr;
    }

#if !VULKAN_USE_CRS
    bool VulkanTexture2D::CreateFence()
    {
        // Create a command buffer to copy
        VkCommandBufferAllocateInfo allocInfo = {};
        allocInfo.sType = VK_STRUCTURE_TYPE_COMMAND_BUFFER_ALLOCATE_INFO;
        allocInfo.level = VK_COMMAND_BUFFER_LEVEL_PRIMARY;
        allocInfo.commandPool = m_commandPool;
        allocInfo.commandBufferCount = 1;

        VkResult result = vkAllocateCommandBuffers(m_Instance.device, &allocInfo, &m_commandBuffer);
        if (result != VK_SUCCESS)
        {
            RTC_LOG(LS_INFO) << "vkAllocateCommandBuffers failed. result:" << result;
            return false;
        }

        VkFenceCreateInfo createInfo = {};
        createInfo.sType = VK_STRUCTURE_TYPE_FENCE_CREATE_INFO;
        createInfo.pNext = nullptr;
        createInfo.flags = 0;
        result = vkCreateFence(m_Instance.device, &createInfo, nullptr, &m_fence);
        if (result != VK_SUCCESS)
        {
            RTC_LOG(LS_INFO) << "vkCreateFence failed. result:" << result;
            return false;
        }
        return true;
    }
#endif

    bool VulkanTexture2D::Init(const UnityVulkanInstance* instance, const VkCommandPool commandPool)
    {
        m_Instance = *instance;
#if !VULKAN_USE_CRS
        m_commandPool = commandPool;
        if (!CreateFence())
            return false;
#endif

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

        BindTexture(GetImage(), this);
        return true;
    }

    bool VulkanTexture2D::InitStaging(
        const UnityVulkanInstance* instance, const VkCommandPool commandPool, bool writable, bool hasHostCachedMemory)
    {
        m_Instance = *instance;
#if !VULKAN_USE_CRS
        m_commandPool = commandPool;
        if (!CreateFence())
            return false;
#endif

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

        BindTexture(GetImage(), this);
        return true;
    }

} // end namespace webrtc
} // end namespace unity
