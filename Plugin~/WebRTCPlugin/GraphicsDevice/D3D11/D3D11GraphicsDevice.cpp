#include "pch.h"
#include "D3D11GraphicsDevice.h"

#include <cuda.h>
#include <cudaD3D11.h>
#include <wrl/client.h>


#include "D3D11Texture2D.h"
#include "NvCodecUtils.h"
#include "GraphicsDevice/GraphicsUtility.h"
#include "GraphicsDevice/Cuda/GpuMemoryBufferCudaHandle.h"

using namespace Microsoft::WRL;
using namespace ::webrtc;

namespace unity
{
namespace webrtc
{

D3D11GraphicsDevice::D3D11GraphicsDevice(
    ID3D11Device* nativeDevice, UnityGfxRenderer renderer) 
    : IGraphicsDevice(renderer)
    , m_d3d11Device(nativeDevice)
{
    // Enable multithread protection
    ComPtr<ID3D11Multithread> thread;
    m_d3d11Device->QueryInterface(IID_PPV_ARGS(&thread));
    thread->SetMultithreadProtected(true);
}


//---------------------------------------------------------------------------------------------------------------------
D3D11GraphicsDevice::~D3D11GraphicsDevice() {
}

//---------------------------------------------------------------------------------------------------------------------
bool D3D11GraphicsDevice::InitV() {
    m_isCudaSupport = CUDA_SUCCESS == m_cudaContext.Init(m_d3d11Device);
    return true;
}

//---------------------------------------------------------------------------------------------------------------------

void D3D11GraphicsDevice::ShutdownV() {
    m_cudaContext.Shutdown();
}

//---------------------------------------------------------------------------------------------------------------------
ITexture2D* D3D11GraphicsDevice::CreateDefaultTextureV(uint32_t w, uint32_t h, UnityRenderingExtTextureFormat textureFormat) {

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
    HRESULT result = m_d3d11Device->CreateTexture2D(&desc, NULL, &texture);
    if (result != S_OK)
    {
        RTC_LOG(LS_INFO) << "CreateTexture2D failed. error:" << result;
        return nullptr;
    }
    return new D3D11Texture2D(w,h,texture);
}

//---------------------------------------------------------------------------------------------------------------------
ITexture2D* D3D11GraphicsDevice::CreateCPUReadTextureV(
    uint32_t w, uint32_t h, UnityRenderingExtTextureFormat textureFormat) {

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
    HRESULT hr = m_d3d11Device->CreateTexture2D(&desc, NULL, &texture);
    if (hr != S_OK) {
        return nullptr;
    }
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

    ComPtr<ID3D11DeviceContext> context;
    m_d3d11Device->GetImmediateContext(context.GetAddressOf());
    context->CopyResource(nativeDest, nativeSrc);
    context->Flush();
    return true;
}

//---------------------------------------------------------------------------------------------------------------------
bool D3D11GraphicsDevice::CopyResourceFromNativeV(ITexture2D* dest, void* nativeTexturePtr) {
    ID3D11Resource* nativeDest = reinterpret_cast<ID3D11Resource*>(
        dest->GetNativeTexturePtrV());
    ID3D11Resource* nativeSrc = reinterpret_cast<ID3D11Resource*>(nativeTexturePtr);
    if (nativeSrc == nativeDest)
        return false;
    if (nativeSrc == nullptr || nativeDest == nullptr)
        return false;
    ComPtr<ID3D11DeviceContext> context;
    m_d3d11Device->GetImmediateContext(context.GetAddressOf());
    context->CopyResource(nativeDest, nativeSrc);
    context->Flush();
    return true;
}

//---------------------------------------------------------------------------------------------------------------------

rtc::scoped_refptr<I420Buffer> D3D11GraphicsDevice::ConvertRGBToI420(ITexture2D* tex) {
    D3D11_MAPPED_SUBRESOURCE pMappedResource;

    ID3D11Resource* pResource =
        reinterpret_cast<ID3D11Resource*>(tex->GetNativeTexturePtrV());
    if (nullptr == pResource)
         return nullptr;

    ComPtr<ID3D11DeviceContext> context;
    m_d3d11Device->GetImmediateContext(context.GetAddressOf());

    const HRESULT hr = context->Map(
        pResource, 0, D3D11_MAP_READ, 0, &pMappedResource);
    if (hr != S_OK)
        return nullptr;

    const uint32_t width = tex->GetWidth();
    const uint32_t height = tex->GetHeight();

    // todo(kazuki) replace to using libyuv function
    rtc::scoped_refptr<I420Buffer> i420_buffer =
        GraphicsUtility::ConvertRGBToI420Buffer(
        width, height, pMappedResource.RowPitch,
            static_cast<uint8_t*>(pMappedResource.pData));

    context->Unmap(pResource, 0);
    return i420_buffer;

}

    std::unique_ptr<GpuMemoryBufferHandle> D3D11GraphicsDevice::Map(ITexture2D* texture)
    {
        if(!IsCudaSupport())
            return nullptr;

        CUgraphicsResource resource;
        ID3D11Resource* pResource = static_cast<ID3D11Resource*>(texture->GetNativeTexturePtrV());

        // set context on the thread.
        cuCtxPushCurrent(GetCUcontext());

        CUresult result = cuGraphicsD3D11RegisterResource(&resource, pResource, CU_GRAPHICS_REGISTER_FLAGS_SURFACE_LDST);
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

        CUarray mappedArray;
        result = cuGraphicsSubResourceGetMappedArray(&mappedArray, resource, 0, 0);
        if (result != CUDA_SUCCESS)
        {
            RTC_LOG(LS_ERROR) << "cuGraphicsSubResourceGetMappedArray";
            throw;
        }
        cuCtxPopCurrent(NULL);

        std::unique_ptr<GpuMemoryBufferCudaHandle> handle = std::make_unique<GpuMemoryBufferCudaHandle>();
        handle->context = GetCUcontext();
        handle->mappedArray = mappedArray;
        handle->resource = resource;
        return handle;
    }

} //end namespace webrtc
} //end namespace unity
