#include "pch.h"
#include "D3D12GraphicsDevice.h"
#include "D3D12Texture2D.h"
#include "../D3D11/D3D11Texture2D.h"

namespace WebRTC {

const D3D12_HEAP_PROPERTIES DEFAULT_HEAP_PROPS = {
    D3D12_HEAP_TYPE_DEFAULT,
    D3D12_CPU_PAGE_PROPERTY_UNKNOWN,
    D3D12_MEMORY_POOL_UNKNOWN,
    0,
    0
};

//---------------------------------------------------------------------------------------------------------------------

D3D12GraphicsDevice::D3D12GraphicsDevice(ID3D12Device* nativeDevice) : m_d3d12Device(nativeDevice)
    , m_d3d11Device(nullptr), m_d3d11Context(nullptr)
{
}


//---------------------------------------------------------------------------------------------------------------------
D3D12GraphicsDevice::~D3D12GraphicsDevice() {

}

//---------------------------------------------------------------------------------------------------------------------

void D3D12GraphicsDevice::InitV() {

    ID3D11Device* legacyDevice;
    ID3D11DeviceContext* legacyContext;

    D3D11CreateDevice(
        nullptr,
        D3D_DRIVER_TYPE_HARDWARE,
        nullptr,
        0,
        nullptr,
        0,
        D3D11_SDK_VERSION,
        &legacyDevice,
        nullptr,
        &legacyContext);

    legacyDevice->QueryInterface(IID_PPV_ARGS(&m_d3d11Device));

    legacyDevice->GetImmediateContext(&legacyContext);
    legacyContext->QueryInterface(IID_PPV_ARGS(&m_d3d11Context));
}

//---------------------------------------------------------------------------------------------------------------------
void D3D12GraphicsDevice::ShutdownV() {
    SAFE_RELEASE(m_d3d11Device);
    SAFE_RELEASE(m_d3d11Context);
}

//---------------------------------------------------------------------------------------------------------------------
ITexture2D* D3D12GraphicsDevice::CreateDefaultTextureV(uint32_t w, uint32_t h) {

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
ITexture2D* D3D12GraphicsDevice::CreateDefaultTextureFromNativeV(uint32_t w, uint32_t h, void* nativeTexturePtr) {
    assert(nullptr!=nativeTexturePtr);
    //ID3D12Resource* texPtr = reinterpret_cast<ID3D12Resource*>(nativeTexturePtr);
    //texPtr->AddRef();

    //[TODO-sin: 2019-10-30] Copy resource from D3D12 to D3D11

    return CreateDefaultTextureV(w,h);
}


//---------------------------------------------------------------------------------------------------------------------
void D3D12GraphicsDevice::CopyNativeResourceV(void* dest, void* src) {
    //[TODO-sin: 2019-10-15] Implement copying native resource
}

//---------------------------------------------------------------------------------------------------------------------

ITexture2D* D3D12GraphicsDevice::CreateD3D12Texture(uint32_t w, uint32_t h) {
    // Describe and create a Texture2D.
    ID3D12Resource* texture = nullptr;
    D3D12_RESOURCE_DESC textureDesc = {  };
    textureDesc.MipLevels = 1;
    textureDesc.Format = DXGI_FORMAT_B8G8R8A8_UNORM;
    textureDesc.Width = w;
    textureDesc.Height = h;
    textureDesc.Flags = D3D12_RESOURCE_FLAG_NONE; 
    textureDesc.DepthOrArraySize = 1;
    textureDesc.SampleDesc.Count = 1;
    textureDesc.SampleDesc.Quality = 0;
    textureDesc.Dimension = D3D12_RESOURCE_DIMENSION_TEXTURE2D;

    D3D12_HEAP_PROPERTIES heapProperties = {};
    heapProperties.Type = D3D12_HEAP_TYPE_DEFAULT;
    heapProperties.CPUPageProperty = D3D12_CPU_PAGE_PROPERTY_UNKNOWN;
    heapProperties.MemoryPoolPreference = D3D12_MEMORY_POOL_UNKNOWN;
    heapProperties.CreationNodeMask = 1;
    heapProperties.VisibleNodeMask = 1;

    m_d3d12Device->CreateCommittedResource(
        &heapProperties,
        D3D12_HEAP_FLAG_NONE,
        &textureDesc,
        D3D12_RESOURCE_STATE_COPY_DEST,
        nullptr,
        IID_PPV_ARGS(&texture));

    return new D3D12Texture2D(w,h,texture);

}


} //end namespace
