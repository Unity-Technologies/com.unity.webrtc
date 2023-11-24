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
        HRESULT hr = S_OK;

        if (m_unityInterface)
        {
            m_fence = m_unityInterface->GetFrameFence();
        }
        else
        {
            // If a unityInterface is not passed, use m_fence to synchronize between GPU and CPU.
            hr = m_d3d12Device->CreateFence(0, D3D12_FENCE_FLAG_NONE, IID_PPV_ARGS(&m_fence));
            if (FAILED(hr))
            {
                RTC_LOG(LS_ERROR) << "ID3D12Device::CreateFence is failed. " << HrToString(hr);
                return false;
            }
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

    bool D3D12GraphicsDevice::CopyResourceFromNativeV(ITexture2D* baseDest, void* nativeTexturePtr)
    {
        D3D12Texture2D* dest = reinterpret_cast<D3D12Texture2D*>(baseDest);
        if (!dest)
            return false;

        ID3D12Resource* destResource = reinterpret_cast<ID3D12Resource*>(dest->GetNativeTexturePtrV());
        ID3D12Resource* srcResource = reinterpret_cast<ID3D12Resource*>(nativeTexturePtr);
        bool isReadbackResource = dest->IsReadbackResource();

        if (srcResource == destResource)
            return false;
        if (!srcResource || !destResource)
            return false;

        // Find elements with the finished commands and reset the CommandAllocator.
        uint64_t completedValue = m_fence->GetCompletedValue();
        for (auto& frame : m_frames)
        {
            if (frame.fenceValue > 0 && frame.fenceValue <= completedValue)
            {
                frame.commandAllocator->Reset();
                frame.fenceValue = 0;
            }
        }

        // Find an element with the same fenceValue. If it does not exist, find an unused element.
        uint64_t nextValue = GetNextFrameFenceValue();
        auto frame = std::find_if(
            m_frames.begin(), m_frames.end(), [nextValue](Frame frame) { return frame.fenceValue == nextValue; });
        if (frame == m_frames.end())
        {
            // Find an unused element.
            frame = std::find_if(m_frames.begin(), m_frames.end(), [](Frame frame) { return frame.fenceValue == 0; });
            if (frame == m_frames.end())
            {
                // Create a new element.
                Frame newFrame;
                if (!CreateFrame(newFrame))
                {
                    RTC_LOG(LS_INFO) << "Failed to create a new frame.";
                    return false;
                }
                m_frames.push_back(newFrame);
                frame = m_frames.end();
                frame--;
            }
            frame->fenceValue = nextValue;
        }
        auto commandList = frame->commandList;

        // Reset m_commandAllocator when the process is arriving here first time in the frame.
        ThrowIfFailed(commandList->Reset(frame->commandAllocator, nullptr));

        std::vector<UnityGraphicsD3D12ResourceState> states;

        // for GPU accessible texture
        if (!isReadbackResource)
        {
            commandList->CopyResource(destResource, srcResource);
            states.push_back(UnityGraphicsD3D12ResourceState {
                srcResource, D3D12_RESOURCE_STATE_COPY_SOURCE, D3D12_RESOURCE_STATE_COPY_SOURCE });
            states.push_back(UnityGraphicsD3D12ResourceState {
                destResource, D3D12_RESOURCE_STATE_COPY_DEST, D3D12_RESOURCE_STATE_COPY_DEST });
        }
        else
        {
            const D3D12ResourceFootprint* resFP = dest->GetNativeTextureFootprint();

            // Change dest state, copy, change dest state back
            D3D12_TEXTURE_COPY_LOCATION srcLoc = {};
            srcLoc.pResource = srcResource;
            srcLoc.Type = D3D12_TEXTURE_COPY_TYPE_SUBRESOURCE_INDEX;
            srcLoc.SubresourceIndex = 0;

            D3D12_TEXTURE_COPY_LOCATION dstLoc = {};
            dstLoc.pResource = destResource;
            dstLoc.Type = D3D12_TEXTURE_COPY_TYPE_PLACED_FOOTPRINT;
            dstLoc.PlacedFootprint = resFP->Footprint;

            commandList->CopyTextureRegion(&dstLoc, 0, 0, 0, &srcLoc, nullptr);

            states.push_back(UnityGraphicsD3D12ResourceState {
                srcResource, D3D12_RESOURCE_STATE_COPY_SOURCE, D3D12_RESOURCE_STATE_COPY_SOURCE });
        }

        ThrowIfFailed(commandList->Close());

        const int commandListsCount = 1;
        ID3D12GraphicsCommandList* cmdList = { commandList };
        uint64_t value = ExecuteCommandList(commandListsCount, cmdList, static_cast<int>(states.size()), states.data());
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

    bool D3D12GraphicsDevice::WaitSync(const ITexture2D* texture)
    {
        const D3D12Texture2D* d3d12Texture = static_cast<const D3D12Texture2D*>(texture);
        const uint64_t value = d3d12Texture->GetSyncCount();
        ID3D12Fence* fence = GetFence();

        if (fence->GetCompletedValue() < value)
        {
            HANDLE fenceEvent = CreateEvent(nullptr, FALSE, FALSE, nullptr);
            if (!fenceEvent)
                return false;
            HRESULT hr = fence->SetEventOnCompletion(value, fenceEvent);
            if (hr != S_OK)
            {
                RTC_LOG(LS_INFO) << "ID3D12Fence::SetEventOnCompletion failed. error:" << hr;
                return false;
            }
            auto milliseconds = std::chrono::duration_cast<std::chrono::milliseconds>(m_syncTimeout).count();
            DWORD ret = WaitForSingleObject(fenceEvent, static_cast<DWORD>(milliseconds));
            CloseHandle(fenceEvent);

            if (ret != WAIT_OBJECT_0)
            {
                RTC_LOG(LS_INFO) << "WaitForSingleObject failed. error:" << ret;
                return false;
            }
        }
        return true;
    }

    bool D3D12GraphicsDevice::ResetSync(const ITexture2D* texture) { return true; }

    bool D3D12GraphicsDevice::WaitIdleForTest()
    {
        HANDLE handle = CreateEvent(nullptr, FALSE, FALSE, nullptr);
        if (!handle)
            return false;
        uint64_t nextFrameFenceValue = GetNextFrameFenceValue();
        ThrowIfFailed(m_d3d12CommandQueue->Signal(m_fence.Get(), nextFrameFenceValue));
        ThrowIfFailed(m_fence->SetEventOnCompletion(nextFrameFenceValue, handle));
        DWORD ret = WaitForSingleObject(handle, INFINITE);
        CloseHandle(handle);
        if (ret != WAIT_OBJECT_0)
        {
            RTC_LOG(LS_INFO) << "WaitForSingleObject failed. error:" << ret;
            return false;
        }
        return true;
    }

    ITexture2D*
    D3D12GraphicsDevice::CreateCPUReadTextureV(uint32_t w, uint32_t h, UnityRenderingExtTextureFormat textureFormat)
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
        m_d3d12Device->GetCopyableFootprints(
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
        const HRESULT hr = m_d3d12Device->CreateCommittedResource(
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
        return new D3D12Texture2D(w, h, resource, footprint);
    }

    //----------------------------------------------------------------------------------------------------------------------
    rtc::scoped_refptr<webrtc::I420Buffer> D3D12GraphicsDevice::ConvertRGBToI420(ITexture2D* texture)
    {
        D3D12Texture2D* d3dTexture2d = reinterpret_cast<D3D12Texture2D*>(texture);
        if (!d3dTexture2d)
        {
            RTC_LOG(LS_INFO) << "texture is nullptr.";
            return nullptr;
        }

        ID3D12Resource* readbackResource = reinterpret_cast<ID3D12Resource*>(d3dTexture2d->GetNativeTexturePtrV());
        assert(readbackResource);
        if (!readbackResource) // the texture has to be prepared for CPU access
            return nullptr;

        const int width = static_cast<int>(d3dTexture2d->GetWidth());
        const int height = static_cast<int>(d3dTexture2d->GetHeight());
        const D3D12ResourceFootprint* footprint = d3dTexture2d->GetNativeTextureFootprint();
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

        D3D12Texture2D* d3d12Texture = static_cast<D3D12Texture2D*>(texture);
        ID3D12Resource* resource = static_cast<ID3D12Resource*>(d3d12Texture->GetNativeTexturePtrV());

        HANDLE sharedHandle = d3d12Texture->GetHandle();
        if (!sharedHandle)
        {
            RTC_LOG(LS_ERROR) << "cannot get shared handle";
            return nullptr;
        }

        D3D12_RESOURCE_DESC desc = resource->GetDesc();
        D3D12_RESOURCE_ALLOCATION_INFO d3d12ResourceAllocationInfo =
            m_d3d12Device->GetResourceAllocationInfo(0, 1, &desc);
        size_t actualSize = d3d12ResourceAllocationInfo.SizeInBytes;
        return GpuMemoryBufferCudaHandle::CreateHandle(GetCUcontext(), resource, sharedHandle, actualSize);
    }

    uint64_t D3D12GraphicsDevice::GetNextFrameFenceValue() const
    {
        if (m_unityInterface)
        {
            return m_unityInterface->GetNextFrameFenceValue();
        }
        else
        {
            return m_fence->GetCompletedValue() + 1;
        }
    }

    uint64_t D3D12GraphicsDevice::ExecuteCommandList(
        int listCount, ID3D12GraphicsCommandList* commandList, int stateCount, UnityGraphicsD3D12ResourceState* states)
    {
        if (m_unityInterface)
        {
            return m_unityInterface->ExecuteCommandList(commandList, stateCount, states);
        }
        else
        {
            ID3D12CommandList* cmdList = commandList;
            m_d3d12CommandQueue->ExecuteCommandLists(static_cast<UINT>(listCount), &cmdList);
            return m_fence->GetCompletedValue() + 1;
        }
    }

    ID3D12Fence* D3D12GraphicsDevice::GetFence()
    {
        if (m_unityInterface)
        {
            return m_unityInterface->GetFrameFence();
        }
        else
        {
            return m_fence.Get();
        }
    }

    bool D3D12GraphicsDevice::CreateFrame(Frame& frame)
    {
        frame.fenceValue = 0;

        HRESULT hr = m_d3d12Device->CreateCommandAllocator(
            D3D12_COMMAND_LIST_TYPE_DIRECT, IID_PPV_ARGS(&frame.commandAllocator));
        if (FAILED(hr))
        {
            RTC_LOG(LS_ERROR) << "ID3D12Device::CreateCommandAllocator is failed. " << HrToString(hr);
            return false;
        }

        hr = m_d3d12Device->CreateCommandList(
            0, D3D12_COMMAND_LIST_TYPE_DIRECT, frame.commandAllocator, nullptr, IID_PPV_ARGS(&frame.commandList));
        if (FAILED(hr))
        {
            RTC_LOG(LS_ERROR) << "ID3D12Device::CreateCommandList is failed. " << HrToString(hr);
            return false;
        }

        // Command lists are created in the recording state, but there is nothing
        // to record yet. The main loop expects it to be closed, so close it now.
        hr = frame.commandList->Close();
        if (FAILED(hr))
        {
            RTC_LOG(LS_ERROR) << "ID3D12GraphicsCommandList::Close is failed. " << HrToString(hr);
            return false;
        }
        return true;
    }

} // end namespace webrtc
} // end namespace unity
