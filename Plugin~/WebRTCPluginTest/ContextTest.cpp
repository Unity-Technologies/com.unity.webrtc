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
    std::unique_ptr<IEncoder> encoder_;
    const int width = 256;
    const int height = 256;
    std::unique_ptr<Context> context;

    void SetUp() override {
        GraphicsDeviceTestBase::SetUp();
        EXPECT_NE(nullptr, m_device);

        encoder_ = EncoderFactory::GetInstance().Init(width, height, m_device, encoderType);
        EXPECT_NE(nullptr, encoder_);

        context = std::make_unique<Context>();
    }
    void TearDown() override {
        GraphicsDeviceTestBase::TearDown();
    }
};
TEST_P(ContextTest, InitializeAndFinalizeEncoder) {
    const std::unique_ptr<ITexture2D> tex(m_device->CreateDefaultTextureV(width, height));
    EXPECT_NE(nullptr, tex);
    const auto track = context->CreateVideoTrack("video", tex.get(), 256, 256, 10000000);
    EXPECT_TRUE(context->InitializeEncoder(encoder_.get(), track));
}

TEST_P(ContextTest, CreateAndDeleteMediaStream) {
    const auto stream = context->CreateMediaStream("test");
    context->DeleteMediaStream(stream);
}


TEST_P(ContextTest, CreateAndDeleteVideoTrack) {
    const std::unique_ptr<ITexture2D> tex(m_device->CreateDefaultTextureV(width, height));
    EXPECT_NE(nullptr, tex.get());
    const auto track = context->CreateVideoTrack("video", tex.get(), 256, 256, 10000000);
    EXPECT_NE(nullptr, track);
    EXPECT_TRUE(context->InitializeEncoder(encoder_.get(), track));
    context->DeleteMediaStreamTrack(track);
}

TEST_P(ContextTest, CreateAndDeleteAudioTrack) {
    const auto track = context->CreateAudioTrack("audio");
    context->DeleteMediaStreamTrack(track);
}

TEST_P(ContextTest, AddAndRemoveAudioTrackToMediaStream) {
    const auto stream = context->CreateMediaStream("audiostream");
    const auto track = context->CreateAudioTrack("audio");
    const auto audiotrack = reinterpret_cast<webrtc::AudioTrackInterface*>(track);
    stream->AddTrack(audiotrack);
    stream->RemoveTrack(audiotrack);
    context->DeleteMediaStream(stream);
    context->DeleteMediaStreamTrack(track);
}

TEST_P(ContextTest, AddAndRemoveVideoTrackToMediaStream) {
    const std::unique_ptr<ITexture2D> tex(m_device->CreateDefaultTextureV(width, height));
    const auto stream = context->CreateMediaStream("videostream");
    const auto track = context->CreateVideoTrack("video", tex.get(), 256, 256, 10000000);
    const auto videoTrack = reinterpret_cast<webrtc::AudioTrackInterface*>(track);
    stream->AddTrack(videoTrack);
    stream->RemoveTrack(videoTrack);
    context->DeleteMediaStream(stream);
    context->DeleteMediaStreamTrack(track);
}

TEST_P(ContextTest, CreateAndDeletePeerConnection) {
    const webrtc::PeerConnectionInterface::RTCConfiguration config;
    const auto connection = context->CreatePeerConnection(config);
    context->DeletePeerConnection(connection);
}

TEST_P(ContextTest, CreateAndDeleteDataChannel) {
    const webrtc::PeerConnectionInterface::RTCConfiguration config;
    const auto connection = context->CreatePeerConnection(config);
    RTCDataChannelInit init;
    init.protocol = "";
    const auto channel = context->CreateDataChannel(connection, "test", init);
    context->DeleteDataChannel(channel);
    context->DeletePeerConnection(connection);
}

INSTANTIATE_TEST_CASE_P(GraphicsDeviceParameters, ContextTest, ValuesIn(VALUES_TEST_ENV));
