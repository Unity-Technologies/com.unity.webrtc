#include "pch.h"
#include "D3D12GraphicsDevice.h"
#include "D3D12Texture2D.h"

namespace WebRTC {

D3D12GraphicsDevice::D3D12GraphicsDevice(ID3D12Device* nativeDevice) : m_d3d12Device(nativeDevice)
{
//    m_d3d12Device->GetImmediateContext(&m_d3d11Context);
}


//---------------------------------------------------------------------------------------------------------------------
D3D12GraphicsDevice::~D3D12GraphicsDevice() {

}

//---------------------------------------------------------------------------------------------------------------------

void D3D12GraphicsDevice::ShutdownV() {
}

//---------------------------------------------------------------------------------------------------------------------
ITexture2D* D3D12GraphicsDevice::CreateEncoderInputTextureV(uint32_t w, uint32_t h) {

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

//---------------------------------------------------------------------------------------------------------------------
ITexture2D* D3D12GraphicsDevice::CreateEncoderInputTextureV(uint32_t w, uint32_t h, void* nativeTexturePtr) {
    assert(nullptr!=nativeTexturePtr);
    ID3D12Resource* texPtr = reinterpret_cast<ID3D12Resource*>(nativeTexturePtr);
    texPtr->AddRef();
    return new D3D12Texture2D(w,h,texPtr);
}


//---------------------------------------------------------------------------------------------------------------------
void D3D12GraphicsDevice::CopyNativeResourceV(void* dest, void* src) {
    //[TODO-sin: 2019-10-15] Implement copying native resource
}

} //end namespace
