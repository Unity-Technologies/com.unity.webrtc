#pragma once

#include "GraphicsDevice/IGraphicsDevice.h"
#include "WebRTCConstants.h"
#include <Metal/Metal.h>

namespace WebRTC {
    class MetalGraphicsDevice : public IGraphicsDevice{
    public:
        MetalGraphicsDevice(void* device);
        virtual ~MetalGraphicsDevice();

        virtual bool InitV() override;
        virtual void ShutdownV() override;
        inline virtual void* GetEncodeDevicePtrV() override;

        virtual ITexture2D* CreateDefaultTextureV(uint32_t w, uint32_t h) override;
        virtual ITexture2D* CreateDefaultTextureFromNativeV(uint32_t w, uint32_t h, void* nativeTexturePtr);
        virtual ITexture2D* CreateCPUReadTextureV(uint32_t width, uint32_t height) override;
        virtual bool CopyResourceV(ITexture2D* dest, ITexture2D* src) override;
        virtual bool CopyResourceFromNativeV(ITexture2D* dest, void* nativeTexturePtr) override;
        inline virtual GraphicsDeviceType GetDeviceType() const override;
        virtual rtc::scoped_refptr<webrtc::I420Buffer> ConvertRGBToI420(ITexture2D* tex) override;

        
    private:
        id<MTLDevice> m_device;
        id<MTLCommandQueue> m_commandQueue;
        bool CopyTexture(id<MTLTexture> dest, id<MTLTexture> src);
    };

    void* MetalGraphicsDevice::GetEncodeDevicePtrV() { return m_device; }
GraphicsDeviceType MetalGraphicsDevice::GetDeviceType() const { return GRAPHICS_DEVICE_METAL;}

//---------------------------------------------------------------------------------------------------------------------
}
