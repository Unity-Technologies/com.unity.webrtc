#include "pch.h"

#include "Context.h"
#include "GraphicsDevice/IGraphicsDevice.h"
#include "GraphicsDevice/ITexture2D.h"
#include "GraphicsDeviceTestBase.h"
#include "UnityVideoRenderer.h"
#include "UnityVideoTrackSource.h"

using testing::_;
using testing::Invoke;
using testing::Mock;

namespace unity
{
namespace webrtc
{
    const int width = 256;
    const int height = 256;

    class VideoRendererTest : public GraphicsDeviceTestBase
    {
    public:
        VideoRendererTest()
        {
            m_trackSource = UnityVideoTrackSource::Create(
                /*is_screencast=*/false,
                /*needs_denoising=*/absl::nullopt);
            m_callback = &OnFrameSizeChange;
            m_renderer = std::make_unique<UnityVideoRenderer>(1, m_callback, true);
            m_trackSource->AddOrUpdateSink(m_renderer.get(), rtc::VideoSinkWants());
        }
        ~VideoRendererTest() override { m_trackSource->RemoveSink(m_renderer.get()); }

    protected:
        void SetUp() override
        {
            if (!device())
                GTEST_SKIP() << "The graphics driver is not installed on the device.";
            m_texture.reset(device()->CreateDefaultTextureV(width, height, format()));
            context = std::make_unique<Context>(device());
        }
        std::unique_ptr<Context> context;
        std::unique_ptr<ITexture2D> m_texture;

        std::unique_ptr<UnityVideoRenderer> m_renderer;
        rtc::scoped_refptr<UnityVideoTrackSource> m_trackSource;
        DelegateVideoFrameResize m_callback;

        webrtc::VideoFrame::Builder CreateBlackFrameBuilder(int width, int height)
        {
            rtc::scoped_refptr<webrtc::I420Buffer> buffer = webrtc::I420Buffer::Create(width, height);

            webrtc::I420Buffer::SetBlack(buffer);
            return webrtc::VideoFrame::Builder().set_video_frame_buffer(buffer).set_timestamp_us(
                Clock::GetRealTimeClock()->TimeInMicroseconds());
        }

        static void OnFrameSizeChange(UnityVideoRenderer* renderer, int width, int height) { }

        void SendTestFrame(int width, int height)
        {
            // auto builder = CreateBlackFrameBuilder(width, height);
            m_trackSource->OnFrameCaptured(0);
        }
    };

    TEST_P(VideoRendererTest, SetAndGetFrameBuffer)
    {
        int width = 256;
        int height = 256;
        EXPECT_EQ(nullptr, m_renderer->GetFrameBuffer());
        auto builder = CreateBlackFrameBuilder(width, height);
        m_renderer->OnFrame(builder.build());
        EXPECT_NE(nullptr, m_renderer->GetFrameBuffer());
    }

    // todo(kazuki)
    TEST_P(VideoRendererTest, DISABLED_SendTestFrame)
    {
        int width = 256;
        int height = 256;
        EXPECT_EQ(nullptr, m_renderer->GetFrameBuffer());
        SendTestFrame(width, height);
        EXPECT_NE(nullptr, m_renderer->GetFrameBuffer());
    }

    TEST_P(VideoRendererTest, ConvertVideoFrameToTexture)
    {
        int width = 256;
        int height = 256;
        auto builder = CreateBlackFrameBuilder(width, height);
        m_renderer->OnFrame(builder.build());

        void* data = m_renderer->ConvertVideoFrameToTextureAndWriteToBuffer(width, height, libyuv::FOURCC_ARGB);
        EXPECT_NE(nullptr, data);
    }

    INSTANTIATE_TEST_SUITE_P(GfxDeviceAndColorSpece, VideoRendererTest, testing::ValuesIn(VALUES_TEST_ENV));

} // end namespace webrtc
} // end namespace unity
