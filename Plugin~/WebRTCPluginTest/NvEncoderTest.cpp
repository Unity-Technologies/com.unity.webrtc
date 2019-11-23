#include "pch.h"
#include "GraphicsDeviceTestBase.h"
#include "../WebRTCPlugin/GraphicsDevice/ITexture2D.h"
#include "../WebRTCPlugin/Codec/EncoderFactory.h"
#include "../WebRTCPlugin/Codec/IEncoder.h"

using namespace WebRTC;

class NvEncoderTest : public GraphicsDeviceTestBase
{
protected:
    IEncoder* encoder_ = nullptr;

    void SetUp() override {
        GraphicsDeviceTestBase::SetUp();
        EXPECT_NE(nullptr, m_device);

        const auto width = 256;
        const auto height = 256;
        EncoderFactory::GetInstance().Init(width, height, m_device);
        encoder_ = EncoderFactory::GetInstance().GetEncoder();
        EXPECT_NE(nullptr, encoder_);
    }
    void TearDown() override {
        EncoderFactory::GetInstance().Shutdown();
        GraphicsDeviceTestBase::TearDown();
    }
};
TEST_P(NvEncoderTest, IsSupported) {
    EXPECT_TRUE(encoder_->IsSupported());
}

TEST_P(NvEncoderTest, CopyBuffer) {
    const auto width = 256;
    const auto height = 256;
    auto tex = m_device->CreateDefaultTextureV(width, height);
    const auto result = encoder_->CopyBuffer(tex->GetEncodeTexturePtrV());
    EXPECT_TRUE(result);
}

TEST_P(NvEncoderTest, EncodeFrame) {
    auto before = encoder_->GetCurrentFrameCount();
    EXPECT_TRUE(encoder_->EncodeFrame());
    const auto after = encoder_->GetCurrentFrameCount();
    EXPECT_EQ(before + 1, after);
}

INSTANTIATE_TEST_CASE_P(
    GraphicsDeviceParameters,
    NvEncoderTest,
    testing::Values(GraphicsDeviceTestBase::CreateParameter())
);
