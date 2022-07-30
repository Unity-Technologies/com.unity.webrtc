#pragma once

#include <api/video/video_frame.h>
#include <vector>

#include "VideoFrame.h"

namespace unity
{
namespace webrtc
{

    using namespace ::webrtc;

    class ScalableBufferInterface : public VideoFrameBuffer
    {
    public:
        virtual bool scaled() const = 0;

    protected:
        ~ScalableBufferInterface() override { }
    };

    class VideoFrameAdapter : public ScalableBufferInterface
    {
    public:
        class ScaledBuffer : public ScalableBufferInterface
        {
        public:
            ScaledBuffer(rtc::scoped_refptr<VideoFrameAdapter> parent, int width, int height);
            ~ScaledBuffer() override;

            VideoFrameBuffer::Type type() const override;
            int width() const override { return width_; }
            int height() const override { return height_; }
            bool scaled() const final { return true; }

            rtc::scoped_refptr<webrtc::I420BufferInterface> ToI420() override;
            const webrtc::I420BufferInterface* GetI420() const override;

            rtc::scoped_refptr<webrtc::VideoFrameBuffer>
            GetMappedFrameBuffer(rtc::ArrayView<webrtc::VideoFrameBuffer::Type> types) override;

            rtc::scoped_refptr<VideoFrame> GetVideoFrame() const { return parent_->frame_; }

            rtc::scoped_refptr<webrtc::VideoFrameBuffer> CropAndScale(
                int offset_x, int offset_y, int crop_width, int crop_height, int scaled_width, int scaled_height)
                override;

        private:
            const rtc::scoped_refptr<VideoFrameAdapter> parent_;
            const int width_;
            const int height_;
        };

        explicit VideoFrameAdapter(rtc::scoped_refptr<VideoFrame> frame);

        static ::webrtc::VideoFrame CreateVideoFrame(rtc::scoped_refptr<VideoFrame> frame);

        rtc::scoped_refptr<VideoFrame> GetVideoFrame() const { return frame_; }

        VideoFrameBuffer::Type type() const override;
        int width() const override { return size_.width(); }
        int height() const override { return size_.height(); }
        bool scaled() const override { return false; }

        const I420BufferInterface* GetI420() const override;
        rtc::scoped_refptr<I420BufferInterface> ToI420() override;
        rtc::scoped_refptr<webrtc::VideoFrameBuffer> CropAndScale(
            int offset_x, int offset_y, int crop_width, int crop_height, int scaled_width, int scaled_height) override;

    protected:
        ~VideoFrameAdapter() override { }

    private:
        rtc::scoped_refptr<webrtc::VideoFrameBuffer> GetOrCreateFrameBufferForSize(const Size& size);
        rtc::scoped_refptr<I420BufferInterface>
        ConvertToVideoFrameBuffer(rtc::scoped_refptr<VideoFrame> video_frame) const;
        // todo(kazuki):
        // Need this buffer because the type() method returns kI420.
        mutable rtc::scoped_refptr<I420BufferInterface> i420Buffer_;
        std::vector<rtc::scoped_refptr<VideoFrameBuffer>> scaledI40Buffers_;
        const rtc::scoped_refptr<VideoFrame> frame_;
        const Size size_;
        mutable std::mutex scaleLock_;
        mutable std::mutex convertLock_;
    };
}
}
