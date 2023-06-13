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
        , m_unityInterface(unityInterface)
        , m_d3d12Device(nativeDevice)
        , m_d3d12CommandQueue(unityInterface->GetCommandQueue())
    {
    }
    //---------------------------------------------------------------------------------------------------------------------
    D3D12GraphicsDevice::D3D12GraphicsDevice(
        ID3D12Device* nativeDevice,
        ID3D12CommandQueue* commandQueue,
        UnityGfxRenderer renderer,
        ProfilerMarkerFactory* profiler)
        : IGraphicsDevice(renderer, profiler)
        , m_unityInterface(nullptr)
        , m_d3d12Device(nativeDevice)
        , m_d3d12CommandQueue(commandQueue)
    {
    }

    //---------------------------------------------------------------------------------------------------------------------
    D3D12GraphicsDevice::~D3D12GraphicsDevice() { }

    //---------------------------------------------------------------------------------------------------------------------

    bool D3D12GraphicsDevice::InitV()
    {
        HRESULT hr =
            m_d3d12Device->CreateCommandAllocator(D3D12_COMMAND_LIST_TYPE_DIRECT, IID_PPV_ARGS(&m_commandAllocator));
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
        m_isCudaSupport = CUDA_SUCCESS == m_cudaContext.Init(m_d3d12Device.Get());
        return true;
    }

    //---------------------------------------------------------------------------------------------------------------------
    void D3D12GraphicsDevice::ShutdownV() { m_cudaContext.Shutdown(); }

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
        if (!dest)
            return false;

        ID3D12Resource* nativeDest = reinterpret_cast<ID3D12Resource*>(dest->GetNativeTexturePtrV());
        ID3D12Resource* nativeSrc = reinterpret_cast<ID3D12Resource*>(nativeTexturePtr);
        ID3D12Resource* readbackResource = dest->GetReadbackResource();

        if (nativeSrc == nativeDest)
            return false;
        if (!nativeSrc)
            return false;

        ThrowIfFailed(m_commandAllocator->Reset());
        ThrowIfFailed(m_commandList->Reset(m_commandAllocator, nullptr));

        std::vector<UnityGraphicsD3D12ResourceState> states;

        // for GPU accessible texture
        if (nativeDest)
        {
            m_commandList->CopyResource(nativeDest, nativeSrc);
            states.push_back(UnityGraphicsD3D12ResourceState {
                nativeSrc, D3D12_RESOURCE_STATE_COPY_SOURCE, D3D12_RESOURCE_STATE_COPY_SOURCE });
            states.push_back(UnityGraphicsD3D12ResourceState {
                nativeDest, D3D12_RESOURCE_STATE_COPY_DEST, D3D12_RESOURCE_STATE_COPY_DEST });
        }

        // for CPU accessible texture
        if (readbackResource)
        {
            const D3D12ResourceFootprint* resFP = dest->GetNativeTextureFootprint();

            // Change dest state, copy, change dest state back
            D3D12_TEXTURE_COPY_LOCATION srcLoc = {};
            srcLoc.pResource = nativeSrc;
            srcLoc.Type = D3D12_TEXTURE_COPY_TYPE_SUBRESOURCE_INDEX;
            srcLoc.SubresourceIndex = 0;

            D3D12_TEXTURE_COPY_LOCATION dstLoc = {};
            dstLoc.pResource = readbackResource;
            dstLoc.Type = D3D12_TEXTURE_COPY_TYPE_PLACED_FOOTPRINT;
            dstLoc.PlacedFootprint = resFP->Footprint;

            m_commandList->CopyTextureRegion(&dstLoc, 0, 0, 0, &srcLoc, nullptr);

            states.push_back(UnityGraphicsD3D12ResourceState {
                nativeSrc, D3D12_RESOURCE_STATE_COPY_SOURCE, D3D12_RESOURCE_STATE_COPY_SOURCE });
        }

        ThrowIfFailed(m_commandList->Close());

        ID3D12GraphicsCommandList* cmdList = { m_commandList };
        uint64_t value = m_unityInterface->ExecuteCommandList(cmdList, static_cast<int>(states.size()), states.data());
        dest->SetSyncCount(value);

        return true;
    }

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
        return new D3D12Texture2D(w, h, resource, handle);
    }

    bool D3D12GraphicsDevice::WaitSync(const ITexture2D* texture, uint64_t nsTimeout)
    {
        const D3D12Texture2D* d3d12Texture = static_cast<const D3D12Texture2D*>(texture);
        ID3D12Fence* fence = m_unityInterface->GetFrameFence();
        uint64_t value = d3d12Texture->GetSyncCount();

        if (fence->GetCompletedValue() < value)
        {
            HANDLE fenceEvent = CreateEvent(nullptr, FALSE, FALSE, nullptr);
            HRESULT hr = fence->SetEventOnCompletion(value, fenceEvent);
            if (hr != S_OK)
            {
                RTC_LOG(LS_INFO) << "ID3D11Fence::SetEventOnCompletion failed. error:" << hr;
                return false;
            }
            auto nanoseconds = std::chrono::nanoseconds(nsTimeout);
            auto milliseconds = std::chrono::duration_cast<std::chrono::milliseconds>(nanoseconds).count();

            if (WaitForSingleObject(fenceEvent, static_cast<DWORD>(milliseconds)) == WAIT_FAILED)
            {
                RTC_LOG(LS_INFO) << "WaitForSingleObject failed. error:" << GetLastError();
                return false;
            }
            CloseHandle(fenceEvent);
        }
        return true;
    }

    bool D3D12GraphicsDevice::ResetSync(const ITexture2D* texture) { return true; }

    //----------------------------------------------------------------------------------------------------------------------

    ITexture2D*
    D3D12GraphicsDevice::CreateCPUReadTextureV(uint32_t w, uint32_t h, UnityRenderingExtTextureFormat textureFormat)
    {
        return D3D12Texture2D::CreateReadbackResource(m_d3d12Device.Get(), w, h);
    }

    //----------------------------------------------------------------------------------------------------------------------
    rtc::scoped_refptr<webrtc::I420Buffer> D3D12GraphicsDevice::ConvertRGBToI420(ITexture2D* baseTex)
    {
        D3D12Texture2D* tex = reinterpret_cast<D3D12Texture2D*>(baseTex);
        if (!tex)
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

        GMB_CUDA_CALL_NULLPTR(cuCtxPushCurrent(GetCUcontext()));

        std::unique_ptr<GpuMemoryBufferCudaHandle> handle = std::make_unique<GpuMemoryBufferCudaHandle>();
        handle->context = GetCUcontext();

        D3D12Texture2D* d3d12Tex = static_cast<D3D12Texture2D*>(texture);

        HANDLE sharedHandle = d3d12Tex->GetHandle();
        if (!sharedHandle)
        {
            RTC_LOG(LS_ERROR) << "cannot get shared handle";
            return nullptr;
        }

        size_t width = d3d12Tex->GetWidth();
        size_t height = d3d12Tex->GetHeight();
        D3D12_RESOURCE_DESC desc = d3d12Tex->GetDesc();
        D3D12_RESOURCE_ALLOCATION_INFO d3d12ResourceAllocationInfo;
        d3d12ResourceAllocationInfo = m_d3d12Device->GetResourceAllocationInfo(0, 1, &desc);
        size_t actualSize = d3d12ResourceAllocationInfo.SizeInBytes;

        CUDA_EXTERNAL_MEMORY_HANDLE_DESC memDesc = {};
        memDesc.type = CU_EXTERNAL_MEMORY_HANDLE_TYPE_D3D12_RESOURCE;
        memDesc.handle.win32.handle = static_cast<void*>(sharedHandle);
        memDesc.size = actualSize;
        memDesc.flags = CUDA_EXTERNAL_MEMORY_DEDICATED;

        GMB_CUDA_CALL_NULLPTR(cuImportExternalMemory(&handle->externalMemory, &memDesc));

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

        GMB_CUDA_CALL_NULLPTR(
            cuExternalMemoryGetMappedMipmappedArray(&handle->mipmappedArray, handle->externalMemory, &mipmapArrayDesc));
        GMB_CUDA_CALL_NULLPTR(cuMipmappedArrayGetLevel(&handle->mappedArray, handle->mipmappedArray, 0));
        GMB_CUDA_CALL_NULLPTR(cuCtxPopCurrent(nullptr));

        return std::move(handle);
    }

} // end namespace webrtc
} // end namespace unity
