#include "pch.h"
#include "D3D11GraphicsDevice.h"
#include "D3D11Texture2D.h"

namespace WebRTC {

D3D11GraphicsDevice::D3D11GraphicsDevice(ID3D11Device* nativeDevice) : m_d3d11Device(nativeDevice)
{
    m_d3d11Device->GetImmediateContext(&m_d3d11Context);
}


//---------------------------------------------------------------------------------------------------------------------
D3D11GraphicsDevice::~D3D11GraphicsDevice() {
    SAFE_RELEASE(m_d3d11Context);
}

//---------------------------------------------------------------------------------------------------------------------
bool D3D11GraphicsDevice::InitV() {
    return true;
}

//---------------------------------------------------------------------------------------------------------------------

void D3D11GraphicsDevice::ShutdownV() {
}

//---------------------------------------------------------------------------------------------------------------------
ITexture2D* D3D11GraphicsDevice::CreateDefaultTextureV(uint32_t w, uint32_t h) {

    ID3D11Texture2D* texture = nullptr;
    D3D11_TEXTURE2D_DESC desc = { 0 };
    desc.Width = w;
    desc.Height = h;
    desc.MipLevels = 1;
    desc.ArraySize = 1;
    desc.Format = DXGI_FORMAT_B8G8R8A8_UNORM;
    desc.SampleDesc.Count = 1;
    desc.Usage = D3D11_USAGE_DEFAULT;
    desc.BindFlags = 0;
    desc.CPUAccessFlags = 0;
    HRESULT r = m_d3d11Device->CreateTexture2D(&desc, NULL, &texture);
    return new D3D11Texture2D(w,h,texture);
}

//---------------------------------------------------------------------------------------------------------------------
ITexture2D* D3D11GraphicsDevice::CreateCPUReadTextureV(uint32_t w, uint32_t h) {

    ID3D11Texture2D* texture = nullptr;
    D3D11_TEXTURE2D_DESC desc = { 0 };
    desc.Width = w;
    desc.Height = h;
    desc.MipLevels = 1;
    desc.ArraySize = 1;
    desc.Format = DXGI_FORMAT_B8G8R8A8_UNORM;
    desc.SampleDesc.Count = 1;
    desc.Usage = D3D11_USAGE_STAGING;
    desc.BindFlags = 0;
    desc.CPUAccessFlags = D3D11_CPU_ACCESS_READ;
    HRESULT r = m_d3d11Device->CreateTexture2D(&desc, NULL, &texture);
    return new D3D11Texture2D(w, h, texture);
}

//---------------------------------------------------------------------------------------------------------------------
ITexture2D* D3D11GraphicsDevice::CreateDefaultTextureFromNativeV(uint32_t w, uint32_t h, void* nativeTexturePtr) {
    assert(nullptr!=nativeTexturePtr);
    ID3D11Texture2D* texPtr = reinterpret_cast<ID3D11Texture2D*>(nativeTexturePtr);
    texPtr->AddRef();
    return new D3D11Texture2D(w,h,texPtr);
}


//---------------------------------------------------------------------------------------------------------------------
bool D3D11GraphicsDevice::CopyResourceV(ITexture2D* dest, ITexture2D* src) {
    ID3D11Resource* nativeDest = reinterpret_cast<ID3D11Resource*>(dest->GetNativeTexturePtrV());
    ID3D11Resource* nativeSrc = reinterpret_cast<ID3D11Texture2D*>(src->GetNativeTexturePtrV());
    if (nativeSrc == nativeDest)
        return false;
    if (nativeSrc == nullptr || nativeDest == nullptr)
        return false;
    m_d3d11Context->CopyResource(nativeDest, nativeSrc);
    return true;
}

//---------------------------------------------------------------------------------------------------------------------
bool D3D11GraphicsDevice::CopyResourceFromNativeV(ITexture2D* dest, void* nativeTexturePtr) {
    auto nativeDest = reinterpret_cast<ID3D11Resource*>(dest->GetNativeTexturePtrV());
    auto nativeSrc = reinterpret_cast<ID3D11Resource*>(nativeTexturePtr);
    if (nativeSrc == nativeDest)
        return false;
    if (nativeSrc == nullptr || nativeDest == nullptr)
        return false;
    m_d3d11Context->CopyResource(nativeDest, nativeSrc);
    return true;
}

rtc::scoped_refptr<webrtc::I420Buffer> D3D11GraphicsDevice::ConvertRGBToI420(ITexture2D* tex)
{
    D3D11_MAPPED_SUBRESOURCE resource;
    HRESULT hr = m_d3d11Context->Map((ID3D11Resource*)tex->GetNativeTexturePtrV(), 0, D3D11_MAP_READ, 0, &resource);
    assert(hr == S_OK);
    if (hr!=S_OK)
    {
        return nullptr;
    }

    int width = tex->GetWidth();
    int height = tex->GetHeight();
    rtc::scoped_refptr<webrtc::I420Buffer> i420_buffer = webrtc::I420Buffer::Create(width, height);

    int StrideY = i420_buffer->StrideY();
    int StrideU = i420_buffer->StrideU();
    int StrideV = i420_buffer->StrideV();

    int yIndex = 0;
    int uIndex = 0;
    int vIndex = 0;

    uint8_t* yuv_y = i420_buffer->MutableDataY();
    uint8_t* yuv_u = i420_buffer->MutableDataU();
    uint8_t* yuv_v = i420_buffer->MutableDataV();

    for (int i = 0; i < height; i++)
    {
        for (int j = 0; j < width; j++)
        {
            const uint8_t* rgba_src = (const uint8_t*)resource.pData;
            int R, G, B, Y, U, V;
            int startIndex = i * resource.RowPitch + j * 4;
            B = rgba_src[startIndex + 0];
            G = rgba_src[startIndex + 1];
            R = rgba_src[startIndex + 2];

            Y = ((66 * R + 129 * G + 25 * B + 128) >> 8) + 16;
            U = ((-38 * R - 74 * G + 112 * B + 128) >> 8) + 128;
            V = ((112 * R - 94 * G - 18 * B + 128) >> 8) + 128;

            yuv_y[yIndex++] = (uint8_t)((Y < 0) ? 0 : ((Y > 255) ? 255 : Y));
            if (i % 2 == 0 && j % 2 == 0)
            {
                yuv_u[uIndex++] = (uint8_t)((U < 0) ? 0 : ((U > 255) ? 255 : U));
                yuv_v[vIndex++] = (uint8_t)((V < 0) ? 0 : ((V > 255) ? 255 : V));
            }
        }
    }

    m_d3d11Context->Unmap((ID3D11Resource*)tex->GetEncodeTexturePtrV(), 0);
    return i420_buffer;
}

} //end namespace
