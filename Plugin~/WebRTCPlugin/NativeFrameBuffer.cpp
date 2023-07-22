#include "pch.h"

#include "GraphicsDevice/IGraphicsDevice.h"
#include "NativeFrameBuffer.h"

namespace unity
{
namespace webrtc
{
    NativeFrameBuffer::NativeFrameBuffer(
        int width,
        int height,
        UnityRenderingExtTextureFormat format,
        IGraphicsDevice* device)
        : device_(device)
        , texture_(device->CreateDefaultTextureV(width, height, format))
        , handle_(nullptr)
    {
    }
    NativeFrameBuffer::~NativeFrameBuffer() { }

    rtc::scoped_refptr<I420BufferInterface> NativeFrameBuffer::ToI420()
    {
        return I420Buffer::Create(width(), height());
    }
    const webrtc::I420BufferInterface* NativeFrameBuffer::GetI420() const
    {
        return I420Buffer::Create(width(), height());
    }

    void NativeFrameBuffer::Map(GpuMemoryBufferHandle::AccessMode mode)
    {
        // This buffer is already mapped.
        RTC_DCHECK(!handle_);
        handle_ = device_->Map(texture_.get(), mode);
    }

    void NativeFrameBuffer::Unmap()
    {
        // This buffer is not mapped.
        RTC_DCHECK(handle_);
        handle_ = nullptr;
    }
}
}
