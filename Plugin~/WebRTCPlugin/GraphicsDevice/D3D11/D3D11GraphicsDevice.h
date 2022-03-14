#pragma once

#include "WebRTCConstants.h"
#include "GraphicsDevice/IGraphicsDevice.h"
#include "GraphicsDevice/Cuda/CudaContext.h"

namespace unity
{
namespace webrtc
{

class D3D11GraphicsDevice : public IGraphicsDevice
{
public:
    D3D11GraphicsDevice(ID3D11Device* nativeDevice, UnityGfxRenderer renderer);
    virtual ~D3D11GraphicsDevice();
    virtual bool InitV() override;
    virtual void ShutdownV() override;
    inline virtual void* GetEncodeDevicePtrV() override;
    virtual ITexture2D* CreateDefaultTextureV(uint32_t w, uint32_t h, UnityRenderingExtTextureFormat textureFormat) override;
    virtual ITexture2D* CreateCPUReadTextureV(uint32_t w, uint32_t h, UnityRenderingExtTextureFormat textureFormat) override;
    virtual bool CopyResourceV(ITexture2D* dest, ITexture2D* src) override;
    virtual bool CopyResourceFromNativeV(ITexture2D* dest, void* nativeTexturePtr) override;
    inline virtual GraphicsDeviceType GetDeviceType() const override;
    std::unique_ptr<GpuMemoryBufferHandle> Map(ITexture2D* texture) override;
    virtual rtc::scoped_refptr < ::webrtc::I420Buffer > ConvertRGBToI420(ITexture2D* tex) override;

    bool IsCudaSupport() override { return m_isCudaSupport; }
    CUcontext GetCUcontext() override { return m_cudaContext.GetContext(); }
    NV_ENC_BUFFER_FORMAT GetEncodeBufferFormat() override { return NV_ENC_BUFFER_FORMAT_ARGB; }
private:
    ID3D11Device* m_d3d11Device;

    bool m_isCudaSupport;
    CudaContext m_cudaContext;
};

//---------------------------------------------------------------------------------------------------------------------

void* D3D11GraphicsDevice::GetEncodeDevicePtrV() { return reinterpret_cast<void*>(m_d3d11Device); }
GraphicsDeviceType D3D11GraphicsDevice::GetDeviceType() const { return GRAPHICS_DEVICE_D3D11; }

} // end namespace webrtc
} // end namespace unity
