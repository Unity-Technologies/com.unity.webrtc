#pragma once

namespace WebRTC {

struct ITexture2D;

class IGraphicsDevice {
public:

    IGraphicsDevice();
    virtual ~IGraphicsDevice() = 0;

    virtual void InitV() = 0;
    virtual void ShutdownV() = 0;
    virtual ITexture2D* CreateDefaultTextureV(uint32_t width, uint32_t height) = 0;
    virtual ITexture2D* CreateDefaultTextureFromNativeV(uint32_t width, uint32_t height, void* nativeTexturePtr) = 0;
    virtual void* GetEncodeDevicePtrV() = 0;
    virtual void CopyResourceV(ITexture2D* dest, ITexture2D* src) = 0;
};

}
