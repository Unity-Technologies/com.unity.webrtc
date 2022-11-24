#include "pch.h"

#include <api/units/time_delta.h>

#include "VideoCodecTest.h"
#include "modules/rtp_rtcp/include/rtp_rtcp_defines.h"

static const webrtc::TimeDelta kEncodeTimeoutMs = webrtc::TimeDelta::Millis(100);
static const webrtc::TimeDelta kDecodeTimeoutMs = webrtc::TimeDelta::Millis(100);

namespace unity
{
namespace webrtc
{
    VideoEncoder::Capabilities kCapabilities() { return VideoEncoder::Capabilities(false); }
    std::string kProfileLevelIdString() { return *H264ProfileLevelIdToString(kProfileLevelId); }
    VideoEncoder::Settings kSettings() { return VideoEncoder::Settings(kCapabilities(), kNumCores, kMaxPayloadSize); }

    EncodedImageCallback::Result VideoCodecTest::FakeEncodedImageCallback::OnEncodedImage(
        const EncodedImage& frame, const CodecSpecificInfo* codec_specific_info)
    {
        MutexLock lock(&_test->encodedFrameSection_);
        _test->encodedFrames_.push_back(frame);
        RTC_DCHECK(codec_specific_info);
        _test->codecSpecificInfos_.push_back(*codec_specific_info);
        // if (!_test->wait_for_encoded_frames_threshold_)
        {
            _test->encodedFrameEvent_.Set();
            return Result(Result::OK);
        }

        // if (_test->encodedFrames_.size() == _test->wait_for_encoded_frames_threshold_)
        //{
        //    _test->wait_for_encoded_frames_threshold_ = 1;
        //    _test->encodedFrameEvent_.Set();
        //}
        // return Result(Result::OK);
    }

    void VideoCodecTest::FakeDecodedImageCallback::Decoded(
        VideoFrame& frame, absl::optional<int32_t> decode_time_ms, absl::optional<uint8_t> qp)
    {
        MutexLock lock(&_test->decodedFrameSection_);
        _test->decodedFrame_.emplace(frame);
        _test->decodedQp_ = qp;
        _test->decodedFrameEvent_.Set();
    }

    VideoFrame VideoCodecTest::NextInputFrame()
    {
        test::FrameGeneratorInterface::VideoFrameData frame_data = inputFrameGenerator_->NextFrame();
        VideoFrame input_frame = VideoFrame::Builder()
                                     .set_video_frame_buffer(frame_data.buffer)
                                     .set_update_rect(frame_data.update_rect)
                                     .build();

        // I420Buffer::SetBlack(frame_data.buffer);
        const uint32_t timestamp = lastInputFrameTimestamp_ + kVideoPayloadTypeFrequency / codecSettings_.maxFramerate;
        input_frame.set_timestamp(timestamp);

        lastInputFrameTimestamp_ = timestamp;
        return input_frame;
    }

    void VideoCodecTest::ChangeFrameResolution(size_t width, size_t height)
    {
        inputFrameGenerator_->ChangeResolution(width, height);
    }

    bool VideoCodecTest::WaitForEncodedFrame(EncodedImage* frame, CodecSpecificInfo* codec_specific_info)
    {
        std::vector<EncodedImage> frames;
        std::vector<CodecSpecificInfo> codec_specific_infos;
        if (!WaitForEncodedFrames(&frames, &codec_specific_infos))
            return false;
        EXPECT_EQ(frames.size(), static_cast<size_t>(1));
        EXPECT_EQ(frames.size(), codec_specific_infos.size());
        *frame = frames[0];
        *codec_specific_info = codec_specific_infos[0];
        return true;
    }

    bool VideoCodecTest::WaitForEncodedFrames(
        std::vector<EncodedImage>* frames, std::vector<CodecSpecificInfo>* codec_specific_info)
    {
        EXPECT_TRUE(encodedFrameEvent_.Wait(kEncodeTimeoutMs)) << "Timed out while waiting for encoded frame.";
        // This becomes unsafe if there are multiple threads waiting for frames.
        MutexLock lock(&encodedFrameSection_);
        EXPECT_FALSE(encodedFrames_.empty());
        EXPECT_FALSE(codecSpecificInfos_.empty());
        EXPECT_EQ(encodedFrames_.size(), codecSpecificInfos_.size());
        if (!encodedFrames_.empty())
        {
            *frames = encodedFrames_;
            encodedFrames_.clear();
            RTC_DCHECK(!codecSpecificInfos_.empty());
            *codec_specific_info = codecSpecificInfos_;
            codecSpecificInfos_.clear();
            return true;
        }
        return false;
    }

    bool VideoCodecTest::WaitForDecodedFrame(std::unique_ptr<VideoFrame>* frame, absl::optional<uint8_t>* qp)
    {
        EXPECT_TRUE(decodedFrameEvent_.Wait(kDecodeTimeoutMs)) << "Timed out while waiting for a decoded frame.";
        // This becomes unsafe if there are multiple threads waiting for frames.
        MutexLock lock(&decodedFrameSection_);
        EXPECT_TRUE(decodedFrame_);
        if (decodedFrame_)
        {
            frame->reset(new VideoFrame(std::move(*decodedFrame_)));
            *qp = decodedQp_;
            decodedFrame_.reset();
            return true;
        }
        else
        {
            return false;
        }
    }

    void VideoCodecTest::SetUp()
    {
        ModifyCodecSettings(&codecSettings_);

        inputFrameGenerator_ = CreateFrameGenerator(
            codecSettings_.width,
            codecSettings_.height,
            test::FrameGeneratorInterface::OutputType::kI420,
            absl::optional<int>());

        encoder_ = CreateEncoder();
        decoder_ = CreateDecoder();
        encoder_->RegisterEncodeCompleteCallback(&encodedImageCallback_);
        decoder_->RegisterDecodeCompleteCallback(&decodedImageCallback_);
    }

    void VideoCodecTest::TearDown()
    {
        // call destructor
        inputFrameGenerator_ = nullptr;
    }
}
}
