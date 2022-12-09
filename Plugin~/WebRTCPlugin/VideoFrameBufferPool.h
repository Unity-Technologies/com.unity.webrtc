#pragma once

#include <api/make_ref_counted.h>
#include <common_video/include/video_frame_buffer.h>
#include <list>
#include <shared_mutex>
#include <system_wrappers/include/clock.h>

#include "GpuMemoryBuffer.h"
#include "GraphicsDevice/ITexture2D.h"
#include "Size.h"
#include "VideoFrame.h"

namespace unity
{
namespace webrtc
{
    enum VideoFrameBufferState
    {
        kUnknown = 0,
        kUnused = 1,
        kReserved = 2,
        kUsed = 3,
    };

    class NativeFrameBuffer : public VideoFrameBuffer
    {
    public:
        static rtc::scoped_refptr<NativeFrameBuffer> Create(void* texture, IGraphicsDevice* device)
        {
            return rtc::make_ref_counted<NativeFrameBuffer>(texture, device);
        }

        VideoFrameBuffer::Type type() const override { return Type::kNative; }
        int width() const override { return width_; }
        int height() const override { return height_; }
        rtc::scoped_refptr<I420BufferInterface> ToI420() override { return I420Buffer::Create(width_, height_); }
        const webrtc::I420BufferInterface* GetI420() const override { return nullptr; }
        const GpuMemoryBufferHandle* handle() const { return handle_.get(); }

    protected:
        NativeFrameBuffer(void* texture, IGraphicsDevice* device)
            : texture_(device->BindTexture(texture))
            , handle_(device->Map(texture_.get()))
            , width_(static_cast<int>(texture_->GetWidth()))
            , height_(static_cast<int>(texture_->GetHeight()))
        {
        }
        ~NativeFrameBuffer() override { }

    private:
        std::unique_ptr<ITexture2D> texture_;
        std::unique_ptr<GpuMemoryBufferHandle> handle_;
        const int width_;
        const int height_;
    };

    class IGraphicsDevice;
    class VideoFrameBufferPool
    {
    public:
        VideoFrameBufferPool(IGraphicsDevice* device, Clock* clock);
        VideoFrameBufferPool(const VideoFrameBufferPool&) = delete;
        VideoFrameBufferPool& operator=(const VideoFrameBufferPool&) = delete;

        virtual ~VideoFrameBufferPool();

        VideoFrameBuffer* Create(void* texture);
        bool Delete(const VideoFrameBuffer* buffer);
        bool Reserve(const VideoFrameBuffer* buffer);
        rtc::scoped_refptr<VideoFrameBuffer> Retain(const VideoFrameBuffer* buffer);
        VideoFrameBufferState GetState(const VideoFrameBuffer* buffer) const;

    private:
        IGraphicsDevice* device_;
        Clock* clock_;
        std::list<rtc::scoped_refptr<VideoFrameBuffer>> pool_;
        std::list<rtc::scoped_refptr<VideoFrameBuffer>> reservedPool_;
    };
}
}
