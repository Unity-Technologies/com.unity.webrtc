#include "pch.h"

#include "VideoFrameScheduler.h"

namespace unity
{
namespace webrtc
{
    void VideoFrameScheduler::Start(const base::RepeatingClosure& capture_callback) { }

    void VideoFrameScheduler::Pause(bool pause) {}

    void VideoFrameScheduler::OnFrameCaptured(const VideoFrame* frame) { }

    void VideoFrameScheduler::SetMaxFramerateFps(int max_framerate_fps) { }

}
}
