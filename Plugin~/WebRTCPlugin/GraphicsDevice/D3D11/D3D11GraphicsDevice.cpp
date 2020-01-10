#include "pch.h"
#include "D3D11GraphicsDevice.h"
#include "D3D11Texture2D.h"
#include "GraphicsDevice/GraphicsUtility.h"

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
    ID3D11Resource* nativeDest = reinterpret_cast<ID3D11Resource*>(dest->GetNativeTexturePtrV());
    ID3D11Resource* nativeSrc = reinterpret_cast<ID3D11Resource*>(nativeTexturePtr);
    if (nativeSrc == nativeDest)
        return false;
    if (nativeSrc == nullptr || nativeDest == nullptr)
        return false;
    m_d3d11Context->CopyResource(nativeDest, nativeSrc);
    return true;
}

//---------------------------------------------------------------------------------------------------------------------

rtc::scoped_refptr<webrtc::I420Buffer> D3D11GraphicsDevice::ConvertRGBToI420(ITexture2D* tex) {
    D3D11_MAPPED_SUBRESOURCE resource;

    ID3D11Resource* nativeTex = reinterpret_cast<ID3D11Resource*>(tex->GetNativeTexturePtrV());
    if (nullptr == nativeTex)
        return nullptr;

    const HRESULT hr = m_d3d11Context->Map(nativeTex, 0, D3D11_MAP_READ, 0, &resource);
    assert(hr == S_OK);
    if (hr!=S_OK) {
        return nullptr;
    }

    const uint32_t width = tex->GetWidth();
    const uint32_t height = tex->GetHeight();

    rtc::scoped_refptr<webrtc::I420Buffer> i420_buffer = GraphicsUtility::ConvertRGBToI420Buffer(
        width, height,
        resource.RowPitch, static_cast<uint8_t*>(resource.pData)
    );

    m_d3d11Context->Unmap(nativeTex, 0);
    return i420_buffer;
}

} //end namespace
