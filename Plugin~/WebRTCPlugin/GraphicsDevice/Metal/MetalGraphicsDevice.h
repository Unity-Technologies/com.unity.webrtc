#pragma once

#include <Metal/Metal.h>

#include "GpuMemoryBuffer.h"
#include "GraphicsDevice/IGraphicsDevice.h"

namespace unity
{
namespace webrtc
{
    using namespace ::webrtc;
    class MetalDevice;
    struct MetalTexture2D;
    class MetalGraphicsDevice : public IGraphicsDevice
    {
    public:
        MetalGraphicsDevice(MetalDevice* device, UnityGfxRenderer renderer, ProfilerMarkerFactory* profiler);
        virtual ~MetalGraphicsDevice() = default;

        bool InitV() override;
        void ShutdownV() override;
        inline virtual void* GetEncodeDevicePtrV() override;

        ITexture2D*
        CreateDefaultTextureV(uint32_t w, uint32_t h, UnityRenderingExtTextureFormat textureFormat) override;
        ITexture2D* CreateDefaultTextureFromNativeV(uint32_t w, uint32_t h, void* nativeTexturePtr);
        ITexture2D*
        CreateCPUReadTextureV(uint32_t width, uint32_t height, UnityRenderingExtTextureFormat textureFormat) override;
        bool CopyResourceV(ITexture2D* dest, ITexture2D* src) override;
        bool CopyResourceFromNativeV(ITexture2D* dest, void* nativeTexturePtr) override;
        rtc::scoped_refptr<I420Buffer> ConvertRGBToI420(ITexture2D* tex) override;
        std::unique_ptr<GpuMemoryBufferHandle> Map(ITexture2D* texture) override { return nullptr; }
        bool WaitSync(const ITexture2D* texture) override;
        bool ResetSync(const ITexture2D* texture) override;

    private:
        MetalDevice* m_device;
        id<MTLCommandQueue> m_queue;
        bool CopyTexture(MetalTexture2D* dest, id<MTLTexture> src);
        static MTLPixelFormat ConvertFormat(UnityRenderingExtTextureFormat format);
    };

    void* MetalGraphicsDevice::GetEncodeDevicePtrV() { return m_device->Device(); }
} // end namespace webrtc
} // end namespace unity
