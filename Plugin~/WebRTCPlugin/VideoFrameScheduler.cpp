#include "pch.h"

#include <functional>
#include <rtc_base/event.h>

#include "VideoFrameScheduler.h"

namespace unity
{
namespace webrtc
{
    constexpr TimeDelta kTimeout = TimeDelta::Millis(1000);

    VideoFrameScheduler::VideoFrameScheduler(TaskQueueBase* queue, Clock* clock)
        : maxFramerate_(30)
        , queue_(queue)
        , lastCaptureStartedTime_(Timestamp::Zero())
        , clock_(clock)
    {
    }

    VideoFrameScheduler::~VideoFrameScheduler()
    {
        rtc::Event done;

        // Waiting for stopping task.
        queue_->PostTask([task = std::move(task_), &done]() mutable {
            task.Stop();
            done.Set();
        });
        done.Wait(kTimeout);
    }

    void VideoFrameScheduler::Start(std::function<void()> callback)
    {
        callback_ = callback;
        lastCaptureStartedTime_ = clock_->CurrentTime();
        StartRepeatingTask();
    }

    void VideoFrameScheduler::Pause(bool pause)
    {
        paused_ = pause;
        if (!paused_)
        {
            StartRepeatingTask();
        }
    }

    void VideoFrameScheduler::OnFrameCaptured(const VideoFrame* frame) { }

    void VideoFrameScheduler::SetMaxFramerateFps(int maxFramerate) { maxFramerate_ = maxFramerate; }

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

        task_ = RepeatingTaskHandle::DelayedStart(queue_, firstDelay.value(), [this]() {
            if (paused_)
            {
                task_.Stop();
                return TimeDelta::PlusInfinity();
            }
            CaptureNextFrame();
            auto delay = ScheduleNextFrame();
            if (delay.has_value())
                return delay.value();
            return TimeDelta::PlusInfinity();
        });
    }
}
}
