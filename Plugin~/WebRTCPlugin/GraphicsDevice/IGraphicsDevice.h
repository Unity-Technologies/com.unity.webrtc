#pragma once

#include <IUnityRenderingExtensions.h>
#include <memory>

#include "PlatformBase.h"
#include "api/video/i420_buffer.h"

#if CUDA_PLATFORM
#include "Cuda/ICudaDevice.h"
#endif

namespace unity
{
namespace webrtc
{
    using NativeTexPtr = void*;
    class ITexture2D;
    struct GpuMemoryBufferHandle;
    class IGraphicsDevice
#if CUDA_PLATFORM
        : public ICudaDevice
#endif
    {
    public:
        IGraphicsDevice(UnityGfxRenderer renderer)
            : m_gfxRenderer(renderer)
        {
        }
#if CUDA_PLATFORM
        virtual ~IGraphicsDevice() override = default;
#else
        virtual ~IGraphicsDevice() = default;
#endif
        virtual bool InitV() = 0;
        virtual void ShutdownV() = 0;
        virtual ITexture2D*
        CreateDefaultTextureV(uint32_t width, uint32_t height, UnityRenderingExtTextureFormat textureFormat) = 0;
        virtual void* GetEncodeDevicePtrV() = 0;
        virtual bool CopyResourceV(ITexture2D* dest, ITexture2D* src) = 0;
        virtual bool CopyResourceFromNativeV(ITexture2D* dest, NativeTexPtr nativeTexturePtr) = 0;
        virtual NativeTexPtr ConvertNativeFromUnityPtr(void* tex) { return tex; }
        virtual UnityGfxRenderer GetGfxRenderer() const { return m_gfxRenderer; }
        virtual std::unique_ptr<GpuMemoryBufferHandle> Map(ITexture2D* texture) = 0;

        // Required for software encoding
        virtual ITexture2D*
        CreateCPUReadTextureV(uint32_t width, uint32_t height, UnityRenderingExtTextureFormat textureFormat) = 0;
        virtual rtc::scoped_refptr<::webrtc::I420Buffer> ConvertRGBToI420(ITexture2D* tex) = 0;

    protected:
        UnityGfxRenderer m_gfxRenderer;
    };

} // end namespace webrtc
} // end namespace unity
