#pragma once

#include "cuda.h"

namespace unity
{
namespace webrtc
{

class VulkanTexture2D;

// Maps a 2D CUDA array on the device memory object referred to by deviceMem. deviceMem should have been created with a
// device memory object backing a 2D VkImage. This mapping makes use of Vulkan's
// export of device memory followed by import of this external memory by CUDA.

class CudaImage
{
public:
    CudaImage();
    ~CudaImage() = default;
    CUresult Init(const VkDevice device, const VulkanTexture2D* texture);
    void Shutdown();
    inline CUarray GetArray() const;


private:
    CUarray m_array;
    CUmipmappedArray m_mipmapArray;
    CUexternalMemory m_extMemory;
};

//---------------------------------------------------------------------------------------------------------------------
    inline CUarray CudaImage::GetArray() const { return m_array; }

} // end namespace webrtc
} // end namespace unity
