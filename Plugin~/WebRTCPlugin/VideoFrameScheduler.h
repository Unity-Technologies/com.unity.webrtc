#pragma once

#include "VideoFrame.h"
#include "base/callback_forward.h"

namespace unity
{
namespace webrtc
{
    class VideoFrameScheduler
    {
    public:
        VideoFrameScheduler() = default;
        VideoFrameScheduler(const VideoFrameScheduler&) = delete;
        VideoFrameScheduler& operator=(const VideoFrameScheduler&) = delete;

        // Starts the scheduler. |capture_callback| will be called whenever a new
        // frame should be captured.
        virtual void Start(std::function<void()> capture_callback);

        // Pause and resumes the scheduler.
        virtual void Pause(bool pause);

        // Called after |frame| has been captured. |frame| may be set to nullptr
        // if the capture request failed.
        virtual void OnFrameCaptured(const VideoFrame* frame);

        // Called when WebRTC requests the VideoTrackSource to provide frames
        // at a maximum framerate.
        virtual void SetMaxFramerateFps(int max_framerate_fps);

    protected:
        virtual ~VideoFrameScheduler() = default;
    };
}
}
