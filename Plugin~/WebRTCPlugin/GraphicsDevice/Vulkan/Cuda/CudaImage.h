#pragma once

#include "cuda.h"
#include <vulkan/vulkan.h>

namespace WebRTC {

class VulkanTexture2D;

// Maps a 2D CUDA array on the device memory object referred to by deviceMem. deviceMem should have been created with a
// device memory object backing a 2D VkImage. This mapping makes use of Vulkan's
// export of device memory followed by import of this external memory by CUDA.

class CudaImage
{
public:
    CudaImage();
    ~CudaImage();
    CUresult Init(const VkDevice device, const VulkanTexture2D* texture);
    void CleanUp();
    inline CUarray GetArray();


private:
    CUarray m_array;
    CUmipmappedArray m_mipmapArray;
    CUexternalMemory m_extMemory;
};

//---------------------------------------------------------------------------------------------------------------------
    inline CUarray CudaImage::GetArray() { return m_array; }

} //end namespace
