#pragma once

#include <d3d11.h>
#include <d3d11_4.h>
#include <memory>
#include <wrl/client.h>

#include "GraphicsDevice/Cuda/CudaContext.h"
#include "GraphicsDevice/IGraphicsDevice.h"
#include "nvEncodeAPI.h"

using namespace Microsoft::WRL;

namespace unity
{
namespace webrtc
{

    class D3D11GraphicsDevice : public IGraphicsDevice
    {
    public:
        D3D11GraphicsDevice(ID3D11Device* nativeDevice, UnityGfxRenderer renderer, ProfilerMarkerFactory* profiler);
        virtual ~D3D11GraphicsDevice() override;
        bool InitV() override;
        void ShutdownV() override;
        inline virtual void* GetEncodeDevicePtrV() override;
        rtc::scoped_refptr<::webrtc::VideoFrameBuffer>
        CreateVideoFrameBuffer(uint32_t width, uint32_t height, UnityRenderingExtTextureFormat textureFormat) override;
        ITexture2D*
        CreateDefaultTextureV(uint32_t w, uint32_t h, UnityRenderingExtTextureFormat textureFormat) override;
        ITexture2D*
        CreateCPUReadTextureV(uint32_t w, uint32_t h, UnityRenderingExtTextureFormat textureFormat) override;
        bool CopyResourceV(ITexture2D* dest, ITexture2D* src) override;
        bool CopyResourceFromNativeV(ITexture2D* dest, void* texture) override;
        bool CopyResourceFromBuffer(void* dest, rtc::scoped_refptr<::webrtc::VideoFrameBuffer> buffer) override;
        bool CopyToVideoFrameBuffer(rtc::scoped_refptr<::webrtc::VideoFrameBuffer>& buffer, void* texture) override;

        std::unique_ptr<GpuMemoryBufferHandle>
        Map(ITexture2D* texture, GpuMemoryBufferHandle::AccessMode mode) override;
        bool WaitSync(const ITexture2D* texture, uint64_t nsTimeout = 0) override;
        bool ResetSync(const ITexture2D* texture) override;
        rtc::scoped_refptr<::webrtc::I420Buffer> ConvertRGBToI420(ITexture2D* tex) override;
        rtc::scoped_refptr<::webrtc::VideoFrameBuffer> ConvertToBuffer(void* texture) override;

        bool IsCudaSupport() override { return m_isCudaSupport; }
        CUcontext GetCUcontext() override { return m_cudaContext.GetContext(); }
        NV_ENC_BUFFER_FORMAT GetEncodeBufferFormat() override { return NV_ENC_BUFFER_FORMAT_ARGB; }

    private:
        HRESULT Signal(ID3D11Fence* fence);
        ID3D11Device* m_d3d11Device;

        bool m_isCudaSupport;
        CudaContext m_cudaContext;
    };

    //---------------------------------------------------------------------------------------------------------------------

    void* D3D11GraphicsDevice::GetEncodeDevicePtrV() { return reinterpret_cast<void*>(m_d3d11Device); }

} // end namespace webrtc
} // end namespace unity
