#include "pch.h"
#include "nvEncodeAPI.h"
#include "NvEncoderD3D11.h"
#include "GraphicsDevice/IGraphicsDevice.h"
#include "GraphicsDevice/ITexture2D.h"

namespace unity
{
namespace webrtc
{
    
    NvEncoderD3D11::NvEncoderD3D11(uint32_t nWidth, uint32_t nHeight, IGraphicsDevice* device, UnityColorSpace colorSpace) :
        NvEncoder(NV_ENC_DEVICE_TYPE_DIRECTX, NV_ENC_INPUT_RESOURCE_TYPE_DIRECTX, NV_ENC_BUFFER_FORMAT_ARGB, nWidth, nHeight, device, colorSpace)
    {
    }

    NvEncoderD3D11::~NvEncoderD3D11()
    {
    }

    void* NvEncoderD3D11::AllocateInputResourceV(ITexture2D* tex) {
        return tex->GetNativeTexturePtrV();
    }

} // end namespace webrtc
} // end namespace unity
