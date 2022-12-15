#include "pch.h"

#include <rtc_base/ref_counted_object.h>

#include "Context.h"
#include "GraphicsDevice/IGraphicsDevice.h"
#include "GraphicsDevice/ITexture2D.h"
#include "GraphicsDeviceContainer.h"
#include "GraphicsDeviceTestBase.h"
#include "VideoFrameUtil.h"

namespace unity
{
namespace webrtc
{

    using namespace ::webrtc;

    class ContextTest : public testing::TestWithParam<UnityGfxRenderer>
    {
    protected:
        const uint32_t kWidth = 256;
        const uint32_t kHeight = 256;
        const UnityRenderingExtTextureFormat kFormat = kUnityRenderingExtFormatR8G8B8A8_SRGB;

        std::unique_ptr<GraphicsDeviceContainer> container_;
        std::unique_ptr<Context> context;
        IGraphicsDevice* device_;
        std::unique_ptr<ITexture2D> texture_;
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
            texture_.reset(device_->CreateDefaultTextureV(kWidth, kHeight, kFormat));
            if (!texture_)
                GTEST_SKIP() << "The graphics driver cannot create a texture resource.";

            ContextDependencies dependencies;
            dependencies.device = device_;
            context = std::make_unique<Context>(dependencies);
        }

        static void OnFrameSizeChange(UnityVideoRenderer* renderer, int width, int height) { }
    };

    TEST_P(ContextTest, Constructor)
    {
        context = nullptr;

        ContextDependencies dependencies;
        dependencies.device = nullptr;
        context = std::make_unique<Context>(dependencies);
    }

    TEST_P(ContextTest, InitializeAndFinalizeEncoder)
    {
        const auto source = context->CreateVideoSource();
        EXPECT_NE(nullptr, source);
        const auto track = context->CreateVideoTrack("video", source.get());
        EXPECT_NE(nullptr, track);
    }

    TEST_P(ContextTest, CreateAndDeleteMediaStream)
    {
        const auto stream = context->CreateMediaStream("test");
        EXPECT_NE(nullptr, stream);
    }

    TEST_P(ContextTest, CreateAndDeleteVideoTrack)
    {
        const auto source = context->CreateVideoSource();
        EXPECT_NE(nullptr, source);
        const auto track = context->CreateVideoTrack("video", source.get());
        EXPECT_NE(nullptr, track);
    }

    TEST_P(ContextTest, CreateAndDeleteAudioTrack)
    {
        const auto source = context->CreateAudioSource();
        EXPECT_NE(nullptr, source);
        const auto track = context->CreateAudioTrack("audio", source.get());
        EXPECT_NE(nullptr, track);
    }

    TEST_P(ContextTest, AddAndRemoveAudioTrackToMediaStream)
    {
        const auto stream = context->CreateMediaStream("audiostream");
        EXPECT_NE(nullptr, stream);
        const auto source = context->CreateAudioSource();
        EXPECT_NE(nullptr, source);
        const auto track = context->CreateAudioTrack("audio", source.get());
        EXPECT_NE(nullptr, track);
        EXPECT_TRUE(stream->AddTrack(track));
        EXPECT_TRUE(stream->RemoveTrack(track));
    }

    TEST_P(ContextTest, AddAndRemoveVideoTrackToMediaStream)
    {
        const auto stream = context->CreateMediaStream("videostream");
        EXPECT_NE(nullptr, stream);
        const auto source = context->CreateVideoSource();
        EXPECT_NE(nullptr, source);
        const auto track = context->CreateVideoTrack("video", source.get());
        EXPECT_NE(nullptr, track);
        EXPECT_TRUE(stream->AddTrack(track));
        EXPECT_TRUE(stream->RemoveTrack(track));
    }

    TEST_P(ContextTest, CreateAndDeletePeerConnection)
    {
        const webrtc::PeerConnectionInterface::RTCConfiguration config;
        const auto connection = context->CreatePeerConnection(config);
        EXPECT_NE(nullptr, connection);
        context->DeletePeerConnection(connection);
    }

    TEST_P(ContextTest, CreateAndDeleteDataChannel)
    {
        const webrtc::PeerConnectionInterface::RTCConfiguration config;
        const auto connection = context->CreatePeerConnection(config);
        EXPECT_NE(nullptr, connection);
        DataChannelInit init;
        init.protocol = "";
        const auto channel = context->CreateDataChannel(connection, "test", init);
        EXPECT_NE(nullptr, channel);
        context->DeleteDataChannel(channel);
        context->DeletePeerConnection(connection);
    }

    TEST_P(ContextTest, AddTrackAndRemoveTrack)
    {
        const webrtc::PeerConnectionInterface::RTCConfiguration config;
        const auto connection = context->CreatePeerConnection(config);
        EXPECT_NE(nullptr, connection);
        auto source = context->CreateVideoSource();
        EXPECT_NE(nullptr, source);
        const auto track = context->CreateVideoTrack("video", source.get());
        EXPECT_NE(nullptr, track);
        std::vector<std::string> streamIds;
        const auto result = connection->connection->AddTrack(track, streamIds);
        EXPECT_TRUE(result.ok());

        auto frame = CreateTestFrame(device_, texture_.get(), kFormat);
        source->OnFrameCaptured(frame);

        const auto sender = result.value();
        const auto result2 = connection->connection->RemoveTrackOrError(sender);
        EXPECT_TRUE(result2.ok());
        context->DeletePeerConnection(connection);
    }

    TEST_P(ContextTest, CreateAndDeleteVideoRenderer)
    {
        const auto renderer = context->CreateVideoRenderer(callback_videoframeresize, true);
        EXPECT_NE(nullptr, renderer);
        context->DeleteVideoRenderer(renderer);
    }

    TEST_P(ContextTest, EqualRendererGetById)
    {
        const auto renderer = context->CreateVideoRenderer(callback_videoframeresize, true);
        EXPECT_NE(nullptr, renderer);
        const auto rendererId = renderer->GetId();
        const auto rendererGetById = context->GetVideoRenderer(rendererId);
        EXPECT_NE(nullptr, rendererGetById);
        context->DeleteVideoRenderer(renderer);
    }

    TEST_P(ContextTest, AddAndRemoveVideoRendererToVideoTrack)
    {
        const auto source = context->CreateVideoSource();
        EXPECT_NE(nullptr, source);
        const auto track = context->CreateVideoTrack("video", source.get());
        EXPECT_NE(nullptr, track);
        const auto renderer = context->CreateVideoRenderer(callback_videoframeresize, true);
        EXPECT_NE(nullptr, renderer);
        track->AddOrUpdateSink(renderer, rtc::VideoSinkWants());
        track->RemoveSink(renderer);
        context->DeleteVideoRenderer(renderer);
    }

    INSTANTIATE_TEST_SUITE_P(GfxDevice, ContextTest, testing::ValuesIn(supportedGfxDevices));

} // end namespace webrtc
} // end namespace unity
