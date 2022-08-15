#include "pch.h"

#include "Codec/MediaCodec/MediaCodec.h"
#include "FrameGenerator.h"
#include "GraphicsDevice/IGraphicsDevice.h"
#include "GraphicsDeviceContainer.h"
#include "VideoCodecTest.h"
#include <common_video/h264/h264_bitstream_parser.h>
#include <rtc_base/thread.h>

namespace unity
{
namespace webrtc
{
    using testing::Values;

    class MediaCodecTest : public VideoCodecTest
    {
    public:
        MediaCodecTest()
            : container_(CreateGraphicsDeviceContainer(GetParam()))
            , device_(container_->device())
        {
        }
        ~MediaCodecTest() override
        {
            if (encoder_)
                encoder_ = nullptr;
            if (decoder_)
                decoder_ = nullptr;
        }
        void SetUp() override
        {
            if (!device_)
                GTEST_SKIP() << "The graphics driver is not installed on the device.";
//            if (!NvEncoder::IsSupported())
//                GTEST_SKIP() << "Current Driver Version does not support this NvEncodeAPI version.";
//            context_ = device_->GetCUcontext();
            VideoCodecTest::SetUp();
        }

    protected:
        std::unique_ptr<VideoEncoder> CreateEncoder() override
        {
            cricket::VideoCodec codec = cricket::VideoCodec(cricket::kH264CodecName);
            codec.SetParam(cricket::kH264FmtpProfileLevelId, kProfileLevelIdString());
            return MediaCodecEncoder::Create(codec, device_, nullptr);
        }

        std::unique_ptr<VideoDecoder> CreateDecoder() override
        {
            cricket::VideoCodec codec = cricket::VideoCodec(cricket::kH264CodecName);
            codec.SetParam(cricket::kH264FmtpProfileLevelId, kProfileLevelIdString());
            return MediaCodecDecoder::Create(codec, device_, nullptr);
        }

        std::unique_ptr<FrameGeneratorInterface> CreateFrameGenerator(
            int width,
            int height,
            absl::optional<FrameGeneratorInterface::OutputType> type,
            absl::optional<int> num_squares) override
        {
            return CreateVideoFrameGenerator(device_, width, height, type, num_squares);
        }

        void ModifyCodecSettings(VideoCodec* codec_settings) override { SetDefaultSettings(codec_settings); }

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

        H264BitstreamParser bitstreamParser_;
        std::unique_ptr<GraphicsDeviceContainer> container_;
        IGraphicsDevice* device_;
    };

//    TEST_P(MediaCodecTest, SupportedNvEncoderCodecs)
//    {
//        std::vector<SdpVideoFormat> formats = SupportedNvEncoderCodecs(context_);
//        EXPECT_GT(formats.size(), 0);
//    }
//
//    TEST_P(MediaCodecTest, SupportedNvDecoderCodecs)
//    {
//        std::vector<SdpVideoFormat> formats = SupportedNvDecoderCodecs(context_);
//        EXPECT_GT(formats.size(), 0);
//    }

//    TEST_P(MediaCodecTest, SupportedEncoderCount) { EXPECT_GT(SupportedEncoderCount(context_), 0); }

    TEST_P(MediaCodecTest, SetRates)
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

    TEST_P(MediaCodecTest, EncodeFrameAndRelease)
    {
        EXPECT_EQ(WEBRTC_VIDEO_CODEC_OK, encoder_->Release());
        EXPECT_EQ(WEBRTC_VIDEO_CODEC_OK, encoder_->InitEncode(&codecSettings_, kSettings()));

        EncodedImage encoded_frame;
        CodecSpecificInfo codec_specific_info;
        EncodeAndWaitForFrame(NextInputFrame(), &encoded_frame, &codec_specific_info);

        EXPECT_EQ(WEBRTC_VIDEO_CODEC_OK, encoder_->Release());
        EXPECT_EQ(WEBRTC_VIDEO_CODEC_UNINITIALIZED, encoder_->Encode(NextInputFrame(), nullptr));
    }

    TEST_P(MediaCodecTest, EncodeOnWorkerThread)
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
            rtc::Thread::SleepMs(33);
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

    TEST_P(MediaCodecTest, EncodeDecode)
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
        const ColorSpace color_space = *decoded_frame->color_space();
        EXPECT_EQ(ColorSpace::PrimaryID::kUnspecified, color_space.primaries());
        EXPECT_EQ(ColorSpace::TransferID::kUnspecified, color_space.transfer());
        EXPECT_EQ(ColorSpace::MatrixID::kUnspecified, color_space.matrix());
        EXPECT_EQ(ColorSpace::RangeID::kInvalid, color_space.range());
        EXPECT_EQ(ColorSpace::ChromaSiting::kUnspecified, color_space.chroma_siting_horizontal());
        EXPECT_EQ(ColorSpace::ChromaSiting::kUnspecified, color_space.chroma_siting_vertical());
    }

    TEST_P(MediaCodecTest, ReconfigureDecoder)
    {
        EXPECT_EQ(WEBRTC_VIDEO_CODEC_OK, encoder_->Release());
        EXPECT_EQ(WEBRTC_VIDEO_CODEC_OK, encoder_->InitEncode(&codecSettings_, kSettings()));
        EXPECT_EQ(WEBRTC_VIDEO_CODEC_OK, decoder_->Release());
        EXPECT_EQ(WEBRTC_VIDEO_CODEC_OK, decoder_->InitDecode(&codecSettings_, 1));

        EncodedImage encoded_frame;
        CodecSpecificInfo codec_specific_info;
        VideoFrame frame = NextInputFrame();
        EncodeAndWaitForFrame(frame, &encoded_frame, &codec_specific_info);

        encoded_frame._frameType = VideoFrameType::kVideoFrameKey;
        EXPECT_EQ(WEBRTC_VIDEO_CODEC_OK, decoder_->Decode(encoded_frame, false, 0));
        std::unique_ptr<VideoFrame> decoded_frame;
        absl::optional<uint8_t> decoded_qp;
        ASSERT_TRUE(WaitForDecodedFrame(&decoded_frame, &decoded_qp));
        ASSERT_TRUE(decoded_frame);
        EXPECT_EQ(decoded_frame->width(), frame.width());
        EXPECT_EQ(decoded_frame->height(), frame.height());

        // change resolution
        uint16_t width = codecSettings_.width / 2;
        uint16_t height = codecSettings_.height / 2;
        codecSettings_.width = width;
        codecSettings_.height = height;
        EXPECT_EQ(WEBRTC_VIDEO_CODEC_OK, encoder_->Release());
        EXPECT_EQ(WEBRTC_VIDEO_CODEC_OK, encoder_->InitEncode(&codecSettings_, kSettings()));

        ChangeFrameResolution(static_cast<size_t>(width), static_cast<size_t>(height));

        VideoFrame frame2 = NextInputFrame();
        EncodeAndWaitForFrame(frame2, &encoded_frame, &codec_specific_info);
        encoded_frame._frameType = VideoFrameType::kVideoFrameKey;
        EXPECT_EQ(WEBRTC_VIDEO_CODEC_OK, decoder_->Decode(encoded_frame, false, 33));
        ASSERT_TRUE(WaitForDecodedFrame(&decoded_frame, &decoded_qp));
        ASSERT_TRUE(decoded_frame);

        // todo(kazuki): `pfnSequenceCallback` in NvEncoder.cpp is not called from NvDec when
        // the first frame after changing resolution, so the resolution of the first frame is old one.
        EXPECT_EQ(decoded_frame->width(), frame.width());
        EXPECT_EQ(decoded_frame->height(), frame.height());

        // The second frame after changing resolution is fine.
        EXPECT_EQ(WEBRTC_VIDEO_CODEC_OK, decoder_->Decode(encoded_frame, false, 66));
        ASSERT_TRUE(WaitForDecodedFrame(&decoded_frame, &decoded_qp));

        EXPECT_EQ(decoded_frame->width(), frame2.width());
        EXPECT_EQ(decoded_frame->height(), frame2.height());
    }

    TEST_P(MediaCodecTest, DecodedQpEqualsEncodedQp)
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

    TEST_P(MediaCodecTest, DecodedTimeStampEqualsEncodedTimeStamp)
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

    INSTANTIATE_TEST_SUITE_P(GfxDevice, MediaCodecTest, testing::ValuesIn(supportedGfxDevices));

}
}
