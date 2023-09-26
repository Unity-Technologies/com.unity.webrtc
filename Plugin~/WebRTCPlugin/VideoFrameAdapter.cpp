#include "pch.h"

#include <api/video/video_frame.h>

#include "VideoFrameAdapter.h"

namespace unity
{
namespace webrtc
{
    template<typename T>
    bool Contains(rtc::ArrayView<T> arr, T value)
    {
        for (auto e : arr)
        {
            if (e == value)
                return true;
        }
        return false;
    }

    ::webrtc::VideoFrame VideoFrameAdapter::CreateVideoFrame(rtc::scoped_refptr<VideoFrame> frame)
    {
        rtc::scoped_refptr<VideoFrameAdapter> adapter(new rtc::RefCountedObject<VideoFrameAdapter>(std::move(frame)));

        return ::webrtc::VideoFrame::Builder().set_video_frame_buffer(adapter).build();
    }

    VideoFrameAdapter::ScaledBuffer::ScaledBuffer(rtc::scoped_refptr<VideoFrameAdapter> parent, int width, int height)
        : parent_(parent)
        , width_(width)
        , height_(height)
    {
    }

    VideoFrameAdapter::ScaledBuffer::~ScaledBuffer() { }

    VideoFrameBuffer::Type VideoFrameAdapter::ScaledBuffer::type() const { return parent_->type(); }

    rtc::scoped_refptr<webrtc::I420BufferInterface> VideoFrameAdapter::ScaledBuffer::ToI420()
    {
        return parent_->GetOrCreateFrameBufferForSize(Size(width_, height_))->ToI420();
    }

    const I420BufferInterface* VideoFrameAdapter::ScaledBuffer::GetI420() const
    {
        return parent_->GetOrCreateFrameBufferForSize(Size(width_, height_))->GetI420();
    }

    rtc::scoped_refptr<VideoFrameBuffer>
    VideoFrameAdapter::ScaledBuffer::GetMappedFrameBuffer(rtc::ArrayView<VideoFrameBuffer::Type> types)
    {
        auto buffer = parent_->GetOrCreateFrameBufferForSize(Size(width_, height_));
        return Contains(types, buffer->type()) ? buffer : nullptr;
    }

    rtc::scoped_refptr<VideoFrameBuffer> VideoFrameAdapter::ScaledBuffer::CropAndScale(
        int offset_x, int offset_y, int crop_width, int crop_height, int scaled_width, int scaled_height)
    {
        return rtc::make_ref_counted<ScaledBuffer>(
            rtc::scoped_refptr<VideoFrameAdapter>(parent_), scaled_width, scaled_height);
    }

    VideoFrameAdapter::VideoFrameAdapter(rtc::scoped_refptr<VideoFrame> frame)
        : frame_(std::move(frame))
        , size_(frame_->size())
    {
    }

    VideoFrameBuffer::Type VideoFrameAdapter::type() const
    {
#if UNITY_IOS || UNITY_OSX || UNITY_ANDROID
        // todo(kazuki): support for kNative type for mobile platform and macOS.
        // Need to pass ObjCFrameBuffer instead of VideoFrameAdapter on macOS/iOS.
        // Need to pass AndroidVideoBuffer instead of VideoFrameAdapter on Android.
        return VideoFrameBuffer::Type::kI420;
#else
        return VideoFrameBuffer::Type::kNative;
#endif
    }

    const I420BufferInterface* VideoFrameAdapter::GetI420() const
    {
        return ConvertToVideoFrameBuffer(frame_)->GetI420();
    }

    rtc::scoped_refptr<I420BufferInterface> VideoFrameAdapter::ToI420()
    {
        return ConvertToVideoFrameBuffer(frame_)->ToI420();
    }

    rtc::scoped_refptr<VideoFrameBuffer> VideoFrameAdapter::CropAndScale(
        int offset_x, int offset_y, int crop_width, int crop_height, int scaled_width, int scaled_height)
    {
        return rtc::make_ref_counted<ScaledBuffer>(
            rtc::scoped_refptr<VideoFrameAdapter>(this), scaled_width, scaled_height);
    }

    rtc::scoped_refptr<VideoFrameBuffer> VideoFrameAdapter::GetOrCreateFrameBufferForSize(const Size& size)
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
    VideoFrameAdapter::ConvertToVideoFrameBuffer(rtc::scoped_refptr<VideoFrame> video_frame) const
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
