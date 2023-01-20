#include "pch.h"

#include "GraphicsDevice/IGraphicsDevice.h"
#include "NativeFrameBuffer.h"

namespace unity
{
namespace webrtc
{
    const I420BufferInterface* NativeFrameBuffer::GetI420() const
    {
        return ConvertToVideoFrameBuffer(frame_)->GetI420();
    }

    rtc::scoped_refptr<I420BufferInterface> NativeFrameBuffer::ToI420()
    {
        return ConvertToVideoFrameBuffer(frame_)->ToI420();
    }

    rtc::scoped_refptr<VideoFrameBuffer> NativeFrameBuffer::GetOrCreateFrameBufferForSize(const Size& size)
    {
        std::unique_lock<std::mutex> guard(scaleLock_);

        for (auto scaledI420buffer : scaledI40Buffers_)
        {
            Size bufferSize(scaledI420buffer->width(), scaledI420buffer->height());
            if (size == bufferSize)
            {
                return scaledI420buffer;
            }
        }
        auto buffer = VideoFrameBuffer::CropAndScale(0, 0, width(), height(), size.width(), size.height());
        scaledI40Buffers_.push_back(buffer);
        return buffer;
    }

    rtc::scoped_refptr<I420BufferInterface>
    NativeFrameBuffer::ConvertToVideoFrameBuffer(rtc::scoped_refptr<VideoFrame> video_frame) const
    {
        std::unique_lock<std::mutex> guard(convertLock_);
        if (i420Buffer_)
            return i420Buffer_;

        RTC_DCHECK(video_frame);
        RTC_DCHECK(video_frame->HasGpuMemoryBuffer());

        auto gmb = video_frame->GetGpuMemoryBuffer();
        i420Buffer_ = gmb->ToI420();
        return i420Buffer_;
    }
}
}
