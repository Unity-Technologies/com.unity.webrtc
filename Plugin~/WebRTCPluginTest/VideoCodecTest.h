#pragma once

#include <IUnityGraphics.h>

#include "api/test/frame_generator_interface.h"
#include "rtc_base/checks.h"
#include "rtc_base/event.h"
#include "rtc_base/synchronization/mutex.h"
#include "gtest/gtest.h"
#include <api/video_codecs/h264_profile_level_id.h>
#include <api/video_codecs/video_codec.h>
#include <api/video_codecs/video_decoder.h>
#include <api/video_codecs/video_encoder.h>
#include <media/base/codec.h>
#include <modules/video_coding/include/video_codec_interface.h>

namespace unity
{
namespace webrtc
{
    using namespace ::webrtc;
    using namespace ::webrtc::test;

    // todo(kazuki):: move another header for CUDA platform
    constexpr int kNumCores = 1;
    constexpr size_t kMaxPayloadSize = 1440;
    const H264ProfileLevelId kProfileLevelId(H264Profile::kProfileBaseline, H264Level::kLevel3_1);
    VideoEncoder::Capabilities kCapabilities();
    std::string kProfileLevelIdString();
    VideoEncoder::Settings kSettings();

// todo(kazuki):: fix workaround
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Wunused-function"
    static void SetDefaultSettings(VideoCodec* codec_settings)
    {
        codec_settings->codecType = kVideoCodecH264;
        codec_settings->maxFramerate = 60;
        codec_settings->width = 1280;
        codec_settings->height = 720;
        codec_settings->SetFrameDropEnabled(true);
        codec_settings->startBitrate = 2000;
        codec_settings->maxBitrate = 4000;
    }

    static void SetDefaultSettings(VideoDecoder::Settings& codec_settings)
    {
        codec_settings.set_codec_type(kVideoCodecH264);
        codec_settings.set_max_render_resolution({ 1280, 720 });
        // If frame dropping is false, we get a warning that bitrate can't
        // be controlled for RC_QUALITY_MODE; RC_BITRATE_MODE and RC_TIMESTAMP_MODE
    }
#pragma clang diagnostic pop

    class VideoCodecTest : public testing::TestWithParam<UnityGfxRenderer>
    {
    public:
        VideoCodecTest()
            : encodedImageCallback_(this)
            , decodedImageCallback_(this)
            , lastInputFrameTimestamp_(0)
        {
        }
        virtual ~VideoCodecTest() override { }

        virtual std::unique_ptr<VideoEncoder> CreateEncoder() = 0;
        virtual std::unique_ptr<VideoDecoder> CreateDecoder() = 0;
        virtual std::unique_ptr<FrameGeneratorInterface> CreateFrameGenerator(
            int width,
            int height,
            absl::optional<FrameGeneratorInterface::OutputType> type,
            absl::optional<int> num_squares) = 0;
        virtual void ModifyCodecSettings(VideoCodec* codec_settings) = 0;
        void SetUp() override;
        void TearDown() override;

        VideoFrame NextInputFrame();
        void ChangeFrameResolution(size_t width, size_t height);

        // Helper method for waiting a single encoded frame.
        bool WaitForEncodedFrame(EncodedImage* frame, CodecSpecificInfo* codec_specific_info);
        bool
        WaitForEncodedFrames(std::vector<EncodedImage>* frames, std::vector<CodecSpecificInfo>* codec_specific_info);

        // Helper method for waiting a single decoded frame.
        bool WaitForDecodedFrame(std::unique_ptr<VideoFrame>* frame, absl::optional<uint8_t>* qp);

    protected:
        class FakeEncodedImageCallback : public EncodedImageCallback
        {
        public:
            explicit FakeEncodedImageCallback(VideoCodecTest* test)
                : _test(test)
            {
            }
            Result OnEncodedImage(const EncodedImage& frame, const CodecSpecificInfo* codec_specific_info) override;

        private:
            VideoCodecTest* _test;
        };
        class FakeDecodedImageCallback : public DecodedImageCallback
        {
        public:
            explicit FakeDecodedImageCallback(VideoCodecTest* test)
                : _test(test)
            {
            }
            int32_t Decoded(VideoFrame& decodedImage) override
            {
                RTC_DCHECK_NOTREACHED();
                return -1;
            }
            void
            Decoded(VideoFrame& frame, absl::optional<int32_t> decode_time_ms, absl::optional<uint8_t> qp) override;

        private:
            VideoCodecTest* _test;
        };

        VideoCodec codecSettings_;
        VideoDecoder::Settings decoderSettings_;
        std::unique_ptr<VideoEncoder> encoder_;
        std::unique_ptr<VideoDecoder> decoder_;
        std::unique_ptr<test::FrameGeneratorInterface> inputFrameGenerator_;

    private:
        rtc::Event encodedFrameEvent_;
        rtc::Event decodedFrameEvent_;
        Mutex encodedFrameSection_;
        Mutex decodedFrameSection_;
        std::vector<EncodedImage> encodedFrames_;
        absl::optional<VideoFrame> decodedFrame_;
        std::vector<CodecSpecificInfo> codecSpecificInfos_;
        absl::optional<uint8_t> decodedQp_;
        FakeEncodedImageCallback encodedImageCallback_;
        FakeDecodedImageCallback decodedImageCallback_;

        uint32_t lastInputFrameTimestamp_;
    };
}
}
