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
    {
        texture_.reset(device_->CreateCPUReadTextureV(size.width(), size.height(), format));

        CopyBuffer(ptr);
    }

    GpuMemoryBufferFromUnity::~GpuMemoryBufferFromUnity() { }

    void GpuMemoryBufferFromUnity::CopyBuffer(NativeTexPtr ptr)
    {
        device_->CopyResourceFromNativeV(texture_.get(), ptr);
    }

    UnityRenderingExtTextureFormat GpuMemoryBufferFromUnity::GetFormat() const { return format_; }

    Size GpuMemoryBufferFromUnity::GetSize() const { return size_; }

    rtc::scoped_refptr<I420BufferInterface> GpuMemoryBufferFromUnity::ToI420()
    {
        return device_->ConvertRGBToI420(texture_.get());
    }

    void GpuMemoryBufferFromUnity::CopyTo(ITexture2D* tex) { device_->CopyResourceV(tex, texture_.get()); }

    //FakeGpuMemoryBuffer::FakeGpuMemoryBuffer(const ITexture2D* texture, UnityRenderingExtTextureFormat format)
    //    : format_(format)
    //    , texture_(texture)
    //{
    //    size_ = Size(texture_->GetWidth(), texture_->GetHeight());
    //}

    //void FakeGpuMemoryBuffer::CopyTo(ITexture2D* tex) { }

    //FakeGpuMemoryBuffer::~FakeGpuMemoryBuffer() { }
}
}
