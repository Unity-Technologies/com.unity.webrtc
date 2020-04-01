#include "pch.h"
#include "DummyVideoEncoder.h"
#include "NvVideoCapturer.h"

namespace unity
{
namespace webrtc
{

    DummyVideoEncoder::DummyVideoEncoder(IVideoEncoderObserver* observer)
    {
        this->m_setKeyFrame.connect(observer, &IVideoEncoderObserver::SetKeyFrame);
        this->m_setRates.connect(observer, &IVideoEncoderObserver::SetRates);
    }

    int32_t DummyVideoEncoder::InitEncode(const webrtc::VideoCodec* codec_settings, int32_t number_of_cores, size_t max_payload_size)
    {
        //m_bitrateAdjuster = std::make_unique<webrtc::BitrateAdjuster>(*codec_settings);
        m_bitrateAdjuster.reset(new webrtc::BitrateAdjuster(.5, .95));
        //m_bitrateAdjuster->OnEncoderInfo(this->GetEncoderInfo());
        return WEBRTC_VIDEO_CODEC_OK;
    }

    int32_t DummyVideoEncoder::RegisterEncodeCompleteCallback(webrtc::EncodedImageCallback* callback)
    {
        this->callback = callback;
        return WEBRTC_VIDEO_CODEC_OK;
    }

    int32_t DummyVideoEncoder::Release()
    {
        this->callback = nullptr;
        return WEBRTC_VIDEO_CODEC_OK;
    }

    int32_t DummyVideoEncoder::Encode(const webrtc::VideoFrame& frame, const std::vector<webrtc::VideoFrameType>* frameTypes)
    {
        FrameBuffer* frameBuffer = static_cast<FrameBuffer*>(frame.video_frame_buffer().get());
        std::vector<uint8_t>& frameDataBuffer = frameBuffer->buffer();

        encodedImage._completeFrame = true;
        encodedImage.SetTimestamp(frame.timestamp());
        encodedImage._encodedWidth = frame.video_frame_buffer()->width();
        encodedImage._encodedHeight = frame.video_frame_buffer()->height();
        encodedImage.ntp_time_ms_ = frame.ntp_time_ms();
        encodedImage.capture_time_ms_ = frame.render_time_ms();
        encodedImage.rotation_ = frame.rotation();
        encodedImage.content_type_ = webrtc::VideoContentType::UNSPECIFIED;
        encodedImage.timing_.flags = webrtc::VideoSendTiming::kInvalid;
        encodedImage._frameType = webrtc::VideoFrameType::kVideoFrameDelta;
        encodedImage.SetColorSpace(frame.color_space());
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
            m_setKeyFrame(m_encoderId);
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

        int qp;
        m_h264BitstreamParser.ParseBitstream(frameDataBuffer.data(), frameDataBuffer.size());
        m_h264BitstreamParser.GetLastSliceQp(&qp);
        encodedImage.qp_ = qp;

        webrtc::CodecSpecificInfo codecInfo;
        codecInfo.codecType = webrtc::kVideoCodecH264;
        codecInfo.codecSpecific.H264.packetization_mode = webrtc::H264PacketizationMode::NonInterleaved;

        const auto result = callback->OnEncodedImage(encodedImage, &codecInfo, &fragHeader);
        if (result.error != webrtc::EncodedImageCallback::Result::OK)
        {
            LogPrint("Encode callback failed %d", result.error);
            return WEBRTC_VIDEO_CODEC_ERROR;
        }
        return WEBRTC_VIDEO_CODEC_OK;
    }

    void DummyVideoEncoder::SetRates(const RateControlParameters& parameters)
    {
        m_setRates(m_encoderId, parameters);
    }

} // end namespace webrtc
} // end namespace unity
