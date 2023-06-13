#include "pch.h"

#include "D3D12Constants.h"
#include "D3D12Texture2D.h"

// nonstandard extension used : class rvalue used as lvalue
#pragma clang diagnostic ignored "-Wlanguage-extension-token"

namespace unity
{
namespace webrtc
{

    //---------------------------------------------------------------------------------------------------------------------

    D3D12Texture2D::D3D12Texture2D(uint32_t w, uint32_t h, ID3D12Resource* nativeTex, HANDLE handle)
        : ITexture2D(w, h)
        , m_nativeTexture(nativeTex)
        , m_sharedHandle(handle)
        , m_readbackResource(nullptr)
        , m_syncCount(0)

    {
    }

    //----------------------------------------------------------------------------------------------------------------------

    D3D12Texture2D* D3D12Texture2D::CreateReadbackResource(ID3D12Device* device, uint32_t w, uint32_t h)
    {
        D3D12_RESOURCE_DESC desc {};
        desc.Dimension = D3D12_RESOURCE_DIMENSION_TEXTURE2D;
        desc.Alignment = 0;
        desc.Width = w;
        desc.Height = h;
        desc.DepthOrArraySize = 1;
        desc.MipLevels = 1;
        desc.Format =
            DXGI_FORMAT_B8G8R8A8_UNORM; // We only support this format which has 4 bytes -> DX12_BYTES_PER_PIXEL
        desc.SampleDesc.Count = 1;
        desc.SampleDesc.Quality = 0;
        desc.Layout = D3D12_TEXTURE_LAYOUT_UNKNOWN;
        desc.Flags = D3D12_RESOURCE_FLAG_ALLOW_UNORDERED_ACCESS;
        desc.Flags |= D3D12_RESOURCE_FLAG_ALLOW_RENDER_TARGET | D3D12_RESOURCE_FLAG_ALLOW_SIMULTANEOUS_ACCESS;

        D3D12ResourceFootprint footprint = {};
        device->GetCopyableFootprints(
            &desc, 0, 1, 0, &footprint.Footprint, &footprint.NumRows, &footprint.RowSize, &footprint.ResourceSize);

        // Create the readback buffer for the texture.
        D3D12_RESOURCE_DESC descBuffer {};
        descBuffer.Dimension = D3D12_RESOURCE_DIMENSION_BUFFER;
        descBuffer.Alignment = 0;
        descBuffer.Width = footprint.ResourceSize;
        descBuffer.Height = 1;
        descBuffer.DepthOrArraySize = 1;
        descBuffer.MipLevels = 1;
        descBuffer.Format = DXGI_FORMAT_UNKNOWN;
        descBuffer.SampleDesc.Count = 1;
        descBuffer.SampleDesc.Quality = 0;
        descBuffer.Layout = D3D12_TEXTURE_LAYOUT_ROW_MAJOR;
        descBuffer.Flags = D3D12_RESOURCE_FLAG_NONE;

        ID3D12Resource* resource = nullptr;
        const HRESULT hr = device->CreateCommittedResource(
            &D3D12_READBACK_HEAP_PROPS,
            D3D12_HEAP_FLAG_NONE,
            &descBuffer,
            D3D12_RESOURCE_STATE_COPY_DEST,
            nullptr,
            IID_PPV_ARGS(&resource));
        if (hr != S_OK)
        {
            RTC_LOG(LS_INFO) << "ID3D12Device::CreateCommittedResource failed. " << hr;
            return nullptr;
        }

        D3D12Texture2D* texture = new D3D12Texture2D(w, h, nullptr);
        texture->m_readbackResource = resource;
        texture->m_nativeTextureFootprint = footprint;
        return texture;
    }
} // end namespace webrtc
} // end namespace unity
