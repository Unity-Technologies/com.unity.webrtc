#include "pch.h"

#include "GpuMemoryBuffer.h"
#include "GraphicsDevice/ITexture2D.h"

namespace unity
{
namespace webrtc
{
    GpuMemoryBufferHandle::GpuMemoryBufferHandle()
#if CUDA_PLATFORM
        : array(nullptr)
        , devicePtr(0)
        , resource(nullptr)
#endif
    {
    }
    GpuMemoryBufferHandle::GpuMemoryBufferHandle(GpuMemoryBufferHandle&& other) = default;
    GpuMemoryBufferHandle& GpuMemoryBufferHandle::operator=(GpuMemoryBufferHandle&& other) = default;

    GpuMemoryBufferHandle::~GpuMemoryBufferHandle()
    {
#if CUDA_PLATFORM
        if (resource != nullptr)
        {
            cuGraphicsUnmapResources(1, &resource, 0);
            cuGraphicsUnregisterResource(resource);
        }
#endif
    }

    GpuMemoryBufferFromUnity::GpuMemoryBufferFromUnity(
        IGraphicsDevice* device, NativeTexPtr ptr, const Size& size, UnityRenderingExtTextureFormat format)
        : device_(device)
        , format_(format)
        , size_(size)
        , texture_(nullptr)
        , textureCpuRead_(nullptr)
        , handle_(nullptr)
    {
        texture_.reset(device_->CreateDefaultTextureV(size.width(), size.height(), format));
        textureCpuRead_.reset(device_->CreateCPUReadTextureV(size.width(), size.height(), format));

        // IGraphicsDevice::Map method is too heavy and stop the graphics process,
        // so must not call this method on the worker thread instead of the render thread.
        handle_ = device_->Map(texture_.get());

        CopyBuffer(ptr);
    }

    GpuMemoryBufferFromUnity::~GpuMemoryBufferFromUnity() { }

    void GpuMemoryBufferFromUnity::CopyBuffer(NativeTexPtr ptr)
    {
        // One texture cannot map CUDA memory and CPU memory simultaneously.
        // Believe there is still room for improvement.
        device_->CopyResourceFromNativeV(texture_.get(), ptr);
        device_->CopyResourceFromNativeV(textureCpuRead_.get(), ptr);
    }

    UnityRenderingExtTextureFormat GpuMemoryBufferFromUnity::GetFormat() const { return format_; }

    Size GpuMemoryBufferFromUnity::GetSize() const { return size_; }

    rtc::scoped_refptr<I420BufferInterface> GpuMemoryBufferFromUnity::ToI420()
    {
        return device_->ConvertRGBToI420(textureCpuRead_.get());
    }
}
}
