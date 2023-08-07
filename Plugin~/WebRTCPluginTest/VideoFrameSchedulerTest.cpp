#include "pch.h"

#include "VideoFrameScheduler.h"
#include <api/task_queue/default_task_queue_factory.h>

namespace unity
{
namespace webrtc
{
    class FakeTaskQueue : public TaskQueueBase
    {
    public:
        explicit FakeTaskQueue(SimulatedClock* clock)
            : task_queue_setter_(this)
            , clock_(clock)
        {
        }

        void Delete() override { }

        void PostTaskImpl(
            absl::AnyInvocable<void() &&> task, const PostTaskTraits& traits, const Location& location) override
        {
            last_task_ = std::move(task);
            last_precision_ = absl::nullopt;
            last_delay_ = TimeDelta::Zero();
        }

        void PostDelayedTaskImpl(
            absl::AnyInvocable<void() &&> task,
            TimeDelta delay,
            const PostDelayedTaskTraits& traits,
            const Location& location) override
        {
            last_task_ = std::move(task);
            last_precision_ = TaskQueueBase::DelayPrecision::kLow;
            last_delay_ = delay;
        }

        bool AdvanceTimeAndRunLastTask()
        {
            EXPECT_TRUE(last_task_);
            EXPECT_TRUE(last_delay_.IsFinite());
            clock_->AdvanceTime(last_delay_);
            last_delay_ = TimeDelta::MinusInfinity();
            auto task = std::move(last_task_);
            std::move(task)();
            return last_task_ == nullptr;
        }

        bool IsTaskQueued() { return !!last_task_; }

        TimeDelta last_delay() const
        {
            EXPECT_TRUE(last_delay_.IsFinite());
            return last_delay_;
        }

    private:
        CurrentTaskQueueSetter task_queue_setter_;
        SimulatedClock* clock_;
        absl::AnyInvocable<void() &&> last_task_;
        TimeDelta last_delay_ = TimeDelta::MinusInfinity();
        absl::optional<TaskQueueBase::DelayPrecision> last_precision_;
    };

    class VideoFrameSchedulerTest : public ::testing::Test
    {
    public:
        VideoFrameSchedulerTest()
            : count_(0)
            , clock_(0)
        {
        }
        ~VideoFrameSchedulerTest() override = default;

        void InitScheduler(FakeTaskQueue& queue)
        {
            scheduler_ = std::make_unique<VideoFrameScheduler>(&queue, &clock_);
            scheduler_->SetMaxFramerateFps(kMaxFramerate);
            scheduler_->Start(std::bind(&VideoFrameSchedulerTest::CaptureCallback, this));
        }

        void CaptureCallback() { count_++; }

    protected:
        const int kMaxFramerate = 30;
        const TimeDelta kTimeDelta = TimeDelta::Seconds(1) / kMaxFramerate;
        int count_ = 0;
        SimulatedClock clock_;
        std::unique_ptr<VideoFrameScheduler> scheduler_;
    };

    TEST_F(VideoFrameSchedulerTest, Run)
    {
        FakeTaskQueue queue(&clock_);
        InitScheduler(queue);
        EXPECT_EQ(0, count_);
        EXPECT_FALSE(queue.AdvanceTimeAndRunLastTask());
        EXPECT_EQ(1, count_);

        scheduler_ = nullptr;
    }

    TEST_F(VideoFrameSchedulerTest, Pause)
    {
        FakeTaskQueue queue(&clock_);
        InitScheduler(queue);
        EXPECT_EQ(0, count_);

        scheduler_->Pause(true);
        EXPECT_TRUE(queue.AdvanceTimeAndRunLastTask());
        EXPECT_EQ(0, count_);

        scheduler_->Pause(false);
        EXPECT_FALSE(queue.AdvanceTimeAndRunLastTask());
        EXPECT_EQ(1, count_);

        scheduler_ = nullptr;
    }

    TEST_F(VideoFrameSchedulerTest, SetMaxFramerateFps)
    {
        FakeTaskQueue queue(&clock_);
        InitScheduler(queue);
        EXPECT_EQ(0, count_);

        const int maxFramerate = 5;
        scheduler_->SetMaxFramerateFps(maxFramerate);
        EXPECT_GT(queue.last_delay(), TimeDelta::Zero());
        EXPECT_FALSE(queue.AdvanceTimeAndRunLastTask());
        EXPECT_EQ(1, count_);

        EXPECT_GT(queue.last_delay(), TimeDelta::Zero());
        EXPECT_FALSE(queue.AdvanceTimeAndRunLastTask());
        EXPECT_EQ(2, count_);

        scheduler_ = nullptr;
    }
}
}
