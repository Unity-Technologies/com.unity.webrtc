#pragma once

#include <shared_mutex>
#include "VideoFrame.h"

namespace unity {
namespace webrtc {

using namespace ::webrtc;

class VideoFrameAdapter : public VideoFrameBuffer
{
public:
    explicit VideoFrameAdapter(rtc::scoped_refptr<VideoFrame> frame)
        : frame_(std::move(frame))
        , size_(frame_->size())
        {}

    static ::webrtc::VideoFrame CreateVideoFrame(rtc::scoped_refptr<VideoFrame> frame);

    rtc::scoped_refptr<VideoFrame> GetVideoFrame() const { return frame_; }

    VideoFrameBuffer::Type type() const override;
    int width() const override { return size_.width(); }
    int height() const override { return size_.height(); }

    const I420BufferInterface* GetI420() const override;
    rtc::scoped_refptr<I420BufferInterface> ToI420() override;
protected:
    ~VideoFrameAdapter() override {};
private:
    rtc::scoped_refptr<I420BufferInterface> ConvertToVideoFrameBuffer(
        rtc::scoped_refptr<VideoFrame> video_frame) const;
    const rtc::scoped_refptr<VideoFrame> frame_;
    const Size size_;
};

// This class implements webrtc's VideoTrackSourceInterface. To pass frames down
// the webrtc video pipeline, each received a media::VideoFrame is converted to
// a webrtc::VideoFrame, taking any adaptation requested by downstream classes
// into account.
class UnityVideoTrackSource :
    public rtc::AdaptedVideoTrackSource
{
    public:
        //struct FrameAdaptationParams
        //{
        //    bool should_drop_frame;
        //    int crop_x;
        //    int crop_y;
        //    int crop_width;
        //    int crop_height;
        //    int scale_to_width;
        //    int scale_to_height;
        //};

    UnityVideoTrackSource(
        bool is_screencast,
        absl::optional<bool> needs_denoising);
    ~UnityVideoTrackSource() override;

    SourceState state() const override;

    bool remote() const override;
    bool is_screencast() const override;
    absl::optional<bool> needs_denoising() const override;
    void OnFrameCaptured(rtc::scoped_refptr<VideoFrame> frame);

    using ::webrtc::VideoTrackSourceInterface::AddOrUpdateSink;
    using ::webrtc::VideoTrackSourceInterface::RemoveSink;
    
    static rtc::scoped_refptr<UnityVideoTrackSource> Create(bool is_screencast,
                                                            absl::optional<bool> needs_denoising);

private:
    void SendFeedback();
    //FrameAdaptationParams ComputeAdaptationParams(int width,
    //                                            int height,
    //                                            int64_t time_us);

    // Delivers |frame| to base class method
    // rtc::AdaptedVideoTrackSource::OnFrame(). If the cropping (given via
    // |frame->visible_rect()|) has changed since the last delivered frame, the
    // whole frame is marked as updated.
    // void DeliverFrame(rtc::scoped_refptr<::webrtc::VideoFrame> frame,
    //                  gfx::Rect* update_rect,
    //                  int64_t timestamp_us);

    // |thread_checker_| is bound to the libjingle worker thread.
    // todo::(kazuki) change compiler vc to clang
    // media::VideoFramePool scaled_frame_pool_;

    // State for the timestamp translation.
    rtc::TimestampAligner timestamp_aligner_;

    const bool is_screencast_;
    const absl::optional<bool> needs_denoising_;
    std::shared_timed_mutex m_mutex;
};

} // end namespace webrtc
} // end namespace unity
