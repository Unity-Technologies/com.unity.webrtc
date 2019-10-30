#include "pch.h"
#include "D3D12GraphicsDevice.h"
#include "D3D12Texture2D.h"

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

    return CreateSharedD3D12Texture(w,h);
}

//---------------------------------------------------------------------------------------------------------------------
ITexture2D* D3D12GraphicsDevice::CreateDefaultTextureFromNativeV(uint32_t w, uint32_t h, void* nativeTexturePtr) {
    return CreateSharedD3D12Texture(w,h);
}


//---------------------------------------------------------------------------------------------------------------------
void D3D12GraphicsDevice::CopyResourceV(ITexture2D* dest, ITexture2D* src) {
    //[TODO-sin: 2019-10-15] Implement copying native resource
}

//---------------------------------------------------------------------------------------------------------------------
void D3D12GraphicsDevice::CopyResourceFromNativeV(ITexture2D* dest, void* nativeTexturePtr) {
    ID3D12Resource* src = reinterpret_cast<ID3D12Resource*>(nativeTexturePtr);
    if (dest->GetNativeTexturePtrV() == src)
        return;

    //[TODO-sin: 2019-10-30] Implement copying native resource
   
}


//---------------------------------------------------------------------------------------------------------------------

ITexture2D* D3D12GraphicsDevice::CreateSharedD3D12Texture(uint32_t w, uint32_t h) {
    //[Note-sin: 2019-10-30] Taken from RaytracedHardShadow
    // note: sharing textures with d3d11 requires some flags and restrictions:
    // - MipLevels must be 1
    // - D3D12_HEAP_FLAG_SHARED for heap flags
    // - D3D12_RESOURCE_FLAG_ALLOW_RENDER_TARGET and D3D12_RESOURCE_FLAG_ALLOW_SIMULTANEOUS_ACCESS for resource flags

    D3D12_RESOURCE_DESC desc{};
    desc.Dimension = D3D12_RESOURCE_DIMENSION_TEXTURE2D;
    desc.Alignment = 0;
    desc.Width = w;
    desc.Height = h;
    desc.DepthOrArraySize = 1;
    desc.MipLevels = 1;
    desc.Format = DXGI_FORMAT_B8G8R8A8_UNORM;
    desc.SampleDesc.Count = 1;
    desc.SampleDesc.Quality = 0;
    desc.Layout = D3D12_TEXTURE_LAYOUT_UNKNOWN;
    desc.Flags = D3D12_RESOURCE_FLAG_ALLOW_UNORDERED_ACCESS;
    desc.Flags |= D3D12_RESOURCE_FLAG_ALLOW_RENDER_TARGET | D3D12_RESOURCE_FLAG_ALLOW_SIMULTANEOUS_ACCESS;

    D3D12_HEAP_FLAGS flags = D3D12_HEAP_FLAG_SHARED;
    D3D12_RESOURCE_STATES initialState = D3D12_RESOURCE_STATE_COMMON; //D3D12_RESOURCE_STATE_COPY_DEST


    ID3D12Resource* nativeTex = nullptr;
    m_d3d12Device->CreateCommittedResource(&DEFAULT_HEAP_PROPS, flags, &desc, initialState, nullptr, IID_PPV_ARGS(&nativeTex));

    ID3D11Texture2D* sharedTex = nullptr;
    HANDLE handle = nullptr;   
    HRESULT hr = m_d3d12Device->CreateSharedHandle(nativeTex, nullptr, GENERIC_ALL, nullptr, &handle);
    if (SUCCEEDED(hr)) {
        //ID3D11Device::OpenSharedHandle() doesn't accept handles created by d3d12. OpenSharedHandle1() is needed.
        hr = m_d3d11Device->OpenSharedResource1(handle, IID_PPV_ARGS(&sharedTex));
    }

    return new D3D12Texture2D(w,h,nativeTex, handle, sharedTex);
}



} //end namespace
