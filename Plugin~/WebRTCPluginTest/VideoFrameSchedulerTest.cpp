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

        void Delete() override {}

        void PostTask(std::unique_ptr<QueuedTask> task) override
        {
            last_task_ = std::move(task);
            last_delay_ = 0;
        }

        void PostDelayedTask(std::unique_ptr<QueuedTask> task, uint32_t milliseconds) override
        {
            last_task_ = std::move(task);
            last_delay_ = milliseconds;
        }

        bool AdvanceTimeAndRunLastTask()
        {
            EXPECT_TRUE(last_task_);
            EXPECT_TRUE(last_delay_);
            clock_->AdvanceTimeMilliseconds(last_delay_.value_or(0));
            last_delay_.reset();
            auto task = std::move(last_task_);
            bool delete_task = task->Run();
            if (!delete_task)
            {
                // If the task should not be deleted then just release it.
                task.release();
            }
            return delete_task;
        }

        bool IsTaskQueued() { return !!last_task_; }

        uint32_t last_delay() const
        {
            EXPECT_TRUE(last_delay_.has_value());
            return last_delay_.value_or(-1);
        }

    private:
        CurrentTaskQueueSetter task_queue_setter_;
        SimulatedClock* clock_;
        std::unique_ptr<QueuedTask> last_task_;
        absl::optional<uint32_t> last_delay_;
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

        void CaptureCallback()
        {
            count_++;
        }

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
        uint32_t delay = queue.last_delay();
        EXPECT_GT(delay, 0u);
        EXPECT_FALSE(queue.AdvanceTimeAndRunLastTask());
        EXPECT_EQ(1, count_);

        EXPECT_GT(queue.last_delay(), delay);
        EXPECT_FALSE(queue.AdvanceTimeAndRunLastTask());
        EXPECT_EQ(2, count_);

        scheduler_ = nullptr;
    }
}
}
