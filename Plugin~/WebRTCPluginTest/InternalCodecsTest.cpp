#include "pch.h"

#include "FrameGenerator.h"
#include "GraphicsDevice/IGraphicsDevice.h"
#include "GraphicsDevice/ITexture2D.h"
#include "GraphicsDeviceContainer.h"
#include "VideoCodecTest.h"
#include "media/engine/internal_decoder_factory.h"
#include "media/engine/internal_encoder_factory.h"
#include "modules/video_coding/utility/vp8_header_parser.h"
#include "test/video_codec_settings.h"

namespace unity
{
namespace webrtc
{
    constexpr int kWidth = 172;
    constexpr int kHeight = 144;
    constexpr UnityRenderingExtTextureFormat kFormat = kUnityRenderingExtFormatR8G8B8A8_SRGB;

    using testing::Values;

    class InternalCodecsTest : public VideoCodecTest
    {
    public:
        InternalCodecsTest()
            : container_(CreateGraphicsDeviceContainer(GetParam()))
            , device_(container_->device())
        {
        }
        ~InternalCodecsTest() override
        {
            if (encoder_)
                encoder_ = nullptr;
        }

    protected:
        void SetUp() override
        {
            if (!device_)
                GTEST_SKIP() << "The graphics driver is not installed on the device.";
            std::unique_ptr<ITexture2D> texture(device_->CreateDefaultTextureV(kWidth, kHeight, kFormat));
            if (!texture)
                GTEST_SKIP() << "The graphics driver cannot create a texture resource.";

            VideoCodecTest::SetUp();
        }

        SdpVideoFormat FindFormat(std::string name, const std::vector<SdpVideoFormat>& formats)
        {
            auto result =
                std::find_if(formats.begin(), formats.end(), [name](SdpVideoFormat x) { return x.name == name; });
            return *result;
        }

        std::unique_ptr<VideoEncoder> CreateEncoder() override
        {
            SdpVideoFormat format = FindFormat(codecName, encoderFactory.GetSupportedFormats());
            return encoderFactory.CreateVideoEncoder(format);
        }

        std::unique_ptr<VideoDecoder> CreateDecoder() override
        {
            SdpVideoFormat format = FindFormat(codecName, decoderFactory.GetSupportedFormats());
            return decoderFactory.CreateVideoDecoder(format);
        }

        std::unique_ptr<FrameGeneratorInterface> CreateFrameGenerator(
            int width,
            int height,
            absl::optional<FrameGeneratorInterface::OutputType> type,
            absl::optional<int> num_squares) override
        {
            return CreateVideoFrameGenerator(container_->device(), width, height, type, num_squares);
        }

        void ModifyCodecSettings(VideoCodec* codec_settings) override
        {
            webrtc::test::CodecSettings(kVideoCodecVP8, codec_settings);
            codec_settings->width = kWidth;
            codec_settings->height = kHeight;
            codec_settings->SetVideoEncoderComplexity(VideoCodecComplexity::kComplexityNormal);
        }

        void VerifyQpParser(const EncodedImage& encoded_frame) const
        {
            int qp;
            EXPECT_GT(encoded_frame.size(), 0u);
            ASSERT_TRUE(vp8::GetQp(encoded_frame.data(), encoded_frame.size(), &qp));
            EXPECT_EQ(encoded_frame.qp_, qp) << "Encoder QP != parsed bitstream QP.";
        }

        void EncodeAndWaitForFrame(
            const VideoFrame& inputFrame,
            EncodedImage* encodedFrame,
            CodecSpecificInfo* codec_specific_info,
            bool keyframe = false)
        {
            std::vector<VideoFrameType> frame_types;
            if (keyframe)
            {
                frame_types.emplace_back(VideoFrameType::kVideoFrameKey);
            }
            else
            {
                frame_types.emplace_back(VideoFrameType::kVideoFrameDelta);
            }
            EXPECT_EQ(WEBRTC_VIDEO_CODEC_OK, encoder_->Encode(inputFrame, &frame_types));
            ASSERT_TRUE(WaitForEncodedFrame(encodedFrame, codec_specific_info));
            VerifyQpParser(*encodedFrame);
            EXPECT_EQ(kVideoCodecVP8, codec_specific_info->codecType);
            EXPECT_TRUE(encodedFrame->SimulcastIndex().has_value());
            EXPECT_EQ(0, encodedFrame->SimulcastIndex());
        }

        std::string codecName = "VP8";
        InternalEncoderFactory encoderFactory;
        InternalDecoderFactory decoderFactory;
        std::unique_ptr<GraphicsDeviceContainer> container_;
        IGraphicsDevice* device_;
    };

    TEST_P(InternalCodecsTest, EncodeFrameAndRelease)
    {
        EXPECT_EQ(WEBRTC_VIDEO_CODEC_OK, encoder_->Release());
        EXPECT_EQ(WEBRTC_VIDEO_CODEC_OK, encoder_->InitEncode(&codecSettings_, kSettings()));

        EncodedImage encoded_frame;
        CodecSpecificInfo codec_specific_info;
        EncodeAndWaitForFrame(NextInputFrame(), &encoded_frame, &codec_specific_info);

        EXPECT_EQ(WEBRTC_VIDEO_CODEC_OK, encoder_->Release());
        EXPECT_EQ(WEBRTC_VIDEO_CODEC_UNINITIALIZED, encoder_->Encode(NextInputFrame(), nullptr));
    }

    INSTANTIATE_TEST_SUITE_P(GfxDevice, InternalCodecsTest, testing::ValuesIn(supportedGfxDevices));

}
}
