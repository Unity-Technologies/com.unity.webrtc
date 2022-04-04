#include "pch.h"

#include "UnityVideoTrackSource.h"
#include "VideoFrameAdapter.h"

namespace unity
{
namespace webrtc
{

rtc::scoped_refptr<UnityVideoTrackSource> UnityVideoTrackSource::Create(bool is_screencast, absl::optional<bool> needs_denoising)
{
    return new rtc::RefCountedObject<UnityVideoTrackSource>(is_screencast, needs_denoising);
}

UnityVideoTrackSource::UnityVideoTrackSource(
    bool is_screencast,
    absl::optional<bool> needs_denoising)
    : AdaptedVideoTrackSource(/*required_alignment=*/1)
    , is_screencast_(is_screencast)
    , needs_denoising_(needs_denoising)
{
}

UnityVideoTrackSource::~UnityVideoTrackSource()
{
    {
        std::unique_lock<std::shared_timed_mutex> lock(m_mutex);
    }
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

void UnityVideoTrackSource::SendFeedback()
{
    //float maxFramerate = video_adapter()->GetMaxFramerate();
    // todo(kazuki):
}

void UnityVideoTrackSource::OnFrameCaptured(
    rtc::scoped_refptr<VideoFrame> frame)
{
    const std::unique_lock<std::shared_timed_mutex> lock(m_mutex, std::try_to_lock);
    if (!lock)
    {
        // currently encoding
        return;
    }

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

    OnFrame(builder.build());
}

} // end namespace webrtc
} // end namespace unity
