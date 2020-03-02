#include "pch.h"
#include "DummyVideoEncoder.h"
#include "NvVideoCapturer.h"

namespace WebRTC
{
    int32_t DummyVideoEncoder::Encode(
        const webrtc::VideoFrame& frame,
        const std::vector<webrtc::VideoFrameType>* frameTypes)
    {
        FrameBuffer* frameBuffer = static_cast<FrameBuffer*>(frame.video_frame_buffer().get());
        std::vector<uint8_t>& frameDataBuffer = frameBuffer->buffer;

        encodedImage._completeFrame = true;
        encodedImage.SetTimestamp(frame.timestamp());
        encodedImage._encodedWidth = frame.video_frame_buffer()->width();
        encodedImage._encodedHeight = frame.video_frame_buffer()->height();
        encodedImage.ntp_time_ms_ = frame.ntp_time_ms();
        encodedImage.rotation_ = frame.rotation();
        encodedImage.content_type_ = webrtc::VideoContentType::UNSPECIFIED;
        encodedImage.timing_.flags = webrtc::VideoSendTiming::kInvalid;
        encodedImage._frameType = webrtc::VideoFrameType::kVideoFrameDelta;
        std::vector<webrtc::H264::NaluIndex> naluIndices =
            webrtc::H264::FindNaluIndices(&frameDataBuffer[0], frameDataBuffer.size());
        for (uint32_t i = 0; i < naluIndices.size(); i++)
        {
            webrtc::H264::NaluType NALUType = webrtc::H264::ParseNaluType(frameDataBuffer[naluIndices[i].payload_start_offset]);
            if (NALUType == webrtc::H264::kIdr)
            {
                encodedImage._frameType = webrtc::VideoFrameType::kVideoFrameKey;
                break;
            }
        }

        if (encodedImage._frameType != webrtc::VideoFrameType::kVideoFrameKey && frameTypes && (*frameTypes)[0] == webrtc::VideoFrameType::kVideoFrameKey)
        {
            SetKeyFrame();
        }

        if (lastBitrate.get_sum_kbps() > 0)
        {
            RateControlParameters param(lastBitrate, 30);
            SetRates(param);
        }

        encodedImage.set_buffer(&frameDataBuffer[0], frameDataBuffer.capacity());
        encodedImage.set_size(frameDataBuffer.size());

        fragHeader.VerifyAndAllocateFragmentationHeader(naluIndices.size());
        fragHeader.fragmentationVectorSize = static_cast<uint16_t>(naluIndices.size());
        for (uint32_t i = 0; i < naluIndices.size(); i++)
        {
            webrtc::H264::NaluIndex const& NALUIndex = naluIndices[i];
            fragHeader.fragmentationOffset[i] = NALUIndex.payload_start_offset;
            fragHeader.fragmentationLength[i] = NALUIndex.payload_size;
        }
        webrtc::CodecSpecificInfo codecInfo;
        codecInfo.codecType = webrtc::kVideoCodecH264;
        auto result = callback->OnEncodedImage(encodedImage, &codecInfo, &fragHeader);
        if(result.error != webrtc::EncodedImageCallback::Result::OK)
        {
            LogPrint("Encode callback failed %d", result.error);
            return WEBRTC_VIDEO_CODEC_ERROR;
        }
        return WEBRTC_VIDEO_CODEC_OK;
    }

    void DummyVideoEncoder::SetRates(const webrtc::VideoEncoder::RateControlParameters& parameters)
    {
        lastBitrate = parameters.bitrate;
        SetRate(parameters.bitrate.get_sum_kbps() * 1000);
    }

    DummyVideoEncoderFactory::DummyVideoEncoderFactory(NvVideoCapturer* videoCapturer):capturer(videoCapturer){}
    std::vector<webrtc::SdpVideoFormat> DummyVideoEncoderFactory::GetSupportedFormats() const
    {
        const absl::optional<std::string> profileLevelId =
            webrtc::H264::ProfileLevelIdToString(webrtc::H264::ProfileLevelId(webrtc::H264::kProfileConstrainedBaseline, webrtc::H264::kLevel5_1));
        return { webrtc::SdpVideoFormat(
            cricket::kH264CodecName,
            { {cricket::kH264FmtpProfileLevelId, *profileLevelId},
              {cricket::kH264FmtpLevelAsymmetryAllowed, "1"},
              {cricket::kH264FmtpPacketizationMode, "1"} }) };
    }

    webrtc::VideoEncoderFactory::CodecInfo DummyVideoEncoderFactory::QueryVideoEncoder(const webrtc::SdpVideoFormat& format) const
    {
        return CodecInfo{ true, false };
    }
    std::unique_ptr<webrtc::VideoEncoder> DummyVideoEncoderFactory::CreateVideoEncoder(
        const webrtc::SdpVideoFormat& format)
    {
        auto dummyVideoEncoder = std::make_unique<DummyVideoEncoder>();
        dummyVideoEncoder->SetKeyFrame.connect(capturer, &NvVideoCapturer::SetKeyFrame);
        dummyVideoEncoder->SetRate.connect(capturer, &NvVideoCapturer::SetRate);
        return dummyVideoEncoder;
    }
}
