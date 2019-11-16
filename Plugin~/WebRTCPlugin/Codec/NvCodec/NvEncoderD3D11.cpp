#include "pch.h"
#include "nvEncodeAPI.h"
#include "NvEncoderD3D11.h"
#include "GraphicsDevice/IGraphicsDevice.h"
#include "GraphicsDevice/D3D11/D3D11Texture2D.h"

namespace WebRTC
{
    NvEncoderD3D11::NvEncoderD3D11(uint32_t nWidth, uint32_t nHeight, IGraphicsDevice* device) :
        NvEncoder(NV_ENC_DEVICE_TYPE_DIRECTX, NV_ENC_INPUT_RESOURCE_TYPE_DIRECTX, nWidth, nHeight, device)
    {
        InitEncoderResources();
        isNvEncoderSupported = true;
    }

    NvEncoderD3D11::~NvEncoderD3D11()
    {
    }

    void* NvEncoderD3D11::AllocateInputBuffer()
    {
        auto tex = m_device->CreateDefaultTextureV(width, height);
        return tex->GetNativeTexturePtrV();
    }

    ITexture2D* NvEncoderD3D11::CreateTexture2DFromInputBuffer(void* buffer)
    {
        return m_device->CreateDefaultTextureFromNativeV(width, height, buffer);
    }
}
