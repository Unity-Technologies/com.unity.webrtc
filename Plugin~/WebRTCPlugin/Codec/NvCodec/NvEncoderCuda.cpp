#include "pch.h"
#include "nvEncodeAPI.h"
#include "NvEncoderCuda.h"
#include "GraphicsDevice/IGraphicsDevice.h"
#include "GraphicsDevice/ITexture2D.h"

namespace WebRTC {

    NvEncoderCuda::NvEncoderCuda(const uint32_t nWidth, const uint32_t nHeight, IGraphicsDevice* device) :
        NvEncoder(NV_ENC_DEVICE_TYPE_CUDA, NV_ENC_INPUT_RESOURCE_TYPE_CUDAARRAY, nWidth, nHeight, device)
    {
    }

    void* NvEncoderCuda::AllocateInputResourceV(ITexture2D* tex) {
        return tex->GetNativeTexturePtrV();
    }

}
