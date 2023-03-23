#pragma once

#include <shared_mutex>

#include <common_video/include/video_frame_buffer.h>
#include <rtc_base/ref_counted_object.h>

#include "GraphicsDevice/GraphicsDevice.h"
#include "IUnityRenderingExtensions.h"
#include "PlatformBase.h"
#include "Size.h"

#if CUDA_PLATFORM
#include <cuda.h>
#endif

namespace unity
{
namespace webrtc
{
    using namespace ::webrtc;

    struct GpuMemoryBufferHandle
    {
        GpuMemoryBufferHandle();
        GpuMemoryBufferHandle(GpuMemoryBufferHandle&& other);
        GpuMemoryBufferHandle& operator=(GpuMemoryBufferHandle&& other);
        virtual ~GpuMemoryBufferHandle();
    };

    class ITexture2D;
    class GpuMemoryBufferInterface : public rtc::RefCountInterface
    {
    public:
        virtual Size GetSize() const = 0;
        virtual UnityRenderingExtTextureFormat GetFormat() const = 0;
        virtual rtc::scoped_refptr<I420BufferInterface> ToI420() = 0;

        virtual const GpuMemoryBufferHandle* handle() const = 0;

    protected:
        ~GpuMemoryBufferInterface() override = default;
    };

    class GpuMemoryBufferFromUnity : public GpuMemoryBufferInterface
    {
    public:
        GpuMemoryBufferFromUnity(IGraphicsDevice* device, const Size& size, UnityRenderingExtTextureFormat format);
        GpuMemoryBufferFromUnity(const GpuMemoryBufferFromUnity&) = delete;
        GpuMemoryBufferFromUnity& operator=(const GpuMemoryBufferFromUnity&) = delete;

        bool ResetSync();
        bool CopyBuffer(NativeTexPtr ptr);
        UnityRenderingExtTextureFormat GetFormat() const override;
        Size GetSize() const override;
        rtc::scoped_refptr<I420BufferInterface> ToI420() override;
        const GpuMemoryBufferHandle* handle() const override;

    protected:
        ~GpuMemoryBufferFromUnity() override;

    private:
        IGraphicsDevice* device_;
        UnityRenderingExtTextureFormat format_;
        Size size_;
        std::unique_ptr<ITexture2D> texture_;
        std::unique_ptr<ITexture2D> textureCpuRead_;
        std::unique_ptr<GpuMemoryBufferHandle> handle_;
    };
}
}
