#include "pch.h"

#include <functional>

#include "VideoFrameScheduler.h"

namespace unity
{
namespace webrtc
{
    VideoFrameScheduler::VideoFrameScheduler(TaskQueueFactory* taskQueueFactory, Clock* clock)
        : maxFramerate_(30)
        , tackQueueFactory_(taskQueueFactory)
        , lastCaptureStartedTime_(Timestamp::Zero())
        , clock_(clock)
    {
        taskQueue_ = std::make_unique<rtc::TaskQueue>(
            tackQueueFactory_->CreateTaskQueue("VideoFrameScheduler", TaskQueueFactory::Priority::NORMAL));
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
            taskQueue_->Get(),
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
        taskQueue_->PostTask([this] { task_.Stop(); });
    }
}
}
