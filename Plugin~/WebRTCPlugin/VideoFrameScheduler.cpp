#include "pch.h"

#include <functional>

#include "VideoFrameScheduler.h"

namespace unity
{
namespace webrtc
{
    VideoFrameScheduler::VideoFrameScheduler(TaskQueueBase* queue, Clock* clock)
        : maxFramerate_(30)
        , queue_(queue)
        , lastCaptureStartedTime_(Timestamp::Zero())
        , clock_(clock)
    {
    }

    void VideoFrameScheduler::Start(std::function<void()> callback)
    {
        callback_ = callback;
        StartRepeatingTask();
    }

    void VideoFrameScheduler::Pause(bool pause)
    {
        paused_ = pause;
        if (paused_)
        {
            StopTask();
        }
        else
        {
            StartRepeatingTask();
        }
    }

    void VideoFrameScheduler::OnFrameCaptured(const VideoFrame* frame) { }

    void VideoFrameScheduler::SetMaxFramerateFps(int maxFramerate)
    {
        maxFramerate_ = maxFramerate;
    }

    absl::optional<TimeDelta> VideoFrameScheduler::ScheduleNextFrame()
    {
        if (paused_)
        {
            return absl::nullopt;
        }

        if (!callback_)
        {
            return absl::nullopt;
        }

        if (maxFramerate_ == 0)
        {
            return absl::nullopt;
        }

        Timestamp now = clock_->CurrentTime();
        TimeDelta interval = std::max(TimeDelta::Seconds(1) / maxFramerate_, TimeDelta::Millis(1));
        Timestamp target_capture_time = std::max(lastCaptureStartedTime_ + interval, now);
        return target_capture_time - now;
    }

    void VideoFrameScheduler::CaptureNextFrame()
    {
        lastCaptureStartedTime_ = clock_->CurrentTime();
        callback_();
    }

    void VideoFrameScheduler::StartRepeatingTask()
    {
        RTC_DCHECK(!paused_);
        RTC_DCHECK(!task_.Running());

        auto firstDelay = ScheduleNextFrame();
        RTC_DCHECK(firstDelay);

        task_ = RepeatingTaskHandle::DelayedStart(
            queue_,
            firstDelay.value(),
            [this]()
            {
                CaptureNextFrame();                
                auto delay = ScheduleNextFrame();
                if (delay.has_value())
                    return delay.value();
                return TimeDelta::PlusInfinity();
            });
    }

    void VideoFrameScheduler::StopTask()
    {
        RTC_DCHECK(task_.Running());
        task_.Stop();
    }
}
}
