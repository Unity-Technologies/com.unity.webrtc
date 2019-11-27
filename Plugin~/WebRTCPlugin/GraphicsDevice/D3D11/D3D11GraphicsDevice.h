#pragma once

#include "GraphicsDevice/IGraphicsDevice.h"
#include "WebRTCConstants.h"

namespace WebRTC {


class D3D11GraphicsDevice : public IGraphicsDevice{
public:
    D3D11GraphicsDevice(ID3D11Device* nativeDevice);
    virtual ~D3D11GraphicsDevice();
    virtual bool InitV();
    virtual void ShutdownV();
    inline virtual void* GetEncodeDevicePtrV();
    virtual ITexture2D* CreateDefaultTextureV(uint32_t w, uint32_t h);
    virtual ITexture2D* CreateStagingTextureV(uint32_t w, uint32_t h);
    virtual ITexture2D* CreateDefaultTextureFromNativeV(uint32_t w, uint32_t h, void* nativeTexturePtr);
    virtual bool CopyResourceV(ITexture2D* dest, ITexture2D* src);
    virtual bool CopyResourceFromNativeV(ITexture2D* dest, void* nativeTexturePtr);
    inline virtual GraphicsDeviceType GetDeviceType();
    virtual rtc::scoped_refptr<webrtc::I420Buffer> ConvertRGBToI420(ITexture2D* tex);

private:
    ID3D11Device* m_d3d11Device;
    ID3D11DeviceContext* m_d3d11Context; 
};

//---------------------------------------------------------------------------------------------------------------------

void* D3D11GraphicsDevice::GetEncodeDevicePtrV() { return reinterpret_cast<void*>(m_d3d11Device); }
GraphicsDeviceType D3D11GraphicsDevice::GetDeviceType() { return GRAPHICS_DEVICE_D3D11; }


}
