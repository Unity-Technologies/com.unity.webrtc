#include "pch.h"

#include "Context.h"
#include "GraphicsDevice/IGraphicsDevice.h"
#include "GraphicsDevice/ITexture2D.h"
#include "GraphicsDeviceTestBase.h"
#include "UnityVideoRenderer.h"
#include "UnityVideoTrackSource.h"
#include <api/task_queue/default_task_queue_factory.h>

using testing::_;
using testing::Invoke;
using testing::Mock;

namespace unity
{
namespace webrtc
{
    const int kWidth = 256;
    const int kHeight = 256;

    class VideoRendererTest : public GraphicsDeviceTestBase
    {
    public:
        VideoRendererTest()
            : m_taskQueueFactory(CreateDefaultTaskQueueFactory())
        {
            m_callback = &OnFrameSizeChange;
            m_renderer = std::make_unique<UnityVideoRenderer>(1, m_callback, true);
        }
        ~VideoRendererTest() override = default;

    protected:
        void SetUp() override
        {
            if (!device())
                GTEST_SKIP() << "The graphics driver is not installed on the device.";
            m_texture.reset(device()->CreateDefaultTextureV(kWidth, kHeight, format()));
            EXPECT_TRUE(device()->WaitIdleForTest());

            ContextDependencies dependencies;
            dependencies.device = device();
            context = std::make_unique<Context>(dependencies);
        }
        std::unique_ptr<Context> context;
        std::unique_ptr<ITexture2D> m_texture;

        std::unique_ptr<TaskQueueFactory> m_taskQueueFactory;
        std::unique_ptr<UnityVideoRenderer> m_renderer;
        DelegateVideoFrameResize m_callback;

        ::webrtc::VideoFrame::Builder CreateBlackFrameBuilder(int width, int height)
        {
            rtc::scoped_refptr<webrtc::I420Buffer> buffer = webrtc::I420Buffer::Create(width, height);

            webrtc::I420Buffer::SetBlack(buffer.get());
            return ::webrtc::VideoFrame::Builder().set_video_frame_buffer(buffer).set_timestamp_us(
                Clock::GetRealTimeClock()->TimeInMicroseconds());
        }

        static void OnFrameSizeChange(UnityVideoRenderer* renderer, int width, int height) { }
    };

    TEST_P(VideoRendererTest, SetAndGetFrameBuffer)
    {
        EXPECT_EQ(nullptr, m_renderer->GetFrameBuffer());
        auto builder = CreateBlackFrameBuilder(kWidth, kHeight);
        m_renderer->OnFrame(builder.build());
        EXPECT_NE(nullptr, m_renderer->GetFrameBuffer());
    }

    TEST_P(VideoRendererTest, ConvertVideoFrameToTexture)
    {
        auto builder = CreateBlackFrameBuilder(kWidth, kHeight);
        m_renderer->OnFrame(builder.build());

        void* data = m_renderer->ConvertVideoFrameToTextureAndWriteToBuffer(kWidth, kHeight, libyuv::FOURCC_ARGB);
        EXPECT_NE(nullptr, data);
    }

    INSTANTIATE_TEST_SUITE_P(GfxDeviceAndColorSpece, VideoRendererTest, testing::ValuesIn(VALUES_TEST_ENV));

} // end namespace webrtc
} // end namespace unity
