#include "pch.h"
#include "GraphicsDeviceTestBase.h"
#include "GraphicsDevice/IGraphicsDevice.h"
#include "GraphicsDevice/ITexture2D.h"
#include "Codec/EncoderFactory.h"
#include "Codec/IEncoder.h"

namespace unity
{
namespace webrtc
{
    
class NvEncoderTest : public GraphicsDeviceTestBase
{
protected:
    std::unique_ptr<IEncoder> encoder_;

    void SetUp() override {
        GraphicsDeviceTestBase::SetUp();
        EXPECT_NE(nullptr, m_device);

        const auto width = 256;
        const auto height = 256;
        encoder_ = EncoderFactory::GetInstance().Init(width, height, m_device, m_encoderType);
        EXPECT_NE(nullptr, encoder_);
    }
    void TearDown() override {
        GraphicsDeviceTestBase::TearDown();
    }
};
TEST_P(NvEncoderTest, IsSupported) {
    EXPECT_TRUE(encoder_->IsSupported());
}

TEST_P(NvEncoderTest, CopyBuffer) {
    const auto width = 256;
    const auto height = 256;
    const std::unique_ptr<ITexture2D> tex(m_device->CreateDefaultTextureV(width, height));
    const auto result = encoder_->CopyBuffer(tex->GetNativeTexturePtrV());
    EXPECT_TRUE(result);
}

TEST_P(NvEncoderTest, EncodeFrame) {
    auto before = encoder_->GetCurrentFrameCount();
    EXPECT_TRUE(encoder_->EncodeFrame());
    const auto after = encoder_->GetCurrentFrameCount();
    EXPECT_EQ(before + 1, after);
}

INSTANTIATE_TEST_CASE_P( GraphicsDeviceParameters, NvEncoderTest, testing::ValuesIn(VALUES_TEST_ENV));

} // end namespace webrtc
} // end namespace unity
