#include "pch.h"

#include "rtc_base/ref_counted_object.h"

#include "Context.h"
#include "GraphicsDeviceTestBase.h"
#include "GraphicsDevice/IGraphicsDevice.h"
#include "GraphicsDevice/ITexture2D.h"

#include "GraphicsDeviceContainer.h"

namespace unity
{
namespace webrtc
{

using namespace ::webrtc;

class ContextTest : public testing::TestWithParam<UnityGfxRenderer>
{
protected:
    const int width = 256;
    const int height = 256;
    std::unique_ptr<GraphicsDeviceContainer> container_;
    std::unique_ptr<Context> context;
    IGraphicsDevice* device_;
    DelegateVideoFrameResize callback_videoframeresize;

    explicit ContextTest()
        : container_(CreateGraphicsDeviceContainer(GetParam()))
        , device_(container_->device())
    {
        callback_videoframeresize = &OnFrameSizeChange;
    }

    void SetUp() override
    {
        if (!device_)
            GTEST_SKIP() << "The graphics driver is not installed on the device.";
        context = std::make_unique<Context>(device_);
    }

    static void OnFrameSizeChange(UnityVideoRenderer* renderer, int width, int height)
    {
    }
};
TEST_P(ContextTest, InitializeAndFinalizeEncoder) {
    const std::unique_ptr<ITexture2D> tex(container_->device()->CreateDefaultTextureV(width, height, kUnityRenderingExtFormatR8G8B8A8_SRGB));
    EXPECT_NE(nullptr, tex);
    const auto source = context->CreateVideoSource();
    const auto track = context->CreateVideoTrack("video", source);

    context->RemoveRefPtr(track);
    context->RemoveRefPtr(source);
}

TEST_P(ContextTest, CreateAndDeleteMediaStream) {
    const auto stream = context->CreateMediaStream("test");
    context->RemoveRefPtr(stream);
}


TEST_P(ContextTest, CreateAndDeleteVideoTrack) {
    const std::unique_ptr<ITexture2D> tex(container_->device()->CreateDefaultTextureV(width, height, kUnityRenderingExtFormatR8G8B8A8_SRGB));
    EXPECT_NE(nullptr, tex.get());
    const auto source = context->CreateVideoSource();
    const auto track = context->CreateVideoTrack("video", source);
    EXPECT_NE(nullptr, track);

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
    const std::unique_ptr<ITexture2D> tex(container_->device()->CreateDefaultTextureV(width, height, kUnityRenderingExtFormatR8G8B8A8_SRGB));
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
    const auto renderer = context->CreateVideoRenderer(callback_videoframeresize, true);
    EXPECT_NE(nullptr, renderer);
    context->DeleteVideoRenderer(renderer);
}

TEST_P(ContextTest, EqualRendererGetById) {
    const auto renderer = context->CreateVideoRenderer(callback_videoframeresize, true);
    EXPECT_NE(nullptr, renderer);
    const auto rendererId = renderer->GetId();
    const auto rendererGetById = context->GetVideoRenderer(rendererId);
    EXPECT_NE(nullptr, rendererGetById);
    context->DeleteVideoRenderer(renderer);
}

TEST_P(ContextTest, AddAndRemoveVideoRendererToVideoTrack) {
    const std::unique_ptr<ITexture2D> tex(container_->device()->CreateDefaultTextureV(width, height, kUnityRenderingExtFormatR8G8B8A8_SRGB));
    const auto source = context->CreateVideoSource();
    const auto track = context->CreateVideoTrack("video", source);
    const auto renderer = context->CreateVideoRenderer(callback_videoframeresize, true);
    track->AddOrUpdateSink(renderer, rtc::VideoSinkWants());
    track->RemoveSink(renderer);
    context->DeleteVideoRenderer(renderer);
    context->RemoveRefPtr(track);
    context->RemoveRefPtr(source);
}

INSTANTIATE_TEST_SUITE_P(GfxDevice, ContextTest, testing::ValuesIn(supportedGfxDevices));

} // end namespace webrtc
} // end namespace unity
