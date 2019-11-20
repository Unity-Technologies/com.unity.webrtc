#pragma once

#include "GraphicsDevice/GraphicsDeviceType.h"

namespace WebRTC {

class ITexture2D;

class IGraphicsDevice {
public:

    IGraphicsDevice();
    virtual ~IGraphicsDevice() = 0;
    virtual bool InitV() = 0;
    virtual void ShutdownV() = 0;
    virtual ITexture2D* CreateDefaultTextureV(uint32_t width, uint32_t height) = 0;
    virtual ITexture2D* CreateCPUReadTextureV(uint32_t width, uint32_t height) = 0;
    virtual ITexture2D* CreateDefaultTextureFromNativeV(uint32_t width, uint32_t height, void* nativeTexturePtr) = 0;
    virtual void* GetEncodeDevicePtrV() = 0;
    virtual bool CopyResourceV(ITexture2D* dest, ITexture2D* src) = 0;
    virtual bool CopyResourceFromNativeV(ITexture2D* dest, void* nativeTexturePtr) = 0;
    virtual GraphicsDeviceType GetDeviceType() = 0;
    virtual rtc::scoped_refptr<webrtc::I420Buffer> ConvertRGBToI420(ITexture2D* tex) = 0;

};

}
