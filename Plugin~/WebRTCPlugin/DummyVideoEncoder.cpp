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
        if (codec_settings == nullptr)
        {
            return WEBRTC_VIDEO_CODEC_ERR_PARAMETER;
        }
        // TODO(kazuki):: this encoder should support codecs other than this.
        if (codec_settings->codecType != webrtc::kVideoCodecH264)
        {
            return WEBRTC_VIDEO_CODEC_ERR_PARAMETER;
        }
        if (codec_settings->maxFramerate == 0 || codec_settings->width == 0 || codec_settings->height == 0)
        {
            return WEBRTC_VIDEO_CODEC_ERR_PARAMETER;
        }
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

        // todo(kazuki): remove it when refactor video encoding process.
        m_encoderId = frameBuffer->encoderId();

        m_encodedImage._completeFrame = true;
        m_encodedImage.SetTimestamp(frame.timestamp());
        m_encodedImage._encodedWidth = frame.video_frame_buffer()->width();
        m_encodedImage._encodedHeight = frame.video_frame_buffer()->height();
        m_encodedImage.ntp_time_ms_ = frame.ntp_time_ms();
        m_encodedImage.capture_time_ms_ = frame.render_time_ms();
        m_encodedImage.rotation_ = frame.rotation();
        m_encodedImage.content_type_ = webrtc::VideoContentType::UNSPECIFIED;
        m_encodedImage.timing_.flags = webrtc::VideoSendTiming::kInvalid;
        m_encodedImage._frameType = webrtc::VideoFrameType::kVideoFrameDelta;
        m_encodedImage.SetColorSpace(frame.color_space());
        std::vector<webrtc::H264::NaluIndex> naluIndices =
            webrtc::H264::FindNaluIndices(&frameDataBuffer[0], frameDataBuffer.size());
        for (uint32_t i = 0; i < naluIndices.size(); i++)
        {
            const webrtc::H264::NaluType naluType = webrtc::H264::ParseNaluType(frameDataBuffer[naluIndices[i].payload_start_offset]);
            if (naluType == webrtc::H264::kIdr)
            {
                m_encodedImage._frameType = webrtc::VideoFrameType::kVideoFrameKey;
                break;
            }
        }

        if (m_encodedImage._frameType != webrtc::VideoFrameType::kVideoFrameKey && frameTypes && (*frameTypes)[0] == webrtc::VideoFrameType::kVideoFrameKey)
        {
            m_setKeyFrame(m_encoderId);
        }

        m_encodedImage.set_buffer(&frameDataBuffer[0], frameDataBuffer.capacity());
        m_encodedImage.set_size(frameDataBuffer.size());

        m_fragHeader.VerifyAndAllocateFragmentationHeader(naluIndices.size());
        m_fragHeader.fragmentationVectorSize = static_cast<uint16_t>(naluIndices.size());
        for (uint32_t i = 0; i < naluIndices.size(); i++)
        {
            webrtc::H264::NaluIndex const& NALUIndex = naluIndices[i];
            m_fragHeader.fragmentationOffset[i] = NALUIndex.payload_start_offset;
            m_fragHeader.fragmentationLength[i] = NALUIndex.payload_size;
        }

        int qp;
        m_h264BitstreamParser.ParseBitstream(frameDataBuffer.data(), frameDataBuffer.size());
        m_h264BitstreamParser.GetLastSliceQp(&qp);
        m_encodedImage.qp_ = qp;

        webrtc::CodecSpecificInfo codecInfo;
        codecInfo.codecType = webrtc::kVideoCodecH264;
        codecInfo.codecSpecific.H264.packetization_mode = webrtc::H264PacketizationMode::NonInterleaved;

        const auto result = callback->OnEncodedImage(m_encodedImage, &codecInfo, &m_fragHeader);
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
