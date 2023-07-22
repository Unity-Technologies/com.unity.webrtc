#pragma once

#include <shared_mutex>

#include <common_video/include/video_frame_buffer.h>
#include <rtc_base/ref_counted_object.h>

//#include "GraphicsDevice/GraphicsDevice.h"
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
        enum class AccessMode
        {
            kRead,
            kWrite
        };

        GpuMemoryBufferHandle();
        GpuMemoryBufferHandle(GpuMemoryBufferHandle&& other);
        GpuMemoryBufferHandle& operator=(GpuMemoryBufferHandle&& other);
        virtual ~GpuMemoryBufferHandle();
    };

#if __ANDROID__
    struct AHardwareBufferHandle : public GpuMemoryBufferHandle
    {
        AHardwareBufferHandle();
        AHardwareBufferHandle(AHardwareBufferHandle&& other);
        AHardwareBufferHandle& operator=(AHardwareBufferHandle&& other);
        virtual ~AHardwareBufferHandle() override;

        AHardwareBuffer* buffer;
    };
#endif

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

#if CUDA_PLATFORM
    class GpuMemoryBufferFromCuda : public GpuMemoryBufferInterface
    {
    public:
        GpuMemoryBufferFromCuda(
            CUcontext context,
            CUdeviceptr ptr,
            const Size& size,
            UnityRenderingExtTextureFormat format,
            GpuMemoryBufferHandle::AccessMode mode);
        GpuMemoryBufferFromCuda(const GpuMemoryBufferFromCuda&) = delete;
        GpuMemoryBufferFromCuda& operator=(const GpuMemoryBufferFromCuda&) = delete;
        UnityRenderingExtTextureFormat GetFormat() const override;
        Size GetSize() const override;
        rtc::scoped_refptr<I420BufferInterface> ToI420() override;
        const GpuMemoryBufferHandle* handle() const override;

    protected:
        ~GpuMemoryBufferFromCuda() override;

    private:
        UnityRenderingExtTextureFormat format_;
        Size size_;
        std::unique_ptr<GpuMemoryBufferHandle> handle_;
    };
#endif

    class IGraphicsDevice;
    class GpuMemoryBufferFromUnity : public GpuMemoryBufferInterface
    {
    public:
        GpuMemoryBufferFromUnity(
            IGraphicsDevice* device,
            void* ptr,
            const Size& size,
            UnityRenderingExtTextureFormat format,
            GpuMemoryBufferHandle::AccessMode mode);
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
