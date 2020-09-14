#include "pch.h"

#include <cuda.h>
#include <cudaD3D11.h>

#include "D3D12Texture2D.h"
#include "D3D12Constants.h"

namespace unity
{
namespace webrtc
{

//---------------------------------------------------------------------------------------------------------------------

D3D12Texture2D::D3D12Texture2D(uint32_t w, uint32_t h, ID3D12Resource* nativeTex, HANDLE handle
                               , ID3D11Texture2D* sharedTex)
    : ITexture2D(w,h)
    , m_nativeTexture(nativeTex), m_sharedHandle(handle), m_sharedTexture(sharedTex)
    , m_readbackResource(nullptr), m_nativeTextureFootprint(nullptr)
{

}

//----------------------------------------------------------------------------------------------------------------------

HRESULT D3D12Texture2D::CreateReadbackResource(ID3D12Device* device) {
    SAFE_DELETE(m_nativeTextureFootprint);
    SAFE_RELEASE(m_readbackResource);

    m_nativeTextureFootprint = new D3D12ResourceFootprint();
    D3D12_RESOURCE_DESC origDesc = m_nativeTexture->GetDesc();
    device->GetCopyableFootprints(&origDesc,0,1,0,
        &m_nativeTextureFootprint->Footprint,
        &m_nativeTextureFootprint->NumRows,
        &m_nativeTextureFootprint->RowSize,
        &m_nativeTextureFootprint->ResourceSize
    );

    //Create the readback buffer for the texture.
    D3D12_RESOURCE_DESC desc{};
    desc.Dimension = D3D12_RESOURCE_DIMENSION_BUFFER;
    desc.Alignment = 0;
    desc.Width = m_nativeTextureFootprint->ResourceSize;
    desc.Height= 1;
    desc.DepthOrArraySize = 1;
    desc.MipLevels = 1;
    desc.Format = DXGI_FORMAT_UNKNOWN;
    desc.SampleDesc.Count = 1;
    desc.SampleDesc.Quality = 0;
    desc.Layout = D3D12_TEXTURE_LAYOUT_ROW_MAJOR;
    desc.Flags = D3D12_RESOURCE_FLAG_NONE;

    const HRESULT hr = device->CreateCommittedResource(&D3D12_READBACK_HEAP_PROPS, D3D12_HEAP_FLAG_NONE,
        &desc, D3D12_RESOURCE_STATE_COPY_DEST, nullptr, IID_PPV_ARGS(&m_readbackResource)
    );
    return hr;
}


    std::unique_ptr<GpuMemoryBufferHandle> D3D12Texture2D::Map()
    {
        CUarray mappedArray;
        CUgraphicsResource resource;
        ID3D11Resource* pResource = static_cast<ID3D11Resource*>(this->GetNativeTexturePtrV());

        CUresult result = cuGraphicsD3D11RegisterResource(&resource, pResource, CU_GRAPHICS_REGISTER_FLAGS_SURFACE_LDST);
        if (result != CUDA_SUCCESS)
        {
            throw;
        }

        result = cuGraphicsMapResources(1, &resource, 0);
        if (result != CUDA_SUCCESS)
        {
            throw;
        }

         result = cuGraphicsSubResourceGetMappedArray(&mappedArray, resource, 0, 0);
        if (result != CUDA_SUCCESS)
        {
            throw;
        }
        std::unique_ptr<GpuMemoryBufferHandle> handle = std::make_unique<GpuMemoryBufferHandle>();
        handle->array = mappedArray;
        handle->resource = resource;
        return handle;
    }
} // end namespace webrtc
} // end namespace unity

