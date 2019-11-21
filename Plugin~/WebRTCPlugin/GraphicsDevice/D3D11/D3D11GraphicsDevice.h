#pragma once

#include "GraphicsDevice/IGraphicsDevice.h"
#include "WebRTCConstants.h"

namespace WebRTC {


class D3D11GraphicsDevice : public IGraphicsDevice{
public:
    D3D11GraphicsDevice(ID3D11Device* nativeDevice);
    virtual ~D3D11GraphicsDevice();
    virtual bool InitV() override;
    virtual void ShutdownV() override;
    inline virtual void* GetEncodeDevicePtrV() override;
    virtual ITexture2D* CreateDefaultTextureV(uint32_t w, uint32_t h) override;
    virtual ITexture2D* CreateCPUReadTextureV(uint32_t w, uint32_t h) override;
    virtual bool CopyResourceV(ITexture2D* dest, ITexture2D* src) override;
    virtual bool CopyResourceFromNativeV(ITexture2D* dest, void* nativeTexturePtr) override;
    inline virtual GraphicsDeviceType GetDeviceType() override;
    virtual rtc::scoped_refptr<webrtc::I420Buffer> ConvertRGBToI420(ITexture2D* tex) override;

private:
    ID3D11Device* m_d3d11Device;
    ID3D11DeviceContext* m_d3d11Context; 
};

//---------------------------------------------------------------------------------------------------------------------

void* D3D11GraphicsDevice::GetEncodeDevicePtrV() { return reinterpret_cast<void*>(m_d3d11Device); }
GraphicsDeviceType D3D11GraphicsDevice::GetDeviceType() { return GRAPHICS_DEVICE_D3D11; }


}
