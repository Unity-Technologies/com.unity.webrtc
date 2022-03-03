#include "pch.h"
#include "GraphicsDeviceTestBase.h"
#include "Codec/EncoderFactory.h"
#include "Codec/IEncoder.h"
#include "Context.h"
#include "GraphicsDevice/IGraphicsDevice.h"
#include "GraphicsDevice/ITexture2D.h"
#include "UnityVideoTrackSource.h"
#include "UnityVideoRenderer.h"

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
    VideoRendererTest() :
        encoder_(EncoderFactory::GetInstance().Init(width, height, m_device, m_encoderType, m_textureFormat)),
        m_texture(m_device->CreateDefaultTextureV(width, height, m_textureFormat))
    {
        m_trackSource = new rtc::RefCountedObject<UnityVideoTrackSource>(
            /*is_screencast=*/ false,
            /*needs_denoising=*/ absl::nullopt);
        m_callback = &OnFrameSizeChange;
        m_renderer = std::make_unique<UnityVideoRenderer>(1, m_callback, true);
        m_trackSource->AddOrUpdateSink(m_renderer.get(), rtc::VideoSinkWants());
        m_trackSource->SetEncoder(encoder_.get());

        EXPECT_NE(nullptr, m_device);
        EXPECT_NE(nullptr, encoder_);

        context = std::make_unique<Context>();
    }
    ~VideoRendererTest() override
    {
        m_trackSource->RemoveSink(m_renderer.get());
    }
protected:
    std::unique_ptr<IEncoder> encoder_;
    std::unique_ptr<Context> context;
    std::unique_ptr<ITexture2D> m_texture;

    std::unique_ptr<UnityVideoRenderer> m_renderer;
    rtc::scoped_refptr<UnityVideoTrackSource> m_trackSource;
    DelegateVideoFrameResize m_callback;

    webrtc::VideoFrame::Builder CreateBlackFrameBuilder(int width, int height)
    {
        rtc::scoped_refptr<webrtc::I420Buffer> buffer =
            webrtc::I420Buffer::Create(width, height);

        webrtc::I420Buffer::SetBlack(buffer);
        return webrtc::VideoFrame::Builder()
            .set_video_frame_buffer(buffer)
            .set_timestamp_us(Clock::GetRealTimeClock()->TimeInMicroseconds());
    }

    static void OnFrameSizeChange(UnityVideoRenderer* renderer, int width, int height)
    {
    }

    void SendTestFrame(int width, int height)
    {
        auto builder = CreateBlackFrameBuilder(width, height);
        m_trackSource->DelegateOnFrame(builder.build());
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

TEST_P(VideoRendererTest, SendTestFrame)
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

    void* data = m_renderer->ConvertVideoFrameToTextureAndWriteToBuffer(
        width, height, libyuv::FOURCC_ARGB);
    EXPECT_NE(nullptr, data);
}

INSTANTIATE_TEST_SUITE_P(
    GraphicsDeviceParameters,
    VideoRendererTest,
    testing::ValuesIn(VALUES_TEST_ENV));

} // end namespace webrtc
} // end namespace unity
