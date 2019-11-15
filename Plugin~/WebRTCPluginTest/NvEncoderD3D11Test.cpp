#include "pch.h"
#include "D3D11GraphicsDeviceTestBase.h"
#include <d3d11.h>
#include <wrl/client.h>
#include "../unity/include/IUnityGraphics.h"
#include "../WebRTCPlugin/GraphicsDevice/GraphicsDevice.h"
#include "../WebRTCPlugin/GraphicsDevice/ITexture2D.h"
#include "../WebRTCPlugin/Codec/EncoderFactory.h"
#include "../WebRTCPlugin/Codec/IEncoder.h"

using namespace WebRTC;

class NvEncoderD3D11Test : public D3D11GraphicsDeviceTestBase
{
protected:
    IEncoder* encoder;
    IGraphicsDevice* device;

    void SetUp() override {
        D3D11GraphicsDeviceTestBase::SetUp();

        GraphicsDevice::GetInstance().Init(UnityGfxRenderer::kUnityGfxRendererD3D11, pD3DDevice.Get());
        device = GraphicsDevice::GetInstance().GetDevice();
        EXPECT_NE(nullptr, device);

        int width = 256;
        int height = 256;
        EncoderFactory::GetInstance().Init(width, height, device);
        encoder = EncoderFactory::GetInstance().GetEncoder();
        EXPECT_NE(nullptr, encoder);
    }
    void TearDown() override {
        EncoderFactory::GetInstance().Shutdown();
        D3D11GraphicsDeviceTestBase::TearDown();
    }
};
TEST_F(NvEncoderD3D11Test, EncoderIsSupported) {
    EXPECT_TRUE(encoder->IsSupported());
}

TEST_F(NvEncoderD3D11Test, EncoderCopyFrame) {
    int width = 256;
    int height = 256;
    auto tex = device->CreateDefaultTextureV(width, height);
    auto result = encoder->CopyFrame(tex->GetEncodeTexturePtrV());
    EXPECT_TRUE(result);
}

TEST_F(NvEncoderD3D11Test, EncoderEncodeFrame) {
    auto before = encoder->GetCurrentFrameCount();
    encoder->EncodeFrame();
    auto after = encoder->GetCurrentFrameCount();
    EXPECT_EQ(before + 1, after);
}
