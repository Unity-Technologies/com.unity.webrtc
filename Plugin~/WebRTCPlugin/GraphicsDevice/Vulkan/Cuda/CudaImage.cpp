#include "pch.h"
#include "CudaImage.h"

#include <sstream> //ostringstream

#include "GraphicsDevice/Vulkan/VulkanTexture2D.h"
#include "GraphicsDevice/Vulkan/VulkanUtility.h"    //GetExportHandle()

namespace WebRTC {


//---------------------------------------------------------------------------------------------------------------------

CudaImage::CudaImage() : m_array(nullptr), m_mipmapArray(nullptr), m_extMemory(nullptr)
{
}

//---------------------------------------------------------------------------------------------------------------------

CUresult CudaImage::Init(const VkDevice device, const VulkanTexture2D* texture) {
    CUresult result = CUDA_SUCCESS;

    void *p = VulkanUtility::GetExportHandle(device, texture->GetTextureImageMemory());

    if (nullptr == p) {
        return CUDA_ERROR_INVALID_HANDLE;
    }

    CUDA_EXTERNAL_MEMORY_HANDLE_DESC memDesc = {};
#ifndef _WIN32
    memDesc.type = CU_EXTERNAL_MEMORY_HANDLE_TYPE_OPAQUE_FD;
#else
    memDesc.type = CU_EXTERNAL_MEMORY_HANDLE_TYPE_OPAQUE_WIN32;
#endif
    memDesc.handle.fd = static_cast<int>(reinterpret_cast<uintptr_t>(p));
    memDesc.size = texture->GetTextureImageMemorySize();

    if ((result=cuImportExternalMemory(&m_extMemory, &memDesc)) != CUDA_SUCCESS) {
        return result;
    }

    const VkExtent2D extent = { texture->GetWidth(), texture->GetHeight() };

    CUDA_ARRAY3D_DESCRIPTOR arrayDesc = {};
    arrayDesc.Width = extent.width;
    arrayDesc.Height = extent.height;
    arrayDesc.Depth = 0; /* CUDA 2D arrays are defined to have depth 0 */
    arrayDesc.Format = CU_AD_FORMAT_UNSIGNED_INT32;
    arrayDesc.NumChannels = 1;
    arrayDesc.Flags = CUDA_ARRAY3D_SURFACE_LDST |
                      CUDA_ARRAY3D_COLOR_ATTACHMENT;

    CUDA_EXTERNAL_MEMORY_MIPMAPPED_ARRAY_DESC mipmapArrayDesc = {};
    mipmapArrayDesc.arrayDesc = arrayDesc;
    mipmapArrayDesc.numLevels = 1;

    result = cuExternalMemoryGetMappedMipmappedArray(&m_mipmapArray, m_extMemory,
                 &mipmapArrayDesc);
    if (result != CUDA_SUCCESS) {
        return result;
    }

    result = cuMipmappedArrayGetLevel(&m_array, m_mipmapArray, 0);
    return result;
}

//---------------------------------------------------------------------------------------------------------------------

void CudaImage::Shutdown() {
    m_array = nullptr;

    if (nullptr != m_mipmapArray) {
        cuMipmappedArrayDestroy(m_mipmapArray);
        m_mipmapArray = nullptr;
    }
    if (nullptr != m_extMemory) {
        cuDestroyExternalMemory(m_extMemory);
        m_extMemory = nullptr;
    }


}

} //end namespace
