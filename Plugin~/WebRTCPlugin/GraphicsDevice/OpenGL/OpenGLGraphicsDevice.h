#pragma once

#include "GraphicsDevice/IGraphicsDevice.h"
#include "WebRTCConstants.h"

namespace WebRTC {


class OpenGLGraphicsDevice : public IGraphicsDevice{
public:
    OpenGLGraphicsDevice();
    virtual ~OpenGLGraphicsDevice();

    virtual void InitV();
    virtual void ShutdownV();
    inline virtual void* GetEncodeDevicePtrV();

    virtual ITexture2D* CreateDefaultTextureV(uint32_t w, uint32_t h);
    virtual ITexture2D* CreateDefaultTextureFromNativeV(uint32_t w, uint32_t h, void* nativeTexturePtr);
    virtual bool CopyResourceV(ITexture2D* dest, ITexture2D* src);
    virtual void CopyResourceFromNativeV(ITexture2D* dest, void* nativeTexturePtr);
};

void* OpenGLGraphicsDevice::GetEncodeDevicePtrV() { return nullptr; }

//---------------------------------------------------------------------------------------------------------------------
}
