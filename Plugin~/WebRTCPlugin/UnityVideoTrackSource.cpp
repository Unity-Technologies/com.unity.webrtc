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
    return new rtc::RefCountedObject<UnityVideoTrackSource>(is_screencast, needs_denoising, taskQueueFactory);
}

::webrtc::VideoFrame BlackFrame(int width, int height)
{
    rtc::scoped_refptr<webrtc::I420Buffer> buffer = webrtc::I420Buffer::Create(width, height);
    webrtc::I420Buffer::SetBlack(buffer.get());
    return ::webrtc::VideoFrame::Builder().set_video_frame_buffer(buffer).build();
}

UnityVideoTrackSource::UnityVideoTrackSource(
    bool is_screencast,
    absl::optional<bool> needs_denoising, TaskQueueFactory* taskQueueFactory)
    : AdaptedVideoTrackSource(/*required_alignment=*/1)
    , is_screencast_(is_screencast)
    , videoFrame_(BlackFrame(128, 128))
{
    taskQueue_ = std::make_unique<rtc::TaskQueue>(
        taskQueueFactory->CreateTaskQueue("VideoFrameScheduler", TaskQueueFactory::Priority::NORMAL));
    scheduler_ = std::make_unique<VideoFrameScheduler>(taskQueue_->Get());
    scheduler_->Start(std::bind(&UnityVideoTrackSource::CaptureNextFrame, this));
}

UnityVideoTrackSource::~UnityVideoTrackSource()
{
    scheduler_ = nullptr;
}

UnityVideoTrackSource::SourceState UnityVideoTrackSource::state() const
{
    // TODO(nisse): What's supposed to change this state?
    return MediaSourceInterface::SourceState::kLive;
}

bool UnityVideoTrackSource::remote() const
{
    return false;
}

bool UnityVideoTrackSource::is_screencast() const
{
    return is_screencast_;
}

absl::optional<bool> UnityVideoTrackSource::needs_denoising() const
{
    return needs_denoising_;
}

void UnityVideoTrackSource::CaptureNextFrame()
{
    const std::unique_lock<std::mutex> lock(mutex_);
    OnFrame(videoFrame_);
}

void UnityVideoTrackSource::SendFeedback()
{
    int maxFramerate = static_cast<int>(video_adapter()->GetMaxFramerate());
    scheduler_->SetMaxFramerateFps(maxFramerate);
}

void UnityVideoTrackSource::OnFrameCaptured(
    rtc::scoped_refptr<VideoFrame> frame)
{
    SendFeedback();

    const int64_t now_us = rtc::TimeMicros();
    const int64_t translated_camera_time_us =
        timestamp_aligner_.TranslateTimestamp(frame->timestamp().us(),
            now_us);

    rtc::scoped_refptr<VideoFrameAdapter> frame_adapter(
        new rtc::RefCountedObject<VideoFrameAdapter>(std::move(frame)));

    ::webrtc::VideoFrame::Builder builder =
        ::webrtc::VideoFrame::Builder()
        .set_video_frame_buffer(frame_adapter)
        .set_timestamp_us(translated_camera_time_us);

    const std::unique_lock<std::mutex> lock(mutex_);
    videoFrame_ = builder.build();
}

} // end namespace webrtc
} // end namespace unity
