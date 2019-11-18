#include "pch.h"
#include "D3D11GraphicsDeviceTestBase.h"
#include "../unity/include/IUnityGraphics.h"
#include "../WebRTCPlugin/GraphicsDevice/GraphicsDevice.h"
#include "../WebRTCPlugin/GraphicsDevice/ITexture2D.h"
#include "../WebRTCPlugin/Codec/EncoderFactory.h"
#include "../WebRTCPlugin/Codec/IEncoder.h"

using namespace WebRTC;

class NvEncoderD3D11Test : public D3D11GraphicsDeviceTestBase
{
protected:
    IEncoder* encoder_ = nullptr;
    IGraphicsDevice* device_ = nullptr;

    void SetUp() override {
        UnityGfxRenderer unityGfxRenderer;
        void* pGraphicsDevice;
        std::tie(unityGfxRenderer, pGraphicsDevice) = GetParam();
        GraphicsDevice::GetInstance().Init(unityGfxRenderer, pGraphicsDevice);
        device_ = GraphicsDevice::GetInstance().GetDevice();
        EXPECT_NE(nullptr, device_);

        const auto width = 256;
        const auto height = 256;
        EncoderFactory::GetInstance().Init(width, height, device_);
        encoder_ = EncoderFactory::GetInstance().GetEncoder();
        EXPECT_NE(nullptr, encoder_);
    }
    void TearDown() override {
        EncoderFactory::GetInstance().Shutdown();
        D3D11GraphicsDeviceTestBase::TearDown();
    }
};
TEST_P(NvEncoderD3D11Test, IsSupported) {
    EXPECT_TRUE(encoder_->IsSupported());
}

TEST_P(NvEncoderD3D11Test, CopyBuffer) {
    const auto width = 256;
    const auto height = 256;
    auto tex = device_->CreateDefaultTextureV(width, height);
    const auto result = encoder_->CopyBuffer(tex->GetEncodeTexturePtrV());
    EXPECT_TRUE(result);
}

TEST_P(NvEncoderD3D11Test, EncodeFrame) {
    auto before = encoder_->GetCurrentFrameCount();
    encoder_->EncodeFrame();
    const auto after = encoder_->GetCurrentFrameCount();
    EXPECT_EQ(before + 1, after);
}

INSTANTIATE_TEST_CASE_P(
        GraphicsDeviceParameters,
        NvEncoderD3D11Test,
        testing::Values(std::make_tuple(kUnityGfxRendererOpenGLCore, nullptr))
);
