#include "pch.h"
#include "GraphicsDeviceTestBase.h"
#include "GraphicsDevice/IGraphicsDevice.h"
#include "GraphicsDevice/ITexture2D.h"
#include "Codec/EncoderFactory.h"
#include "Codec/IEncoder.h"
#include "Context.h"

#include "rtc_base/ref_counted_object.h"

namespace unity
{
namespace webrtc
{

class ContextTest : public GraphicsDeviceTestBase
{
protected:
    const int width = 256;
    const int height = 256;
    std::unique_ptr<Context> context;
    std::unique_ptr<IEncoder> encoder_;

    void SetUp() override {
        GraphicsDeviceTestBase::SetUp();
        EXPECT_NE(nullptr, m_device);

        encoder_ = EncoderFactory::GetInstance().Init(width, height, m_device, m_encoderType, m_textureFormat);
        EXPECT_NE(nullptr, encoder_);

        context = std::make_unique<Context>();
    }
    void TearDown() override {
        GraphicsDeviceTestBase::TearDown();
    }
};
TEST_P(ContextTest, InitializeAndFinalizeEncoder) {
    const std::unique_ptr<ITexture2D> tex(m_device->CreateDefaultTextureV(width, height, m_textureFormat));
    EXPECT_NE(nullptr, tex);
    const auto source = context->CreateVideoSource();
    const auto track = context->CreateVideoTrack("video", source);
    EXPECT_TRUE(context->InitializeEncoder(encoder_.get(), track));
    context->RemoveRefPtr(track);
    context->RemoveRefPtr(source);
}

TEST_P(ContextTest, CreateAndDeleteMediaStream) {
    const auto stream = context->CreateMediaStream("test");
    context->RemoveRefPtr(stream);
}


TEST_P(ContextTest, CreateAndDeleteVideoTrack) {
    const std::unique_ptr<ITexture2D> tex(m_device->CreateDefaultTextureV(width, height, m_textureFormat));
    EXPECT_NE(nullptr, tex.get());
    const auto source = context->CreateVideoSource();
    const auto track = context->CreateVideoTrack("video", source);
    EXPECT_NE(nullptr, track);
    EXPECT_TRUE(context->InitializeEncoder(encoder_.get(), track));
    context->RemoveRefPtr(track);
    context->RemoveRefPtr(source);
}

TEST_P(ContextTest, CreateAndDeleteAudioTrack) {
    const auto source = context->CreateAudioSource();
    const auto track = context->CreateAudioTrack("audio", source);
    context->RemoveRefPtr(track);
    context->RemoveRefPtr(source);
}

TEST_P(ContextTest, AddAndRemoveAudioTrackToMediaStream) {
    const auto stream = context->CreateMediaStream("audiostream");
    const auto source = context->CreateAudioSource();
    const auto track = context->CreateAudioTrack("audio", source);
    const auto audiotrack = reinterpret_cast<webrtc::AudioTrackInterface*>(track);
    stream->AddTrack(audiotrack);
    stream->RemoveTrack(audiotrack);
    context->RemoveRefPtr(stream);
    context->RemoveRefPtr(track);
    context->RemoveRefPtr(source);
}

TEST_P(ContextTest, AddAndRemoveVideoTrackToMediaStream) {
    const std::unique_ptr<ITexture2D> tex(m_device->CreateDefaultTextureV(width, height, m_textureFormat));
    const auto stream = context->CreateMediaStream("videostream");
    const auto source = context->CreateVideoSource();
    const auto track = context->CreateVideoTrack("video", source);
    const auto videoTrack = reinterpret_cast<webrtc::VideoTrackInterface*>(track);
    stream->AddTrack(videoTrack);
    stream->RemoveTrack(videoTrack);
    context->RemoveRefPtr(stream);
    context->RemoveRefPtr(track);
    context->RemoveRefPtr(source);
}

TEST_P(ContextTest, CreateAndDeletePeerConnection) {
    const webrtc::PeerConnectionInterface::RTCConfiguration config;
    const auto connection = context->CreatePeerConnection(config);
    context->DeletePeerConnection(connection);
}

TEST_P(ContextTest, CreateAndDeleteDataChannel) {
    const webrtc::PeerConnectionInterface::RTCConfiguration config;
    const auto connection = context->CreatePeerConnection(config);
    DataChannelInit init;
    init.protocol = "";
    const auto channel = context->CreateDataChannel(connection, "test", init);
    context->DeleteDataChannel(channel);
    context->DeletePeerConnection(connection);
}

TEST_P(ContextTest, CreateAndDeleteVideoRenderer) {
    const auto renderer = context->CreateVideoRenderer();
    EXPECT_NE(nullptr, renderer);
    context->DeleteVideoRenderer(renderer);
}

TEST_P(ContextTest, EqualRendererGetById) {
    const auto renderer = context->CreateVideoRenderer();
    EXPECT_NE(nullptr, renderer);
    const auto rendererId = renderer->GetId();
    const auto rendererGetById = context->GetVideoRenderer(rendererId);
    EXPECT_NE(nullptr, rendererGetById);
    context->DeleteVideoRenderer(renderer);
}

TEST_P(ContextTest, AddAndRemoveVideoRendererToVideoTrack) {
    const std::unique_ptr<ITexture2D> tex(m_device->CreateDefaultTextureV(width, height, m_textureFormat));
    const auto source = context->CreateVideoSource();
    const auto track = context->CreateVideoTrack("video", source);
    const auto renderer = context->CreateVideoRenderer();
    track->AddOrUpdateSink(renderer, rtc::VideoSinkWants());
    track->RemoveSink(renderer);
    context->DeleteVideoRenderer(renderer);
    context->RemoveRefPtr(track);
    context->RemoveRefPtr(source);
}

INSTANTIATE_TEST_CASE_P(GraphicsDeviceParameters, ContextTest, testing::ValuesIn(VALUES_TEST_ENV));

} // end namespace webrtc
} // end namespace unity
