#pragma once

#include <rtc_base/task_utils/repeating_task.h>

#include "VideoFrame.h"

namespace unity
{
namespace webrtc
{
    class VideoFrameScheduler
    {
    public:
        VideoFrameScheduler(TaskQueueBase* queue, Clock* clock = Clock::GetRealTimeClock());
        VideoFrameScheduler(const VideoFrameScheduler&) = delete;
        VideoFrameScheduler& operator=(const VideoFrameScheduler&) = delete;

        virtual ~VideoFrameScheduler();

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
        virtual void SetMaxFramerateFps(int maxFramerate);

    private:
        absl::optional<TimeDelta> ScheduleNextFrame();
        void CaptureNextFrame();
        void StartRepeatingTask();
        void StopTask();

        std::function<void()> callback_;
        bool paused_ = false;
        int maxFramerate_;
        RepeatingTaskHandle task_;
        TaskQueueBase* queue_;
        Timestamp lastCaptureStartedTime_;
        Clock* clock_;
    };
}
}
