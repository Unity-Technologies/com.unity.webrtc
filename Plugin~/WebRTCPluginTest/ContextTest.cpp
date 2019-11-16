#include "pch.h"
#include <wrl/client.h>
#include "D3D11GraphicsDeviceTestBase.h"
#include "../unity/include/IUnityGraphics.h"
#include "../WebRTCPlugin/GraphicsDevice/GraphicsDevice.h"
#include "../WebRTCPlugin/GraphicsDevice/ITexture2D.h"
#include "../WebRTCPlugin/Codec/EncoderFactory.h"
#include "../WebRTCPlugin/Codec/IEncoder.h"
#include "../WebRTCPlugin/Context.h"

using namespace WebRTC;

class ContextTest : public D3D11GraphicsDeviceTestBase
{
protected:
    IEncoder* encoder_ = nullptr;
    IGraphicsDevice* device_ = nullptr;
    const int width = 256;
    const int height = 256;
    std::unique_ptr<Context> context;

    void SetUp() override {
        D3D11GraphicsDeviceTestBase::SetUp();

        GraphicsDevice::GetInstance().Init(kUnityGfxRendererD3D11, pD3DDevice.Get());
        device_ = GraphicsDevice::GetInstance().GetDevice();
        EXPECT_NE(nullptr, device_);

        EncoderFactory::GetInstance().Init(width, height, device_);
        encoder_ = EncoderFactory::GetInstance().GetEncoder();
        EXPECT_NE(nullptr, encoder_);

        context = std::make_unique<Context>();
    }
    void TearDown() override {
        EncoderFactory::GetInstance().Shutdown();
        D3D11GraphicsDeviceTestBase::TearDown();
    }
};
TEST_F(ContextTest, InitializeAndFinalizeEncoder) {
    context->InitializeEncoder(device_);
    context->FinalizerEncoder();
}

TEST_F(ContextTest, CreateAndDeleteVideoStream) {
    context->InitializeEncoder(device_);
    auto tex = device_->CreateDefaultTextureV(width, height);
    const auto stream = context->CreateVideoStream(tex->GetEncodeTexturePtrV(), width, height);
    context->DeleteVideoStream(stream);
    context->FinalizerEncoder();
}

TEST_F(ContextTest, CreateAndDeleteAudioStream) {
    const auto stream = context->CreateAudioStream();
    context->DeleteAudioStream(stream);
}

TEST_F(ContextTest, CreateAndDeletePeerConnection) {
    const auto connection = context->CreatePeerConnection();
    context->DeletePeerConnection(connection);
}

TEST_F(ContextTest, CreateAndDeleteDataChannel) {
    const auto connection = context->CreatePeerConnection();
    RTCDataChannelInit init;
    const auto channel = context->CreateDataChannel(connection, "test", init);
    context->DeleteDataChannel(channel);
    context->DeletePeerConnection(connection);
}
