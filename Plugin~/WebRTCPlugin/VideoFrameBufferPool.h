#pragma once

#include <common_video/include/video_frame_buffer.h>
#include <list>
#include <shared_mutex>
#include <system_wrappers/include/clock.h>

#include "GpuMemoryBuffer.h"
#include "GraphicsDevice/ITexture2D.h"

namespace unity
{
namespace webrtc
{
    class NativeFrameBuffer : public VideoFrameBuffer
    {
    public:
        static rtc::scoped_refptr<NativeFrameBuffer>
        Create(int width, int height, UnityRenderingExtTextureFormat format, IGraphicsDevice* device)
        {
            return new rtc::RefCountedObject<NativeFrameBuffer>(width, height, format, device);
        }
        VideoFrameBuffer::Type type() const override { return Type::kNative; }
        int width() const override { return texture_->GetWidth(); }
        int height() const override { return texture_->GetHeight(); }
        UnityRenderingExtTextureFormat format() { return texture_->GetFormat(); };
        ITexture2D* texture() { return texture_.get(); }
        rtc::scoped_refptr<I420BufferInterface> ToI420() override { return I420Buffer::Create(width(), height()); }
        const webrtc::I420BufferInterface* GetI420() const override { return I420Buffer::Create(width(), height()); }
        const GpuMemoryBufferHandle* handle() { return handle_.get(); }

    protected:
        NativeFrameBuffer(int width, int height, UnityRenderingExtTextureFormat format, IGraphicsDevice* device)
            : texture_(device->CreateDefaultTextureV(width, height, format))
            , handle_(device->Map(texture_.get()))
        {
        }
        ~NativeFrameBuffer() override { }

    private:
        std::unique_ptr<ITexture2D> texture_;
        std::unique_ptr<GpuMemoryBufferHandle> handle_;
    };

    class IGraphicsDevice;
    class VideoFrameBufferPool
    {
    public:
        VideoFrameBufferPool(IGraphicsDevice* device, Clock* clock);
        VideoFrameBufferPool(const VideoFrameBufferPool&) = delete;
        VideoFrameBufferPool& operator=(const VideoFrameBufferPool&) = delete;

        virtual ~VideoFrameBufferPool();

        rtc::scoped_refptr<VideoFrameBuffer> Create(int width, int height, UnityRenderingExtTextureFormat format);
    private:
        IGraphicsDevice* device_;
        Clock* clock_;
        std::list<rtc::scoped_refptr<VideoFrameBuffer>> pool_;
    };
}
}
