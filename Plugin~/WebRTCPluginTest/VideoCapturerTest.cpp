#include "pch.h"
#include "GraphicsDeviceTestBase.h"
#include "../WebRTCPlugin/GraphicsDevice/ITexture2D.h"
#include "../WebRTCPlugin/Codec/EncoderFactory.h"
#include "../WebRTCPlugin/Codec/IEncoder.h"
#include "../WebRTCPlugin/NvVideoCapturer.h"

namespace unity
{
namespace webrtc
{

class VideoCapturerTest : public GraphicsDeviceTestBase
{
protected:
    std::unique_ptr<IEncoder> encoder_;
    const int width_ = 256;
    const int height_ = 256;
    std::unique_ptr<NvVideoCapturer> capturer_;

    void SetUp() override {
        GraphicsDeviceTestBase::SetUp();
        EXPECT_NE(nullptr, m_device);

        encoder_ = EncoderFactory::GetInstance().Init(width_, height_, m_device, encoderType);
        EXPECT_NE(nullptr, encoder_);

        capturer_ = std::make_unique<NvVideoCapturer>();
    }

    void TearDown() override {
        GraphicsDeviceTestBase::TearDown();
    }
};
TEST_P(VideoCapturerTest, SetEncoder) {
    capturer_->SetEncoder(encoder_.get());
}

TEST_P(VideoCapturerTest, EncodeVideoData) {
    capturer_->SetEncoder(encoder_.get());
    auto tex = m_device->CreateDefaultTextureV(width_, height_);
    capturer_->SetFrameBuffer(tex->GetEncodeTexturePtrV());
    capturer_->EncodeVideoData();
}

INSTANTIATE_TEST_CASE_P(GraphicsDeviceParameters, VideoCapturerTest, testing::ValuesIn(VALUES_TEST_ENV));

} // end namespace webrtc
} // end namespace unity
