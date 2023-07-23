#include "pch.h"

#include "UnityVideoTrackSource.h"
#include "VideoFrameAdapter.h"
#include "VideoFrameScheduler.h"

namespace unity
{
namespace webrtc
{

    rtc::scoped_refptr<UnityVideoTrackSource> UnityVideoTrackSource::Create(
        bool is_screencast, absl::optional<bool> needs_denoising, TaskQueueFactory* taskQueueFactory)
    {
        return rtc::make_ref_counted<UnityVideoTrackSource>(is_screencast, needs_denoising, taskQueueFactory);
    }

    UnityVideoTrackSource::UnityVideoTrackSource(
        bool is_screencast, absl::optional<bool> needs_denoising, TaskQueueFactory* taskQueueFactory)
        : AdaptedVideoTrackSource(/*required_alignment=*/1)
        , is_screencast_(is_screencast)
        , buffer_(nullptr)
        , timestamp_(Timestamp::Zero())
    {
        taskQueue_ = std::make_unique<rtc::TaskQueue>(
            taskQueueFactory->CreateTaskQueue("VideoFrameScheduler", TaskQueueFactory::Priority::NORMAL));
        scheduler_ = std::make_unique<VideoFrameScheduler>(taskQueue_->Get());
        scheduler_->Start(std::bind(&UnityVideoTrackSource::CaptureNextFrame, this));
    }

    UnityVideoTrackSource::~UnityVideoTrackSource() { scheduler_ = nullptr; }

    UnityVideoTrackSource::FrameAdaptationParams
    UnityVideoTrackSource::ComputeAdaptationParams(int width, int height, int64_t time_us)
    {
        FrameAdaptationParams result { false, 0, 0, 0, 0, 0, 0 };
        result.should_drop_frame = !AdaptFrame(
            width,
            height,
            time_us,
            &result.scale_to_width,
            &result.scale_to_height,
            &result.crop_width,
            &result.crop_height,
            &result.crop_x,
            &result.crop_y);
        return result;
    }

    UnityVideoTrackSource::SourceState UnityVideoTrackSource::state() const { return kLive; }

    bool UnityVideoTrackSource::remote() const { return false; }

    bool UnityVideoTrackSource::is_screencast() const { return is_screencast_; }

    absl::optional<bool> UnityVideoTrackSource::needs_denoising() const { return needs_denoising_; }

    void UnityVideoTrackSource::CaptureNextFrame()
    {
        const std::unique_lock<std::mutex> lock(mutex_);
        if (!buffer_)
            return;

        const int orig_width = buffer_->width();
        const int orig_height = buffer_->height();
        const int64_t now_us = rtc::TimeMicros();
        FrameAdaptationParams frame_adaptation_params = ComputeAdaptationParams(orig_width, orig_height, now_us);
        if (frame_adaptation_params.should_drop_frame)
        {
            buffer_ = nullptr;
            return;
        }

        ::webrtc::VideoFrame::Builder builder = ::webrtc::VideoFrame::Builder()
                                                    .set_video_frame_buffer(std::move(buffer_))
                                                    .set_timestamp_us(timestamp_.us());
        OnFrame(builder.build());
    }

    void UnityVideoTrackSource::SendFeedback()
    {
        float maxFramerate = video_adapter()->GetMaxFramerate();
        if (maxFramerate == std::numeric_limits<float>::infinity())
            return;
        scheduler_->SetMaxFramerateFps(static_cast<int>(maxFramerate));
    }

    void
    UnityVideoTrackSource::OnFrameCaptured(rtc::scoped_refptr<VideoFrameBuffer> buffer, webrtc::Timestamp timestamp)
    {
        SendFeedback();

        const std::unique_lock<std::mutex> lock(mutex_);
        buffer_ = buffer;
        timestamp_ = timestamp;
    }

} // end namespace webrtc
} // end namespace unity
