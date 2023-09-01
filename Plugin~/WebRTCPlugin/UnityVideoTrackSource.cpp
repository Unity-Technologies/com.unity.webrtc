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
        , frame_(nullptr)
        , syncApplicationFramerate_(true)
    {
        taskQueue_ = std::make_unique<rtc::TaskQueue>(
            taskQueueFactory->CreateTaskQueue("VideoFrameScheduler", TaskQueueFactory::Priority::NORMAL));
        scheduler_ = std::make_unique<VideoFrameScheduler>(taskQueue_->Get());
        scheduler_->Start(std::bind(&UnityVideoTrackSource::OnUpdateVideoFrame, this));
        if (syncApplicationFramerate_)
            scheduler_->Pause(true);
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

    void UnityVideoTrackSource::OnUpdateVideoFrame()
    {
        const std::unique_lock<std::mutex> lock(mutex_);
        CaptureVideoFrame();
    }

    void UnityVideoTrackSource::CaptureVideoFrame()
    {
        if (!frame_)
            return;

        const int orig_width = frame_->size().width();
        const int orig_height = frame_->size().height();
        const int64_t now_us = rtc::TimeMicros();
        FrameAdaptationParams frame_adaptation_params = ComputeAdaptationParams(orig_width, orig_height, now_us);
        if (frame_adaptation_params.should_drop_frame)
        {
            frame_ = nullptr;
            return;
        }

        const webrtc::TimeDelta timestamp = frame_->timestamp();
        rtc::scoped_refptr<VideoFrameAdapter> frame_adapter(
            new rtc::RefCountedObject<VideoFrameAdapter>(std::move(frame_)));

        ::webrtc::VideoFrame::Builder builder = ::webrtc::VideoFrame::Builder()
                                                    .set_video_frame_buffer(std::move(frame_adapter))
                                                    .set_timestamp_us(timestamp.us());
        OnFrame(builder.build());
    }

    void UnityVideoTrackSource::SendFeedback()
    {
        float maxFramerate = video_adapter()->GetMaxFramerate();
        if (maxFramerate == std::numeric_limits<float>::infinity())
            return;
        scheduler_->SetMaxFramerateFps(static_cast<int>(maxFramerate));
    }

    void UnityVideoTrackSource::OnFrameCaptured(rtc::scoped_refptr<VideoFrame> frame)
    {
        SendFeedback();

        const std::unique_lock<std::mutex> lock(mutex_);
        frame_ = frame;

        if (syncApplicationFramerate_)
            CaptureVideoFrame();
    }

    void UnityVideoTrackSource::SetSyncApplicationFramerate(bool value)
    {
        if (syncApplicationFramerate_ == value)
            return;

        scheduler_->Pause(value);
        syncApplicationFramerate_ = value;
    }

} // end namespace webrtc
} // end namespace unity
