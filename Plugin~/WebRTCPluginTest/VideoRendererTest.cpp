#include "pch.h"
#include "GraphicsDeviceTestBase.h"
#include "../WebRTCPlugin/Codec/EncoderFactory.h"
#include "../WebRTCPlugin/Codec/IEncoder.h"
#include "../WebRTCPlugin/Context.h"
#include "../WebRTCPlugin/GraphicsDevice/ITexture2D.h"
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
        encoder_(EncoderFactory::GetInstance().Init(width, height, m_device, m_encoderType)),
        m_texture(m_device->CreateDefaultTextureV(width, height))
    {
        m_trackSource = new rtc::RefCountedObject<UnityVideoTrackSource>(
            m_texture->GetNativeTexturePtrV(),
            /*is_screencast=*/ false,
            /*needs_denoising=*/ absl::nullopt);
        m_renderer = std::make_unique<UnityVideoRenderer>(1);
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

    webrtc::VideoFrame::Builder CreateBlackFrameBuilder(int width, int height)
    {
        rtc::scoped_refptr<webrtc::I420Buffer> buffer =
            webrtc::I420Buffer::Create(width, height);

        webrtc::I420Buffer::SetBlack(buffer);
        return webrtc::VideoFrame::Builder().set_video_frame_buffer(buffer);
    }

    void SendTestFrame(int width, int height)
    {
        m_trackSource->OnFrameCaptured();
    }
};

// todo::(kazuki) fix MetalGraphicsDevice.mm
#if !defined(SUPPORT_METAL)
TEST_P(VideoRendererTest, SendTestFrame)
{
    int width = 256;
    int height = 256;
    SendTestFrame(width, height);
    EXPECT_NE(nullptr, m_renderer->GetFrameBuffer());
}
#endif

INSTANTIATE_TEST_CASE_P(
    GraphicsDeviceParameters,
    VideoRendererTest,
    testing::ValuesIn(VALUES_TEST_ENV));

} // end namespace webrtc
} // end namespace unity
