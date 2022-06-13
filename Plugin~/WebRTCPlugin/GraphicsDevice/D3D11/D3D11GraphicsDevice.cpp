#include "pch.h"

#include <third_party/libyuv/include/libyuv/convert.h>

#include "D3D11GraphicsDevice.h"
#include "D3D11Texture2D.h"
#include "GraphicsDevice/Cuda/GpuMemoryBufferCudaHandle.h"
#include "GraphicsDevice/GraphicsUtility.h"
#include "NvCodecUtils.h"

using namespace ::webrtc;
using namespace Microsoft::WRL;

namespace unity
{
namespace webrtc
{

    D3D11GraphicsDevice::D3D11GraphicsDevice(
        ID3D11Device* nativeDevice, UnityGfxRenderer renderer, ProfilerMarkerFactory* profiler)
        : IGraphicsDevice(renderer, profiler)
        , m_d3d11Device(nativeDevice)
        , m_isCudaSupport(false)
    {
        // Enable multithread protection
        ComPtr<ID3D11Multithread> thread;
        m_d3d11Device->QueryInterface<ID3D11Multithread>(&thread);
        thread->SetMultithreadProtected(true);
    }

    D3D11GraphicsDevice::~D3D11GraphicsDevice() { }

    bool D3D11GraphicsDevice::InitV()
    {
        CUresult ret = m_cudaContext.Init(m_d3d11Device);
        if (ret == CUDA_SUCCESS)
        {
            m_isCudaSupport = true;
        }
        return true;
    }

    //---------------------------------------------------------------------------------------------------------------------

    void D3D11GraphicsDevice::ShutdownV() { m_cudaContext.Shutdown(); }

    //---------------------------------------------------------------------------------------------------------------------
    ITexture2D*
    D3D11GraphicsDevice::CreateDefaultTextureV(uint32_t w, uint32_t h, UnityRenderingExtTextureFormat textureFormat)
    {

        ID3D11Texture2D* texture = nullptr;
        D3D11_TEXTURE2D_DESC desc = {};
        desc.Width = w;
        desc.Height = h;
        desc.MipLevels = 1;
        desc.ArraySize = 1;
        desc.Format = DXGI_FORMAT_B8G8R8A8_UNORM;
        desc.SampleDesc.Count = 1;
        desc.Usage = D3D11_USAGE_DEFAULT;
        desc.BindFlags = 0;
        desc.CPUAccessFlags = 0;
        HRESULT result = m_d3d11Device->CreateTexture2D(&desc, nullptr, &texture);
        if (result != S_OK)
        {
            RTC_LOG(LS_INFO) << "CreateTexture2D failed. error:" << result;
            return nullptr;
        }
        return new D3D11Texture2D(w, h, texture);
    }

    //---------------------------------------------------------------------------------------------------------------------
    ITexture2D*
    D3D11GraphicsDevice::CreateCPUReadTextureV(uint32_t w, uint32_t h, UnityRenderingExtTextureFormat textureFormat)
    {

        ID3D11Texture2D* texture = nullptr;
        D3D11_TEXTURE2D_DESC desc = {};
        desc.Width = w;
        desc.Height = h;
        desc.MipLevels = 1;
        desc.ArraySize = 1;
        desc.Format = DXGI_FORMAT_B8G8R8A8_UNORM;
        desc.SampleDesc.Count = 1;
        desc.Usage = D3D11_USAGE_STAGING;
        desc.BindFlags = 0;
        desc.CPUAccessFlags = D3D11_CPU_ACCESS_READ;
        HRESULT hr = m_d3d11Device->CreateTexture2D(&desc, nullptr, &texture);
        if (hr != S_OK)
        {
            return nullptr;
        }
        return new D3D11Texture2D(w, h, texture);
    }

    //---------------------------------------------------------------------------------------------------------------------
    bool D3D11GraphicsDevice::CopyResourceV(ITexture2D* dest, ITexture2D* src)
    {
        ID3D11Resource* nativeDest = reinterpret_cast<ID3D11Resource*>(dest->GetNativeTexturePtrV());
        ID3D11Resource* nativeSrc = reinterpret_cast<ID3D11Texture2D*>(src->GetNativeTexturePtrV());
        if (nativeSrc == nativeDest)
            return false;
        if (nativeSrc == nullptr || nativeDest == nullptr)
            return false;

        ComPtr<ID3D11DeviceContext> context;
        m_d3d11Device->GetImmediateContext(context.GetAddressOf());
        context->CopyResource(nativeDest, nativeSrc);

        // todo(kazuki): Flush incurs a significant amount of overhead.
        // Should run the process of copying texture asyncnously.
        HRESULT hr = WaitFlush();
        if (hr != S_OK)
        {
            RTC_LOG(LS_INFO) << "WaitFlush failed. error:" << hr;
            return false;
        }
        return true;
    }

    //---------------------------------------------------------------------------------------------------------------------
    bool D3D11GraphicsDevice::CopyResourceFromNativeV(ITexture2D* dest, void* nativeTexturePtr)
    {
        ID3D11Resource* nativeDest = reinterpret_cast<ID3D11Resource*>(dest->GetNativeTexturePtrV());
        ID3D11Resource* nativeSrc = reinterpret_cast<ID3D11Resource*>(nativeTexturePtr);
        if (nativeSrc == nativeDest)
            return false;
        if (nativeSrc == nullptr || nativeDest == nullptr)
            return false;

        ComPtr<ID3D11DeviceContext> context;
        m_d3d11Device->GetImmediateContext(context.GetAddressOf());
        context->CopyResource(nativeDest, nativeSrc);

        // todo(kazuki): Flush incurs a significant amount of overhead.
        // Should run the process of copying texture asyncnously.
        HRESULT hr = WaitFlush();
        if (hr != S_OK)
        {
            RTC_LOG(LS_INFO) << "WaitFlush failed. error:" << hr;
            return false;
        }
        return true;
    }

    HRESULT D3D11GraphicsDevice::WaitFlush()
    {
        ComPtr<ID3D11DeviceContext> context;
        m_d3d11Device->GetImmediateContext(context.GetAddressOf());
        context->Flush();

        D3D11_QUERY_DESC queryDesc = { D3D11_QUERY_EVENT, 0 };
        ComPtr<ID3D11Query> query;
        HRESULT hr = m_d3d11Device->CreateQuery(&queryDesc, query.GetAddressOf());
        if (hr != S_OK)
        {
            return hr;
        }
        context->End(query.Get());
        while (S_OK != context->GetData(query.Get(), nullptr, 0, 0))
            ;
        return S_OK;
    }

    //---------------------------------------------------------------------------------------------------------------------

    rtc::scoped_refptr<I420Buffer> D3D11GraphicsDevice::ConvertRGBToI420(ITexture2D* tex)
    {
        D3D11_MAPPED_SUBRESOURCE pMappedResource;

        ID3D11Resource* pResource = reinterpret_cast<ID3D11Resource*>(tex->GetNativeTexturePtrV());
        if (nullptr == pResource)
            return nullptr;

        ComPtr<ID3D11DeviceContext> context;
        m_d3d11Device->GetImmediateContext(context.GetAddressOf());

        const HRESULT hr = context->Map(pResource, 0, D3D11_MAP_READ, 0, &pMappedResource);
        if (hr != S_OK)
            return nullptr;

        const int32_t width = static_cast<int32_t>(tex->GetWidth());
        const int32_t height = static_cast<int32_t>(tex->GetHeight());

        rtc::scoped_refptr<webrtc::I420Buffer> i420_buffer = webrtc::I420Buffer::Create(width, height);
        libyuv::ARGBToI420(
            static_cast<uint8_t*>(pMappedResource.pData),
            static_cast<int32_t>(pMappedResource.RowPitch),
            i420_buffer->MutableDataY(),
            i420_buffer->StrideY(),
            i420_buffer->MutableDataU(),
            i420_buffer->StrideU(),
            i420_buffer->MutableDataV(),
            i420_buffer->StrideV(),
            width,
            height);

        context->Unmap(pResource, 0);
        return i420_buffer;
    }

    std::unique_ptr<GpuMemoryBufferHandle> D3D11GraphicsDevice::Map(ITexture2D* texture)
    {
        if (!IsCudaSupport())
            return nullptr;

        CUgraphicsResource resource;
        ID3D11Resource* pResource = static_cast<ID3D11Resource*>(texture->GetNativeTexturePtrV());

        // set context on the thread.
        cuCtxPushCurrent(GetCUcontext());

        CUresult result =
            cuGraphicsD3D11RegisterResource(&resource, pResource, CU_GRAPHICS_REGISTER_FLAGS_SURFACE_LDST);
        if (result != CUDA_SUCCESS)
        {
            RTC_LOG(LS_ERROR) << "cuGraphicsD3D11RegisterResource";
            throw;
        }
        result = cuGraphicsMapResources(1, &resource, nullptr);
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
        cuCtxPopCurrent(nullptr);

        std::unique_ptr<GpuMemoryBufferCudaHandle> handle = std::make_unique<GpuMemoryBufferCudaHandle>();
        handle->context = GetCUcontext();
        handle->mappedArray = mappedArray;
        handle->resource = resource;
        return std::move(handle);
    }

} // end namespace webrtc
} // end namespace unity
