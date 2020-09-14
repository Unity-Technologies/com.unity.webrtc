#include "pch.h"

#include "D3D11Texture2D.h"

#include "NvCodecUtils.h"
#include <cudaD3D11.h>

namespace unity
{
namespace webrtc
{

    D3D11Texture2D::D3D11Texture2D(uint32_t w, uint32_t h, ID3D11Texture2D* tex)
        : ITexture2D(w, h)
        , m_texture(tex)
    {
    }

    std::unique_ptr<GpuMemoryBufferHandle> D3D11Texture2D::Map()
    {
        CUarray mappedArray;
        CUgraphicsResource resource;
        ID3D11Resource* pResource = static_cast<ID3D11Resource*>(this->GetNativeTexturePtrV());

        CUresult result =
            cuGraphicsD3D11RegisterResource(&resource, pResource, CU_GRAPHICS_REGISTER_FLAGS_SURFACE_LDST);
        if (result != CUDA_SUCCESS)
        {
            RTC_LOG(LS_ERROR) << "cuGraphicsD3D11RegisterResource";
            throw;
        }

        result = cuGraphicsMapResources(1, &resource, 0);
        if (result != CUDA_SUCCESS)
        {
            RTC_LOG(LS_ERROR) << "cuGraphicsMapResources";
            throw;
        }

        result = cuGraphicsSubResourceGetMappedArray(&mappedArray, resource, 0, 0);
        if (result != CUDA_SUCCESS)
        {
            RTC_LOG(LS_ERROR) << "cuGraphicsSubResourceGetMappedArray";
            throw;
        }
        std::unique_ptr<GpuMemoryBufferHandle> handle = std::make_unique<GpuMemoryBufferHandle>();
        handle->array = mappedArray;
        handle->resource = resource;
        return handle;
    }
} // end namespace webrtc
} // end namespace unity
