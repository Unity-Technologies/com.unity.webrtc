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

}

//---------------------------------------------------------------------------------------------------------------------
void D3D11GraphicsDevice::InitV() {
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
ITexture2D* D3D11GraphicsDevice::CreateDefaultTextureFromNativeV(uint32_t w, uint32_t h, void* nativeTexturePtr) {
    assert(nullptr!=nativeTexturePtr);
    ID3D11Texture2D* texPtr = reinterpret_cast<ID3D11Texture2D*>(nativeTexturePtr);
    texPtr->AddRef();
    return new D3D11Texture2D(w,h,texPtr);
}


//---------------------------------------------------------------------------------------------------------------------
void D3D11GraphicsDevice::CopyResourceV(ITexture2D* dest, ITexture2D* src) {
    ID3D11Resource* nativeDest = reinterpret_cast<ID3D11Resource*>(dest->GetNativeTexturePtrV());
    ID3D11Resource* nativeSrc= reinterpret_cast<ID3D11Texture2D*>(src->GetNativeTexturePtrV());
    m_d3d11Context->CopyResource(nativeDest, nativeSrc);
}

//---------------------------------------------------------------------------------------------------------------------
void D3D11GraphicsDevice::CopyResourceFromNativeV(ITexture2D* dest, void* nativeTexturePtr) {
    if (dest->GetNativeTexturePtrV() == nativeTexturePtr)
        return;

    //[Note-sin: 2019-10-30] Do we need to implement this for RenderStreaming ?
    DebugWarning("D3D11: CopyResourceFromNativeV() is not supported");

}

} //end namespace
