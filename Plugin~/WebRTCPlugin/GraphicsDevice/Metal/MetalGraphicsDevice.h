#pragma once

#include "GraphicsDevice/IGraphicsDevice.h"
#include "WebRTCConstants.h"
#include <Metal/Metal.h>

namespace unity
{
namespace webrtc
{
    namespace webrtc = ::webrtc;
    class MetalGraphicsDevice : public IGraphicsDevice
    {
    public:
        MetalGraphicsDevice(id<MTLDevice> device, IUnityGraphicsMetal* unityGraphicsMetal, UnityGfxRenderer renderer);
        virtual ~MetalGraphicsDevice();

        bool InitV() override;
        void ShutdownV() override;
        inline virtual void* GetEncodeDevicePtrV() override;

        ITexture2D* CreateDefaultTextureV(uint32_t w, uint32_t h, UnityRenderingExtTextureFormat textureFormat) override;
        ITexture2D* CreateDefaultTextureFromNativeV(uint32_t w, uint32_t h, void* nativeTexturePtr);
        ITexture2D* CreateCPUReadTextureV(uint32_t width, uint32_t height, UnityRenderingExtTextureFormat textureFormat) override;
        bool CopyResourceV(ITexture2D* dest, ITexture2D* src) override;
        bool CopyResourceFromNativeV(ITexture2D* dest, void* nativeTexturePtr) override;
        inline GraphicsDeviceType GetDeviceType() const override;
        rtc::scoped_refptr<webrtc::I420Buffer> ConvertRGBToI420(ITexture2D* tex) override;
        std::unique_ptr<GpuMemoryBufferHandle> Map(ITexture2D* texture) override { /* todo */ throw; };
        
    private:
        id<MTLDevice> m_device;
        
        bool CopyTexture(id<MTLTexture> dest, id<MTLTexture> src);
        IUnityGraphicsMetal* m_unityGraphicsMetal;

        static MTLPixelFormat ConvertFormat(UnityRenderingExtTextureFormat format);
    };

    void* MetalGraphicsDevice::GetEncodeDevicePtrV() { return m_device; }
    GraphicsDeviceType MetalGraphicsDevice::GetDeviceType() const { return GRAPHICS_DEVICE_METAL;}
} // end namespace webrtc
} // end namespace unity
