#pragma once

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

        bool Init(const UnityVulkanInstance* instance);
        bool InitStaging(const UnityVulkanInstance* instance, bool writable, bool hasHostCachedMemory);
        void Shutdown();

        inline virtual void* GetNativeTexturePtrV() override;
        inline virtual const void* GetNativeTexturePtrV() const override;
        inline virtual void* GetEncodeTexturePtrV() override;
        inline virtual const void* GetEncodeTexturePtrV() const override;

        inline VkImage GetImage() const;
        inline VkDeviceMemory GetTextureImageMemory() const;
        inline VkDeviceSize GetTextureImageMemorySize() const;
        inline VkFormat GetTextureFormat() const;

        size_t GetPitch() const { return m_rowPitch; }

        void ResetFrameNumber() const { currentFrameNumber = 0; }
        mutable unsigned long long currentFrameNumber = 0;

    private:
        UnityVulkanInstance m_Instance = {};
        VkFormat m_textureFormat;
        size_t m_rowPitch;
        UnityVulkanImage m_unityVulkanImage = {};
        const VkAllocationCallbacks* m_allocator = nullptr;
    };

    void* VulkanTexture2D::GetNativeTexturePtrV() { return &m_unityVulkanImage; }
    const void* VulkanTexture2D::GetNativeTexturePtrV() const { return &m_unityVulkanImage; };
    void* VulkanTexture2D::GetEncodeTexturePtrV() { return nullptr; }
    const void* VulkanTexture2D::GetEncodeTexturePtrV() const { return nullptr; }

    VkImage VulkanTexture2D::GetImage() const { return m_unityVulkanImage.image; }
    VkDeviceMemory VulkanTexture2D::GetTextureImageMemory() const { return m_unityVulkanImage.memory.memory; }
    VkDeviceSize VulkanTexture2D::GetTextureImageMemorySize() const { return m_unityVulkanImage.memory.size; }
    VkFormat VulkanTexture2D::GetTextureFormat() const { return m_textureFormat; }

} // end namespace unity
} // end namespace webrtc
