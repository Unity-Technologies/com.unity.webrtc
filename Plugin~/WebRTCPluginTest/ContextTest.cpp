#include "pch.h"
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
        //D3D11GraphicsDeviceTestBase::SetUp();

        UnityGfxRenderer unityGfxRenderer;
        void* pGraphicsDevice;
        std::tie(unityGfxRenderer, pGraphicsDevice) = GetParam();
        GraphicsDevice::GetInstance().Init(unityGfxRenderer, pGraphicsDevice);
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
TEST_P(ContextTest, InitializeAndFinalizeEncoder) {
    context->InitializeEncoder(device_);
    context->FinalizerEncoder();
}

TEST_P(ContextTest, CreateAndDeleteVideoStream) {
    context->InitializeEncoder(device_);
    auto tex = device_->CreateDefaultTextureV(width, height);
    const auto stream = context->CreateVideoStream(tex->GetEncodeTexturePtrV(), width, height);
    context->DeleteVideoStream(stream);
    context->FinalizerEncoder();
}

TEST_P(ContextTest, CreateAndDeleteAudioStream) {
    const auto stream = context->CreateAudioStream();
    context->DeleteAudioStream(stream);
}

TEST_P(ContextTest, CreateAndDeletePeerConnection) {
    const auto connection = context->CreatePeerConnection();
    context->DeletePeerConnection(connection);
}

TEST_P(ContextTest, CreateAndDeleteDataChannel) {
    const auto connection = context->CreatePeerConnection();
    RTCDataChannelInit init;
    const auto channel = context->CreateDataChannel(connection, "test", init);
    context->DeleteDataChannel(channel);
    context->DeletePeerConnection(connection);
}

INSTANTIATE_TEST_CASE_P(
        GraphicsDeviceParameters,
        ContextTest,
        testing::Values(std::make_tuple(kUnityGfxRendererOpenGLCore, nullptr))
        );