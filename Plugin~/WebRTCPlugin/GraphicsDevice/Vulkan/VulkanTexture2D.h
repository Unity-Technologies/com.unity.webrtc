#pragma once

#include <vulkan/vulkan.h>

#include "GraphicsDevice/ITexture2D.h"
#include "IUnityGraphicsVulkan.h"

namespace unity
{
namespace webrtc
{

    class VulkanTexture2D : public ITexture2D
    {
    public:
        VulkanTexture2D(const uint32_t w, const uint32_t h);
        virtual ~VulkanTexture2D() override;

        bool Init(const VkPhysicalDevice physicalDevice, const VkDevice device, const VkCommandPool commandPool);
        bool InitCpuRead(const VkPhysicalDevice physicalDevice, const VkDevice device, const VkCommandPool commandPool);
        void Shutdown();

        inline virtual void* GetNativeTexturePtrV() override;
        inline virtual const void* GetNativeTexturePtrV() const override;
        inline virtual void* GetEncodeTexturePtrV() override;
        inline virtual const void* GetEncodeTexturePtrV() const override;

        inline VkImage GetImage() const;
        inline VkDeviceMemory GetTextureImageMemory() const;
        inline VkDeviceSize GetTextureImageMemorySize() const;
        inline VkFormat GetTextureFormat() const;

        VkFence GetFence() const { return m_fence; }
        VkCommandBuffer GetCommandBuffer() const { return m_commandBuffer; }

    private:
        VkImage m_textureImage;
        VkDeviceMemory m_textureImageMemory;
        VkDeviceSize m_textureImageMemorySize;
        VkPhysicalDevice m_physicalDevice;
        VkDevice m_device;
        VkCommandPool m_commandPool;
        VkFence m_fence;
        VkCommandBuffer m_commandBuffer;
        VkFormat m_textureFormat;
        UnityVulkanImage m_unityVulkanImage;
        const VkAllocationCallbacks* m_allocator = nullptr;
    };

    //---------------------------------------------------------------------------------------------------------------------

    void* VulkanTexture2D::GetNativeTexturePtrV() { return &m_unityVulkanImage; }
    const void* VulkanTexture2D::GetNativeTexturePtrV() const { return &m_unityVulkanImage; };
    void* VulkanTexture2D::GetEncodeTexturePtrV() { return nullptr; }
    const void* VulkanTexture2D::GetEncodeTexturePtrV() const { return nullptr; }

    VkImage VulkanTexture2D::GetImage() const { return m_textureImage; }
    VkDeviceMemory VulkanTexture2D::GetTextureImageMemory() const { return m_textureImageMemory; }
    VkDeviceSize VulkanTexture2D::GetTextureImageMemorySize() const { return m_textureImageMemorySize; }
    VkFormat VulkanTexture2D::GetTextureFormat() const { return m_textureFormat; }

} // end namespace unity
} // end namespace webrtc
