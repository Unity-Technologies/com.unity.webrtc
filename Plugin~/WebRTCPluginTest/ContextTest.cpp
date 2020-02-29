#include "pch.h"
#include "GraphicsDeviceTestBase.h"
#include "../WebRTCPlugin/GraphicsDevice/ITexture2D.h"
#include "../WebRTCPlugin/Codec/EncoderFactory.h"
#include "../WebRTCPlugin/Codec/IEncoder.h"
#include "../WebRTCPlugin/Context.h"

using namespace WebRTC;
using namespace testing;

class ContextTest : public GraphicsDeviceTestBase
{
protected:
    IEncoder* encoder_ = nullptr;
    const int width = 256;
    const int height = 256;
    std::unique_ptr<Context> context;

    void SetUp() override {
        GraphicsDeviceTestBase::SetUp();
        EXPECT_NE(nullptr, m_device);

        EncoderFactory::GetInstance().Init(width, height, m_device, encoderType);
        encoder_ = EncoderFactory::GetInstance().GetEncoder();
        EXPECT_NE(nullptr, encoder_);

        context = std::make_unique<Context>();
    }
    void TearDown() override {
        EncoderFactory::GetInstance().Shutdown();
        GraphicsDeviceTestBase::TearDown();
    }
};
TEST_P(ContextTest, InitializeAndFinalizeEncoder) {
//    EXPECT_EQ(CodecInitializationResult::NotInitialized, context->GetCodecInitializationResult());
    EXPECT_TRUE(context->InitializeEncoder(m_device));
//    EXPECT_EQ(CodecInitializationResult::Success, context->GetCodecInitializationResult());
    context->FinalizeEncoder();
//    EXPECT_EQ(CodecInitializationResult::NotInitialized, context->GetCodecInitializationResult());
}

TEST_P(ContextTest, CreateAndDeleteMediaStream) {
    const auto stream = context->CreateMediaStream("test");
    context->DeleteMediaStream(stream);
}


TEST_P(ContextTest, CreateAndDeleteVideoTrack) {
    EXPECT_TRUE(context->InitializeEncoder(m_device));
    const auto tex = m_device->CreateDefaultTextureV(width, height);
    const auto track = context->CreateVideoTrack("video", tex, 256, 256, 10000000);
    context->FinalizeEncoder();
}

TEST_P(ContextTest, CreateAndDeleteAudioStream) {
//    const auto stream = context->CreateAudioStream();
//    context->DeleteAudioStream(stream);
//      const auto stream = context->CreateMediaStream("audio");
      //context->
    context->CreateAudioTrack("audio");
}

TEST_P(ContextTest, CreateAndDeletePeerConnection) {
    const auto connection = context->CreatePeerConnection();
    context->DeletePeerConnection(connection);
}

TEST_P(ContextTest, CreateAndDeleteDataChannel) {
    const auto connection = context->CreatePeerConnection();
    RTCDataChannelInit init;
    init.protocol = "";
    const auto channel = context->CreateDataChannel(connection, "test", init);
    context->DeleteDataChannel(channel);
    context->DeletePeerConnection(connection);
}

INSTANTIATE_TEST_CASE_P(GraphicsDeviceParameters, ContextTest, ValuesIn(VALUES_TEST_ENV));
