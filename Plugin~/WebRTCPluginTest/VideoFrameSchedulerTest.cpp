#include "pch.h"

#include "VideoFrameScheduler.h"
#include "api/task_queue/default_task_queue_factory.h"

namespace unity
{
namespace webrtc
{
    class VideoFrameSchedulerTest : public ::testing::Test
    {
    public:
        VideoFrameSchedulerTest()
            : count_(0)
            , clock_(0)
        {
        }
        ~VideoFrameSchedulerTest() override = default;
        void InitScheduler()
        {
            taskQueueFactory_ = CreateDefaultTaskQueueFactory();
            scheduler_ = std::make_unique<VideoFrameScheduler>(taskQueueFactory_.get(), &clock_);
            scheduler_->SetMaxFramerateFps(kMaxFramerate);
            scheduler_->Start(std::bind(&VideoFrameSchedulerTest::CaptureCallback, this));
        }

        void CaptureCallback()
        {
            count_++;
        }

    protected:
        const int kMaxFramerate = 30;
        const TimeDelta kTimeDelta = TimeDelta::Seconds(1) / kMaxFramerate;
        int count_ = 0;
        SimulatedClock clock_;
        std::unique_ptr<TaskQueueFactory> taskQueueFactory_;
        std::unique_ptr<VideoFrameScheduler> scheduler_;
    };

    TEST_F(VideoFrameSchedulerTest, Run)
    {
        InitScheduler();
        EXPECT_EQ(0, count_);

        rtc::Thread::SleepMs(static_cast<int>(kTimeDelta.ms()) + 1);
        EXPECT_EQ(1, count_);
    }

    TEST_F(VideoFrameSchedulerTest, Pause)
    {
        InitScheduler();
        EXPECT_EQ(0, count_);

        scheduler_->Pause(true);
        rtc::Thread::SleepMs(static_cast<int>(kTimeDelta.ms()) + 1);
        EXPECT_EQ(0, count_);

        scheduler_->Pause(false);
        rtc::Thread::SleepMs(static_cast<int>(kTimeDelta.ms()) + 1);
        EXPECT_EQ(1, count_);
    }

    TEST_F(VideoFrameSchedulerTest, SetMaxFramerateFps)
    {
        InitScheduler();
        EXPECT_EQ(0, count_);

        const int maxFramerate = 5;
        scheduler_->SetMaxFramerateFps(maxFramerate);
        rtc::Thread::SleepMs(static_cast<int>(kTimeDelta.ms()) + 1);
        EXPECT_EQ(1, count_);

        const TimeDelta timeDelta = TimeDelta::Seconds(1) / maxFramerate;
        rtc::Thread::SleepMs(static_cast<int>(timeDelta.ms()) + 1);
        EXPECT_EQ(2, count_);

    }
}
}
