#include "pch.h"

#include <functional>

#include "VideoFrameScheduler.h"

namespace unity
{
namespace webrtc
{
    void VideoFrameScheduler::Start(std::function<void()> capture_callback) { }

    void VideoFrameScheduler::Pause(bool pause) {}

    void VideoFrameScheduler::OnFrameCaptured(const VideoFrame* frame) { }

    void VideoFrameScheduler::SetMaxFramerateFps(int max_framerate_fps) { }

}
}
