#include "pch.h"

#include "Context.h"
#include "GpuMemoryBuffer.h"
#include "GraphicsDevice/IGraphicsDevice.h"
#include "GraphicsDevice/ITexture2D.h"
#include "GraphicsDeviceTestBase.h"
#include "UnityVideoTrackSource.h"
#include "VideoFrameUtil.h"
#include <api/task_queue/default_task_queue_factory.h>

using testing::_;
using testing::Invoke;
using testing::Mock;

namespace unity
{
namespace webrtc
{
    constexpr TimeDelta kTimeout = TimeDelta::Millis(1000);

    class MockVideoSink : public rtc::VideoSinkInterface<::webrtc::VideoFrame>
    {
    public:
        ~MockVideoSink() override = default;
        MOCK_METHOD(void, OnFrame, (const ::webrtc::VideoFrame&), (override));
    };

    const int kWidth = 1280;
    const int kHeight = 720;

    class VideoTrackSourceTest : public GraphicsDeviceTestBase
    {
    public:
        VideoTrackSourceTest()
            : m_texture(nullptr)
            , m_taskQueueFactory(CreateDefaultTaskQueueFactory())
        {
            m_trackSource = UnityVideoTrackSource::Create(false, absl::nullopt, m_taskQueueFactory.get());
            m_trackSource->AddOrUpdateSink(&sink_, rtc::VideoSinkWants());
        }

        ~VideoTrackSourceTest() override { m_trackSource->RemoveSink(&sink_); }

    protected:
        void SetUp() override
        {
            if (!device())
                GTEST_SKIP() << "The graphics driver is not installed on the device.";

            m_texture.reset(device()->CreateDefaultTextureV(kWidth, kHeight, format()));
            if (!m_texture)
                GTEST_SKIP() << "The graphics driver cannot create a texture resource.";

            ContextDependencies dependencies;
            dependencies.device = device();
            context = std::make_unique<Context>(dependencies);
        }

        std::unique_ptr<Context> context;
        std::unique_ptr<ITexture2D> m_texture;
        std::unique_ptr<TaskQueueFactory> m_taskQueueFactory;

        MockVideoSink sink_;
        rtc::scoped_refptr<UnityVideoTrackSource> m_trackSource;

        ::webrtc::VideoFrame::Builder CreateBlackFrameBuilder(int width, int height)
        {
            rtc::scoped_refptr<webrtc::I420Buffer> buffer = webrtc::I420Buffer::Create(width, height);

            webrtc::I420Buffer::SetBlack(buffer.get());
            return ::webrtc::VideoFrame::Builder().set_video_frame_buffer(buffer);
        }

        void SendTestFrame()
        {
            auto frame = CreateTestFrame(device(), m_texture.get(), format());
            m_trackSource->OnFrameCaptured(std::move(frame));
        }
    };

    TEST_P(VideoTrackSourceTest, OnFrameCaptured)
    {
        rtc::Event done;
        EXPECT_CALL(sink_, OnFrame(_)).WillOnce(Invoke([&done](const ::webrtc::VideoFrame& frame) { done.Set(); }));
        SendTestFrame();
        EXPECT_TRUE(done.Wait(kTimeout));
    }

    INSTANTIATE_TEST_SUITE_P(GfxDeviceAndColorSpece, VideoTrackSourceTest, testing::ValuesIn(VALUES_TEST_ENV));

} // end namespace webrtc
} // end namespace unity
