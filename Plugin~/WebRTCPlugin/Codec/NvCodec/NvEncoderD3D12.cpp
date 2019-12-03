#include "pch.h"
#include "nvEncodeAPI.h"
#include "NvEncoderD3D12.h"
#include "GraphicsDevice/IGraphicsDevice.h"
#include "GraphicsDevice/ITexture2D.h"

namespace WebRTC
{
    NvEncoderD3D12::NvEncoderD3D12(uint32_t nWidth, uint32_t nHeight, IGraphicsDevice* device) :
        NvEncoder(NV_ENC_DEVICE_TYPE_DIRECTX, NV_ENC_INPUT_RESOURCE_TYPE_DIRECTX, nWidth, nHeight, device)
    {
    }

    NvEncoderD3D12::~NvEncoderD3D12()
    {
    }

    void* NvEncoderD3D12::AllocateInputResourceV(ITexture2D* tex) {
        return tex->GetNativeTexturePtrV();
    }

}
