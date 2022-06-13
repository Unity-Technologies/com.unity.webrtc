#include "pch.h"

#include <third_party/libyuv/include/libyuv.h>

#include "D3D12Constants.h"
#include "D3D12GraphicsDevice.h"
#include "D3D12Texture2D.h"
#include "GraphicsDevice/Cuda/GpuMemoryBufferCudaHandle.h"
#include "GraphicsDevice/D3D11/D3D11Texture2D.h"
#include "GraphicsDevice/GraphicsUtility.h"
#include "NvCodecUtils.h"

// nonstandard extension used : class rvalue used as lvalue
#pragma clang diagnostic ignored "-Wlanguage-extension-token"

using namespace Microsoft::WRL;

namespace unity
{
namespace webrtc
{

    //---------------------------------------------------------------------------------------------------------------------

    D3D12GraphicsDevice::D3D12GraphicsDevice(
        ID3D12Device* nativeDevice,
        IUnityGraphicsD3D12v5* unityInterface,
        UnityGfxRenderer renderer,
        ProfilerMarkerFactory* profiler)
        : IGraphicsDevice(renderer, profiler)
        , m_d3d12Device(nativeDevice)
        , m_d3d12CommandQueue(unityInterface->GetCommandQueue())
        , m_d3d11Device(nullptr)
        , m_d3d11Context(nullptr)
        , m_copyResourceFence(nullptr)
        , m_copyResourceEventHandle(nullptr)
    {
    }
    //---------------------------------------------------------------------------------------------------------------------
    D3D12GraphicsDevice::D3D12GraphicsDevice(
        ID3D12Device* nativeDevice,
        ID3D12CommandQueue* commandQueue,
        UnityGfxRenderer renderer,
        ProfilerMarkerFactory* profiler)
        : IGraphicsDevice(renderer, profiler)
        , m_d3d12Device(nativeDevice)
        , m_d3d12CommandQueue(commandQueue)
        , m_d3d11Device(nullptr)
        , m_d3d11Context(nullptr)
        , m_copyResourceFence(nullptr)
        , m_copyResourceEventHandle(nullptr)
    {
    }

    //---------------------------------------------------------------------------------------------------------------------
    D3D12GraphicsDevice::~D3D12GraphicsDevice() { }

    //---------------------------------------------------------------------------------------------------------------------

    bool D3D12GraphicsDevice::InitV()
    {

        ID3D11Device* legacyDevice;
        ID3D11DeviceContext* legacyContext;

        HRESULT hr = D3D11CreateDevice(
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

        if (FAILED(hr))
        {
            RTC_LOG(LS_ERROR) << "D3D11CreateDevice is failed. " << HrToString(hr);
            return false;
        }

        hr = legacyDevice->QueryInterface(IID_PPV_ARGS(&m_d3d11Device));
        if (FAILED(hr))
        {
            RTC_LOG(LS_ERROR) << "ID3D11DeviceContext::QueryInterface is failed. " << HrToString(hr);
            return false;
        }

        legacyDevice->GetImmediateContext(&legacyContext);
        hr = legacyContext->QueryInterface(IID_PPV_ARGS(&m_d3d11Context));
        if (FAILED(hr))
        {
            RTC_LOG(LS_ERROR) << "ID3D11DeviceContext::QueryInterface is failed. " << HrToString(hr);
            return false;
        }

        hr = m_d3d12Device->CreateCommandAllocator(D3D12_COMMAND_LIST_TYPE_DIRECT, IID_PPV_ARGS(&m_commandAllocator));
        if (FAILED(hr))
        {
            RTC_LOG(LS_ERROR) << "ID3D12Device::CreateCommandAllocator is failed. " << HrToString(hr);
            return false;
        }

        hr = m_d3d12Device->CreateCommandList(
            0, D3D12_COMMAND_LIST_TYPE_DIRECT, m_commandAllocator, nullptr, IID_PPV_ARGS(&m_commandList));
        if (FAILED(hr))
        {
            RTC_LOG(LS_ERROR) << "ID3D12Device::CreateCommandList is failed. " << HrToString(hr);
            return false;
        }

        // Command lists are created in the recording state, but there is nothing
        // to record yet. The main loop expects it to be closed, so close it now.
        hr = m_commandList->Close();
        if (FAILED(hr))
        {
            RTC_LOG(LS_ERROR) << "ID3D12GraphicsCommandList::Close is failed. " << HrToString(hr);
            return false;
        }

        hr = m_d3d12Device->CreateFence(0, D3D12_FENCE_FLAG_NONE, IID_PPV_ARGS(&m_copyResourceFence));
        if (FAILED(hr))
        {
            RTC_LOG(LS_ERROR) << "ID3D12Device::CreateFence is failed. " << HrToString(hr);
            return false;
        }

        m_copyResourceEventHandle = CreateEvent(nullptr, FALSE, FALSE, nullptr);
        if (m_copyResourceEventHandle == nullptr)
        {
            hr = HRESULT_FROM_WIN32(GetLastError());
            RTC_LOG(LS_ERROR) << "CreateEvent is failed. " << HrToString(hr);
            return false;
        }
        m_isCudaSupport = CUDA_SUCCESS == m_cudaContext.Init(m_d3d12Device.Get());
        return true;
    }

    //---------------------------------------------------------------------------------------------------------------------
    void D3D12GraphicsDevice::ShutdownV()
    {
        m_cudaContext.Shutdown();
        SAFE_CLOSE_HANDLE(m_copyResourceEventHandle)
    }

    //---------------------------------------------------------------------------------------------------------------------
    ITexture2D*
    D3D12GraphicsDevice::CreateDefaultTextureV(uint32_t w, uint32_t h, UnityRenderingExtTextureFormat textureFormat)
    {

        return CreateSharedD3D12Texture(w, h);
    }

    //---------------------------------------------------------------------------------------------------------------------

    bool D3D12GraphicsDevice::CopyResourceV(ITexture2D* dest, ITexture2D* src)
    {
        //[Note-sin: 2020-2-19] This function is currently not required by RenderStreaming. Delete?
        return true;
    }

    //---------------------------------------------------------------------------------------------------------------------
    bool D3D12GraphicsDevice::CopyResourceFromNativeV(ITexture2D* baseDest, void* nativeTexturePtr)
    {

        D3D12Texture2D* dest = reinterpret_cast<D3D12Texture2D*>(baseDest);
        assert(nullptr != dest);
        if (nullptr == dest)
            return false;

        ID3D12Resource* nativeDest = reinterpret_cast<ID3D12Resource*>(dest->GetNativeTexturePtrV());
        ID3D12Resource* nativeSrc = reinterpret_cast<ID3D12Resource*>(nativeTexturePtr);
        if (nativeSrc == nativeDest)
            return false;
        if (nativeSrc == nullptr || nativeDest == nullptr)
            return false;

        ThrowIfFailed(m_commandAllocator->Reset());
        ThrowIfFailed(m_commandList->Reset(m_commandAllocator, nullptr));

        m_commandList->CopyResource(nativeDest, nativeSrc);

        // for CPU accessible texture
        ID3D12Resource* readbackResource = dest->GetReadbackResource();
        const D3D12ResourceFootprint* resFP = dest->GetNativeTextureFootprint();
        if (nullptr != readbackResource)
        {
            // Change dest state, copy, change dest state back
            Barrier(nativeDest, D3D12_RESOURCE_STATE_COPY_DEST, D3D12_RESOURCE_STATE_COPY_SOURCE);
            D3D12_TEXTURE_COPY_LOCATION td, ts;
            td.pResource = readbackResource;
            td.Type = D3D12_TEXTURE_COPY_TYPE_PLACED_FOOTPRINT;
            td.PlacedFootprint = resFP->Footprint;
            ts.pResource = nativeDest;
            ts.Type = D3D12_TEXTURE_COPY_TYPE_SUBRESOURCE_INDEX;
            ts.SubresourceIndex = 0;
            m_commandList->CopyTextureRegion(&td, 0, 0, 0, &ts, nullptr);
            Barrier(nativeDest, D3D12_RESOURCE_STATE_COPY_SOURCE, D3D12_RESOURCE_STATE_COPY_DEST);
        }

        ThrowIfFailed(m_commandList->Close());

        ID3D12CommandList* cmdList[] = { m_commandList };
        m_d3d12CommandQueue->ExecuteCommandLists(_countof(cmdList), cmdList);

        WaitForFence(m_copyResourceFence.Get(), m_copyResourceEventHandle, &m_copyResourceFenceValue);

        return true;
    }

    //---------------------------------------------------------------------------------------------------------------------

    D3D12Texture2D* D3D12GraphicsDevice::CreateSharedD3D12Texture(uint32_t w, uint32_t h)
    {
        //[Note-sin: 2019-10-30] Taken from RaytracedHardShadow
        // note: sharing textures with d3d11 requires some flags and restrictions:
        // - MipLevels must be 1
        // - D3D12_HEAP_FLAG_SHARED for heap flags
        // - D3D12_RESOURCE_FLAG_ALLOW_RENDER_TARGET and D3D12_RESOURCE_FLAG_ALLOW_SIMULTANEOUS_ACCESS for resource
        // flags

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

        const D3D12_HEAP_FLAGS flags = D3D12_HEAP_FLAG_SHARED;
        const D3D12_RESOURCE_STATES initialState = D3D12_RESOURCE_STATE_COPY_DEST;

        ID3D12Resource* resource = nullptr;
        HRESULT result = m_d3d12Device->CreateCommittedResource(
            &D3D12_DEFAULT_HEAP_PROPS, flags, &desc, initialState, nullptr, IID_PPV_ARGS(&resource));

        if (result != S_OK)
        {
            RTC_LOG(LS_INFO) << "CreateCommittedResource failed. error:" << result;
            return nullptr;
        }

        HANDLE handle = nullptr;
        result = m_d3d12Device->CreateSharedHandle(resource, nullptr, GENERIC_ALL, nullptr, &handle);
        if (result != S_OK)
        {
            RTC_LOG(LS_INFO) << "CreateSharedHandle failed. error:" << result;
            return nullptr;
        }

        // ID3D11Device::OpenSharedHandle() doesn't accept handles created by d3d12.
        // OpenSharedResource1() is needed.
        ID3D11Texture2D* sharedTex = nullptr;
        result = m_d3d11Device->OpenSharedResource1(handle, IID_PPV_ARGS(&sharedTex));
        if (result != S_OK)
        {
            RTC_LOG(LS_INFO) << "OpenSharedResource1 failed. error:" << result;
            return nullptr;
        }

        return new D3D12Texture2D(w, h, resource, handle, sharedTex);
    }

    //----------------------------------------------------------------------------------------------------------------------
    void D3D12GraphicsDevice::WaitForFence(ID3D12Fence* fence, HANDLE handle, uint64_t* fenceValue)
    {
        ThrowIfFailed(m_d3d12CommandQueue->Signal(fence, *fenceValue));
        ThrowIfFailed(fence->SetEventOnCompletion(*fenceValue, handle));
        WaitForSingleObject(handle, INFINITE);
        ++(*fenceValue);
    }

    //----------------------------------------------------------------------------------------------------------------------

    void D3D12GraphicsDevice::Barrier(
        ID3D12Resource* res,
        const D3D12_RESOURCE_STATES stateBefore,
        const D3D12_RESOURCE_STATES stateAfter,
        const UINT subresource)
    {
        D3D12_RESOURCE_BARRIER barrier;
        barrier.Type = D3D12_RESOURCE_BARRIER_TYPE_TRANSITION;
        barrier.Flags = D3D12_RESOURCE_BARRIER_FLAG_NONE;
        barrier.Transition.pResource = res;
        barrier.Transition.StateBefore = stateBefore;
        barrier.Transition.StateAfter = stateAfter;
        barrier.Transition.Subresource = subresource;
        m_commandList->ResourceBarrier(1, &barrier);
    }

    //----------------------------------------------------------------------------------------------------------------------

    ITexture2D*
    D3D12GraphicsDevice::CreateCPUReadTextureV(uint32_t w, uint32_t h, UnityRenderingExtTextureFormat textureFormat)
    {
        D3D12Texture2D* tex = CreateSharedD3D12Texture(w, h);
        const HRESULT hr = tex->CreateReadbackResource(m_d3d12Device.Get());
        if (FAILED(hr))
        {
            delete tex;
            return nullptr;
        }

        return tex;
    }

    //----------------------------------------------------------------------------------------------------------------------
    rtc::scoped_refptr<webrtc::I420Buffer> D3D12GraphicsDevice::ConvertRGBToI420(ITexture2D* baseTex)
    {
        D3D12Texture2D* tex = reinterpret_cast<D3D12Texture2D*>(baseTex);
        assert(nullptr != tex);
        if (nullptr == tex)
            return nullptr;

        ID3D12Resource* readbackResource = tex->GetReadbackResource();
        assert(nullptr != readbackResource);
        if (nullptr == readbackResource) // the texture has to be prepared for CPU access
            return nullptr;

        const int width = static_cast<int>(tex->GetWidth());
        const int height = static_cast<int>(tex->GetHeight());
        const D3D12ResourceFootprint* footprint = tex->GetNativeTextureFootprint();
        const int rowPitch = static_cast<int>(footprint->Footprint.Footprint.RowPitch);

        // Map to read from CPU
        uint8* data {};
        const HRESULT hr = readbackResource->Map(0, nullptr, reinterpret_cast<void**>(&data));
        assert(hr == S_OK);
        if (hr != S_OK)
        {
            return nullptr;
        }

        // RGBA -> I420
        rtc::scoped_refptr<webrtc::I420Buffer> i420_buffer = webrtc::I420Buffer::Create(width, height);
        libyuv::ARGBToI420(
            static_cast<uint8_t*>(data),
            rowPitch,
            i420_buffer->MutableDataY(),
            i420_buffer->StrideY(),
            i420_buffer->MutableDataU(),
            i420_buffer->StrideU(),
            i420_buffer->MutableDataV(),
            i420_buffer->StrideV(),
            width,
            height);

        D3D12_RANGE emptyRange { 0, 0 };
        readbackResource->Unmap(0, &emptyRange);

        return i420_buffer;
    }

    std::unique_ptr<GpuMemoryBufferHandle> D3D12GraphicsDevice::Map(ITexture2D* texture)
    {
        if (!IsCudaSupport())
            return nullptr;

        D3D12Texture2D* d3d12Texure = static_cast<D3D12Texture2D*>(texture);

        // set context on the thread.
        cuCtxPushCurrent(GetCUcontext());

        HANDLE sharedHandle = d3d12Texure->GetHandle();
        if (!sharedHandle)
        {
            RTC_LOG(LS_ERROR) << "cannot get shared handle";
            throw;
        }

        size_t width = d3d12Texure->GetWidth();
        size_t height = d3d12Texure->GetHeight();
        D3D12_RESOURCE_DESC desc = d3d12Texure->GetDesc();
        D3D12_RESOURCE_ALLOCATION_INFO d3d12ResourceAllocationInfo;
        d3d12ResourceAllocationInfo = m_d3d12Device->GetResourceAllocationInfo(0, 1, &desc);
        size_t actualSize = d3d12ResourceAllocationInfo.SizeInBytes;

        CUDA_EXTERNAL_MEMORY_HANDLE_DESC memDesc = {};
        memDesc.type = CU_EXTERNAL_MEMORY_HANDLE_TYPE_D3D12_RESOURCE;
        memDesc.handle.win32.handle = static_cast<void*>(sharedHandle);
        memDesc.size = actualSize;
        memDesc.flags = CUDA_EXTERNAL_MEMORY_DEDICATED;

        CUresult result;
        CUexternalMemory externalMemory = {};
        result = cuImportExternalMemory(&externalMemory, &memDesc);
        if (result != CUDA_SUCCESS)
        {
            RTC_LOG(LS_ERROR) << "cuImportExternalMemory error";
            throw;
        }

        CUDA_ARRAY3D_DESCRIPTOR arrayDesc = {};
        arrayDesc.Width = width;
        arrayDesc.Height = height;
        arrayDesc.Depth = 0; /* CUDA 2D arrays are defined to have depth 0 */
        arrayDesc.Format = CU_AD_FORMAT_UNSIGNED_INT32;
        arrayDesc.NumChannels = 1;
        arrayDesc.Flags = CUDA_ARRAY3D_SURFACE_LDST | CUDA_ARRAY3D_COLOR_ATTACHMENT;

        CUDA_EXTERNAL_MEMORY_MIPMAPPED_ARRAY_DESC mipmapArrayDesc = {};
        mipmapArrayDesc.arrayDesc = arrayDesc;
        mipmapArrayDesc.numLevels = 1;

        CUmipmappedArray mipmappedArray;
        result = cuExternalMemoryGetMappedMipmappedArray(&mipmappedArray, externalMemory, &mipmapArrayDesc);
        if (result != CUDA_SUCCESS)
        {
            RTC_LOG(LS_ERROR) << "cuExternalMemoryGetMappedMipmappedArray error";
            throw;
        }

        CUarray array;
        result = cuMipmappedArrayGetLevel(&array, mipmappedArray, 0);
        if (result != CUDA_SUCCESS)
        {
            RTC_LOG(LS_ERROR) << "cuMipmappedArrayGetLevel error";
            throw;
        }
        cuCtxPopCurrent(nullptr);

        std::unique_ptr<GpuMemoryBufferCudaHandle> handle = std::make_unique<GpuMemoryBufferCudaHandle>();
        handle->context = GetCUcontext();
        handle->mappedArray = array;
        handle->externalMemory = externalMemory;
        return std::move(handle);
    }

} // end namespace webrtc
} // end namespace unity
