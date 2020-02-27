#include "pch.h"
#include "GraphicsDeviceTestBase.h"
#include "../WebRTCPlugin/GraphicsDevice/ITexture2D.h"
#include "../WebRTCPlugin/Codec/EncoderFactory.h"
#include "../WebRTCPlugin/Codec/IEncoder.h"
#include "../WebRTCPlugin/NvVideoCapturer.h"

using namespace WebRTC;
using namespace testing;

class VideoCapturerTest : public GraphicsDeviceTestBase
{
protected:
    IEncoder* encoder_ = nullptr;
    const int width_ = 256;
    const int height_ = 256;
    std::unique_ptr<NvVideoCapturer> capturer_;

    void SetUp() override {
        GraphicsDeviceTestBase::SetUp();
        EXPECT_NE(nullptr, m_device);

        EncoderFactory::GetInstance().Init(width_, height_, m_device, UnityEncoderHardware);
        encoder_ = EncoderFactory::GetInstance().GetEncoder();
        EXPECT_NE(nullptr, encoder_);

        capturer_ = std::make_unique<NvVideoCapturer>();
    }

    void TearDown() override {
        EncoderFactory::GetInstance().Shutdown();
        GraphicsDeviceTestBase::TearDown();
    }
};
TEST_P(VideoCapturerTest, InitializeAndFinalize) {
    capturer_->InitializeEncoder(m_device, UnityEncoderHardware);
    capturer_->FinalizeEncoder();
}

TEST_P(VideoCapturerTest, EncodeVideoData) {
    capturer_->InitializeEncoder(m_device, UnityEncoderHardware);
    auto tex = m_device->CreateDefaultTextureV(width_, height_);
    capturer_->SetFrameBuffer(tex->GetEncodeTexturePtrV());
    capturer_->EncodeVideoData();
    capturer_->FinalizeEncoder();
}

INSTANTIATE_TEST_CASE_P(GraphicsDeviceParameters, VideoCapturerTest, ValuesIn(VALUES_TEST_ENV));
