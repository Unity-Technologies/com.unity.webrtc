#pragma once

#include <comdef.h>
#include <d3d12.h>
#include <stdexcept>
#include <wrl/client.h>

#include <IUnityGraphicsD3D12.h>

#include "D3D12Texture2D.h"
#include "GraphicsDevice/Cuda/CudaContext.h"
#include "GraphicsDevice/IGraphicsDevice.h"

using namespace Microsoft::WRL;

namespace unity
{
namespace webrtc
{
    namespace webrtc = ::webrtc;

#define DefPtr(_a) _COM_SMARTPTR_TYPEDEF(_a, __uuidof(_a))
    DefPtr(ID3D12CommandAllocator);
    DefPtr(ID3D12GraphicsCommandList4);

    inline std::string HrToString(HRESULT hr)
    {

        char s_str[64] = {};
        sprintf_s(s_str, "HRESULT of 0x%08X", static_cast<UINT>(hr));
        return std::string(s_str);
    }

    class HrException : public std::runtime_error
    {
    public:
        HrException(HRESULT hr)
            : std::runtime_error(HrToString(hr))
            , m_hr(hr)
        {
        }
        HRESULT Error() const { return m_hr; }

    private:
        const HRESULT m_hr;
    };

    inline void ThrowIfFailed(HRESULT hr)
    {
        if (FAILED(hr))
        {
            throw HrException(hr);
        }
    }

    class D3D12GraphicsDevice : public IGraphicsDevice
    {
    public:
        explicit D3D12GraphicsDevice(
            ID3D12Device* nativeDevice,
            IUnityGraphicsD3D12v5* unityInterface,
            UnityGfxRenderer renderer,
            ProfilerMarkerFactory* profiler);
        explicit D3D12GraphicsDevice(
            ID3D12Device* nativeDevice,
            ID3D12CommandQueue* commandQueue,
            UnityGfxRenderer renderer,
            ProfilerMarkerFactory* profiler);
        virtual ~D3D12GraphicsDevice();
        virtual bool InitV() override;
        virtual void ShutdownV() override;
        inline virtual void* GetEncodeDevicePtrV() override;

        virtual ITexture2D*
        CreateDefaultTextureV(uint32_t w, uint32_t h, UnityRenderingExtTextureFormat textureFormat) override;
        virtual bool CopyResourceV(ITexture2D* dest, ITexture2D* src) override;
        virtual bool CopyResourceFromNativeV(ITexture2D* dest, void* nativeTexturePtr) override;
        std::unique_ptr<GpuMemoryBufferHandle> Map(ITexture2D* texture) override;
        bool WaitSync(const ITexture2D* texture) override;
        bool ResetSync(const ITexture2D* texture) override;
        bool WaitIdleForTest() override;

        virtual ITexture2D*
        CreateCPUReadTextureV(uint32_t w, uint32_t h, UnityRenderingExtTextureFormat textureFormat) override;
        virtual rtc::scoped_refptr<webrtc::I420Buffer> ConvertRGBToI420(ITexture2D* texture) override;

        bool IsCudaSupport() override { return m_isCudaSupport; }
        CUcontext GetCUcontext() override { return m_cudaContext.GetContext(); }
        NV_ENC_BUFFER_FORMAT GetEncodeBufferFormat() override { return NV_ENC_BUFFER_FORMAT_ARGB; }

    private:
        struct Frame
        {
            ID3D12CommandAllocatorPtr commandAllocator;
            ID3D12GraphicsCommandList4Ptr commandList;
            uint64_t fenceValue;
        };
        D3D12Texture2D* CreateSharedD3D12Texture(uint32_t w, uint32_t h);
        uint64_t GetNextFrameFenceValue() const;
        uint64_t ExecuteCommandList(
            int listCount,
            ID3D12GraphicsCommandList* commandList,
            int stateCount,
            UnityGraphicsD3D12ResourceState* states);
        ID3D12Fence* GetFence();
        bool CreateFrame(Frame& frame);

        IUnityGraphicsD3D12v5* m_unityInterface;
        ComPtr<ID3D12Device> m_d3d12Device;
        ComPtr<ID3D12CommandQueue> m_d3d12CommandQueue;
        ComPtr<ID3D12Fence> m_fence;

        bool m_isCudaSupport;
        CudaContext m_cudaContext;

        std::vector<Frame> m_frames;
    };

    //---------------------------------------------------------------------------------------------------------------------

    // use D3D11. See notes below
    void* D3D12GraphicsDevice::GetEncodeDevicePtrV() { return reinterpret_cast<void*>(m_d3d12Device.Get()); }

} // end namespace webrtc
} // end namespace unity

//---------------------------------------------------------------------------------------------------------------------
//[Note-sin: 2019-10-30]
// Since NVEncoder does not support DX12, we use a DX12 resource that can be shared with DX11, and then pass it
// the DX11 resource to NVidia Encoder
