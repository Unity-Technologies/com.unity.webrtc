#include "pch.h"
#include "nvEncodeAPI.h"
#include "NvEncoderCuda.h"

#include <cuda.h>

#include "GraphicsDevice/IGraphicsDevice.h"
#include "GraphicsDevice/ITexture2D.h"
#include "GraphicsDevice/Vulkan/VulkanGraphicsDevice.h"

namespace unity
{
namespace webrtc
{

    NvEncoderCuda::NvEncoderCuda(const uint32_t nWidth, const uint32_t nHeight, IGraphicsDevice* device) :
        NvEncoder(NV_ENC_DEVICE_TYPE_CUDA, NV_ENC_INPUT_RESOURCE_TYPE_CUDAARRAY, NV_ENC_BUFFER_FORMAT_ARGB, nWidth, nHeight, device)
    {
    }

    void NvEncoderCuda::InitV()
    {
        CUcontext context = static_cast<CUcontext>(m_device->GetEncodeDevicePtrV());

        CUcontext current;
        CUresult result = cuCtxGetCurrent(&current);
        if (result != CUDA_SUCCESS)
        {
            throw;
        }
        if (context != current)
        {
            result = cuCtxSetCurrent(context);
            if (result != CUDA_SUCCESS)
            {
                throw;
            }
        }
        NvEncoder::InitV();
    }


    void* NvEncoderCuda::AllocateInputResourceV(ITexture2D* tex) {
        return tex->GetEncodeTexturePtrV();
    }

} // end namespace webrtc
} // end namespace unity
