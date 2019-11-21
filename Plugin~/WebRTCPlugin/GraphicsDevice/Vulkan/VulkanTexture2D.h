#pragma once

#include "GraphicsDevice/ITexture2D.h"
#include "d3d11.h"

#include "WebRTCMacros.h"
#include "Cuda/CudaImage.h"

namespace WebRTC {

class VulkanTexture2D : public ITexture2D {
public:

    VulkanTexture2D(const uint32_t w, const uint32_t h);
    virtual ~VulkanTexture2D();

    bool Init(const VkPhysicalDevice physicalDevice, const VkDevice device); 
    void Shutdown(); 

    inline virtual void* GetNativeTexturePtrV() override;
    inline virtual const void* GetNativeTexturePtrV() const override;
    inline virtual void* GetEncodeTexturePtrV() override;
    inline virtual const void* GetEncodeTexturePtrV() const override;

    inline VkImage      GetImage() const;
    inline VkImageView  GetImageView() const;
    inline VkDeviceMemory GetTextureImageMemory() const;
    inline VkDeviceSize GetTextureImageMemorySize() const;

private:
    VkImage             m_textureImage;
    VkDeviceMemory      m_textureImageMemory;
    VkImageView         m_textureImageView;
    VkDeviceSize        m_textureImageMemorySize;
    VkDevice            m_device;

    CudaImage           m_cudaImage;

    const VkAllocationCallbacks* m_allocator = nullptr;

};

//---------------------------------------------------------------------------------------------------------------------

void* VulkanTexture2D::GetNativeTexturePtrV() { return  m_textureImage; }
const void* VulkanTexture2D::GetNativeTexturePtrV() const { return m_textureImage; };
void* VulkanTexture2D::GetEncodeTexturePtrV() { return m_cudaImage.GetArray(); }
const void* VulkanTexture2D::GetEncodeTexturePtrV() const { return m_cudaImage.GetArray(); }

VkImage         VulkanTexture2D::GetImage() const               { return m_textureImage; }
VkImageView     VulkanTexture2D::GetImageView() const           { return m_textureImageView; }
VkDeviceMemory  VulkanTexture2D::GetTextureImageMemory() const  { return m_textureImageMemory; }
VkDeviceSize    VulkanTexture2D::GetTextureImageMemorySize() const { return m_textureImageMemorySize; }

} //end namespace


