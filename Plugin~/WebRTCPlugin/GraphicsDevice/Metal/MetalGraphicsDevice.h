#pragma once

#include "GraphicsDevice/IGraphicsDevice.h"
#include "WebRTCConstants.h"

namespace WebRTC {


    class MetalGraphicsDevice : public IGraphicsDevice{
    public:
        MetalGraphicsDevice();
        virtual ~MetalGraphicsDevice();

        virtual bool InitV();
        virtual void ShutdownV();
        inline virtual void* GetEncodeDevicePtrV();

        virtual ITexture2D* CreateDefaultTextureV(uint32_t w, uint32_t h);
        virtual ITexture2D* CreateDefaultTextureFromNativeV(uint32_t w, uint32_t h, void* nativeTexturePtr);
        virtual bool CopyResourceV(ITexture2D* dest, ITexture2D* src);
        virtual bool CopyResourceFromNativeV(ITexture2D* dest, void* nativeTexturePtr);
    };

    void* MetalGraphicsDevice::GetEncodeDevicePtrV() { return nullptr; }

//---------------------------------------------------------------------------------------------------------------------
}
