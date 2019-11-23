#pragma once

#include "GraphicsDevice/IGraphicsDevice.h"
#include "WebRTCConstants.h"

namespace WebRTC {
    class MTLDevice;
    class MTLTexture;
    class MetalGraphicsDevice : public IGraphicsDevice{
    public:
        MetalGraphicsDevice(void* device);
        virtual ~MetalGraphicsDevice();

        virtual bool InitV();
        virtual void ShutdownV();
        inline virtual void* GetEncodeDevicePtrV();

        virtual ITexture2D* CreateDefaultTextureV(uint32_t w, uint32_t h);
        virtual ITexture2D* CreateDefaultTextureFromNativeV(uint32_t w, uint32_t h, void* nativeTexturePtr);
        virtual bool CopyResourceV(ITexture2D* dest, ITexture2D* src);
        virtual bool CopyResourceFromNativeV(ITexture2D* dest, void* nativeTexturePtr);
    private:
        id<MTLDevice> m_device;
        bool CopyTexture(id<MTLTexture> dest, id<MTLTexture> src);
    };

    void* MetalGraphicsDevice::GetEncodeDevicePtrV() { return m_device; }

//---------------------------------------------------------------------------------------------------------------------
}
