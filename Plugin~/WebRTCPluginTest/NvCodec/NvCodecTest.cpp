#include "pch.h"

#include "Codec/NvCodec/NvCodec.h"
#include "FrameGenerator.h"
#include "GraphicsDevice/IGraphicsDevice.h"
#include "GraphicsDeviceContainer.h"
#include "NvCodecUtils.h"
#include "VideoCodecTest.h"

using testing::Values;

namespace unity
{
namespace webrtc
{
    class NvCodecTest : public VideoCodecTest
    {
    public:
        NvCodecTest()
        {
            container_ = CreateGraphicsDeviceContainer(GetParam());
            context_ = container_->device()->GetCUcontext();
        }
        ~NvCodecTest() override
        {
            if (encoder_)
                encoder_ = nullptr;
            if (decoder_)
                decoder_ = nullptr;
            EXPECT_TRUE(ck(cuCtxDestroy(context_)));
        }

    protected:
        std::unique_ptr<VideoEncoder> CreateEncoder() override
        {
            cricket::VideoCodec codec = cricket::VideoCodec(cricket::kH264CodecName);
            codec.SetParam(cricket::kH264FmtpProfileLevelId, kProfileLevelIdString());
            return NvEncoder::Create(
                codec, context_, CU_MEMORYTYPE_ARRAY, NV_ENC_BUFFER_FORMAT_ARGB);
        }

        std::unique_ptr<VideoDecoder> CreateDecoder() override
        {
            cricket::VideoCodec codec = cricket::VideoCodec(cricket::kH264CodecName);
            codec.SetParam(cricket::kH264FmtpProfileLevelId, kProfileLevelIdString());
            return NvDecoder::Create(codec, context_);
        }

        std::unique_ptr<FrameGeneratorInterface> CreateFrameGenerator(
            int width,
            int height,
            absl::optional<FrameGeneratorInterface::OutputType> type,
            absl::optional<int> num_squares) override
        {
            return CreateVideoFrameGenerator(container_->device(), width, height, type, num_squares);
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
            EXPECT_EQ(kVideoCodecH264, codec_specific_info->codecType);
            EXPECT_EQ(0, encodedFrame->SpatialIndex());
        }

        void VerifyQpParser(const EncodedImage& encoded_frame)
        {
            EXPECT_GT(encoded_frame.size(), 0u);

            bitstreamParser_.ParseBitstream(rtc::ArrayView<const uint8_t>(encoded_frame.data(), encoded_frame.size()));
            int qp = bitstreamParser_.GetLastSliceQp().value_or(-1);
            EXPECT_EQ(encoded_frame.qp_, qp) << "Encoder QP != parsed bitstream QP.";
        }

        CUdevice device_;
        CUcontext context_;
        H264BitstreamParser bitstreamParser_;
        std::unique_ptr<GraphicsDeviceContainer> container_;
    };

    TEST_P(NvCodecTest, SupportedNvEncoderCodecs)
    {
        std::vector<SdpVideoFormat> formats = SupportedNvEncoderCodecs(context_);
        EXPECT_GT(formats.size(), 0);
    }

    TEST_P(NvCodecTest, SupportedNvDecoderCodecs)
    {
        std::vector<SdpVideoFormat> formats = SupportedNvDecoderCodecs(context_);
        EXPECT_GT(formats.size(), 0);
    }

    TEST_P(NvCodecTest, SupportedEncoderCount) { EXPECT_GT(SupportedEncoderCount(context_), 0); }

    TEST_P(NvCodecTest, SetRates)
    {
        EXPECT_EQ(WEBRTC_VIDEO_CODEC_OK, encoder_->InitEncode(&codecSettings_, kSettings()));

        const uint32_t kBitrateBps = 300000;
        VideoBitrateAllocation bitrate_allocation;
        bitrate_allocation.SetBitrate(0, 0, kBitrateBps);
        // EXPECT_CALL(
        //    *vpx,
        //    codec_enc_config_set(
        //        _,
        //        AllOf(
        //            Field(&vpx_codec_enc_cfg_t::rc_target_bitrate, kBitrateBps / 1000),
        //            Field(&vpx_codec_enc_cfg_t::rc_undershoot_pct, 100u),
        //            Field(&vpx_codec_enc_cfg_t::rc_overshoot_pct, 15u),
        //            Field(&vpx_codec_enc_cfg_t::rc_buf_sz, 1000u),
        //            Field(&vpx_codec_enc_cfg_t::rc_buf_optimal_sz, 600u),
        //            Field(&vpx_codec_enc_cfg_t::rc_dropframe_thresh, 30u))))
        //    .WillOnce(Return(VPX_CODEC_OK));

        encoder_->SetRates(
            VideoEncoder::RateControlParameters(bitrate_allocation, static_cast<double>(codecSettings_.maxFramerate)));
    }

    // todo(kazuki)
    TEST_P(NvCodecTest, EncodeFrameAndRelease)
    {
        EXPECT_EQ(WEBRTC_VIDEO_CODEC_OK, encoder_->Release());
        EXPECT_EQ(WEBRTC_VIDEO_CODEC_OK, encoder_->InitEncode(&codecSettings_, kSettings()));

        EncodedImage encoded_frame;
        CodecSpecificInfo codec_specific_info;
        EncodeAndWaitForFrame(NextInputFrame(), &encoded_frame, &codec_specific_info);

        EXPECT_EQ(WEBRTC_VIDEO_CODEC_OK, encoder_->Release());
        EXPECT_EQ(WEBRTC_VIDEO_CODEC_UNINITIALIZED, encoder_->Encode(NextInputFrame(), nullptr));
    }

    TEST_P(NvCodecTest, EncodeOnWorkerThread)
    {
        EXPECT_EQ(WEBRTC_VIDEO_CODEC_OK, encoder_->Release());
        EXPECT_EQ(WEBRTC_VIDEO_CODEC_OK, encoder_->InitEncode(&codecSettings_, kSettings()));

        EncodedImage encoded_frame;
        CodecSpecificInfo codec_specific_info;

        std::unique_ptr<rtc::Thread> thread = rtc::Thread::CreateWithSocketServer();
        thread->Start();

        // Test for executing command on several thread asyncnously.
        int count = 100;
        std::queue<VideoFrame> frames;
        while (count)
        {
            frames.push(NextInputFrame());
            thread->PostTask(
                RTC_FROM_HERE,
                [&]()
                {
                    VideoFrame frame = frames.front();
                    EncodeAndWaitForFrame(frame, &encoded_frame, &codec_specific_info);
                    frames.pop();
                });
            count--;
        }

        thread->Stop();
        EXPECT_EQ(WEBRTC_VIDEO_CODEC_OK, encoder_->Release());
    }

    TEST_P(NvCodecTest, EncodeDecode)
    {
        EXPECT_EQ(WEBRTC_VIDEO_CODEC_OK, encoder_->Release());
        EXPECT_EQ(WEBRTC_VIDEO_CODEC_OK, encoder_->InitEncode(&codecSettings_, kSettings()));
        EXPECT_EQ(WEBRTC_VIDEO_CODEC_OK, decoder_->Release());
        EXPECT_EQ(WEBRTC_VIDEO_CODEC_OK, decoder_->InitDecode(&codecSettings_, 1));

        EncodedImage encoded_frame;
        CodecSpecificInfo codec_specific_info;
        EncodeAndWaitForFrame(NextInputFrame(), &encoded_frame, &codec_specific_info);

        // First frame should be a key frame.
        encoded_frame._frameType = VideoFrameType::kVideoFrameKey;
        EXPECT_EQ(WEBRTC_VIDEO_CODEC_OK, decoder_->Decode(encoded_frame, false, 0));
        std::unique_ptr<VideoFrame> decoded_frame;
        absl::optional<uint8_t> decoded_qp;
        ASSERT_TRUE(WaitForDecodedFrame(&decoded_frame, &decoded_qp));
        ASSERT_TRUE(decoded_frame);

        // todo: set color space data on decode frame
        // const ColorSpace color_space = *decoded_frame->color_space();
        // EXPECT_EQ(ColorSpace::PrimaryID::kUnspecified, color_space.primaries());
        // EXPECT_EQ(ColorSpace::TransferID::kUnspecified, color_space.transfer());
        // EXPECT_EQ(ColorSpace::MatrixID::kUnspecified, color_space.matrix());
        // EXPECT_EQ(ColorSpace::RangeID::kInvalid, color_space.range());
        // EXPECT_EQ(ColorSpace::ChromaSiting::kUnspecified, color_space.chroma_siting_horizontal());
        // EXPECT_EQ(ColorSpace::ChromaSiting::kUnspecified, color_space.chroma_siting_vertical());
    }

    TEST_P(NvCodecTest, DecodedQpEqualsEncodedQp)
    {
        EXPECT_EQ(WEBRTC_VIDEO_CODEC_OK, encoder_->Release());
        EXPECT_EQ(WEBRTC_VIDEO_CODEC_OK, encoder_->InitEncode(&codecSettings_, kSettings()));
        EXPECT_EQ(WEBRTC_VIDEO_CODEC_OK, decoder_->Release());
        EXPECT_EQ(WEBRTC_VIDEO_CODEC_OK, decoder_->InitDecode(&codecSettings_, 1));

        EncodedImage encoded_frame;
        CodecSpecificInfo codec_specific_info;
        EncodeAndWaitForFrame(NextInputFrame(), &encoded_frame, &codec_specific_info);

        // First frame should be a key frame.
        encoded_frame._frameType = VideoFrameType::kVideoFrameKey;
        EXPECT_EQ(WEBRTC_VIDEO_CODEC_OK, decoder_->Decode(encoded_frame, false, 0));
        std::unique_ptr<VideoFrame> decoded_frame;
        absl::optional<uint8_t> decoded_qp;
        ASSERT_TRUE(WaitForDecodedFrame(&decoded_frame, &decoded_qp));
        ASSERT_TRUE(decoded_frame);
        ASSERT_TRUE(decoded_qp);
        EXPECT_EQ(encoded_frame.qp_, *decoded_qp);
    }

    TEST_P(NvCodecTest, DecodedTimeStampEqualsEncodedTimeStamp)
    {
        EXPECT_EQ(WEBRTC_VIDEO_CODEC_OK, encoder_->Release());
        EXPECT_EQ(WEBRTC_VIDEO_CODEC_OK, encoder_->InitEncode(&codecSettings_, kSettings()));
        EXPECT_EQ(WEBRTC_VIDEO_CODEC_OK, decoder_->Release());
        EXPECT_EQ(WEBRTC_VIDEO_CODEC_OK, decoder_->InitDecode(&codecSettings_, 1));

        EncodedImage encoded_frame;
        CodecSpecificInfo codec_specific_info;
        EncodeAndWaitForFrame(NextInputFrame(), &encoded_frame, &codec_specific_info);

        // First frame should be a key frame.
        encoded_frame._frameType = VideoFrameType::kVideoFrameKey;
        EXPECT_EQ(WEBRTC_VIDEO_CODEC_OK, decoder_->Decode(encoded_frame, false, 0));
        std::unique_ptr<VideoFrame> decoded_frame;
        absl::optional<uint8_t> decoded_qp;
        ASSERT_TRUE(WaitForDecodedFrame(&decoded_frame, &decoded_qp));
        ASSERT_TRUE(decoded_frame);
        EXPECT_EQ(encoded_frame.Timestamp(), decoded_frame->timestamp());
    }

    INSTANTIATE_TEST_SUITE_P(GfxDevice, NvCodecTest, testing::ValuesIn(supportedGfxDevices));

}
}
