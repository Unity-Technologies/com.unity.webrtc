#include "pch.h"
#include "DummyVideoEncoder.h"

namespace unity
{
namespace webrtc
{

    DummyVideoEncoder::DummyVideoEncoder(IVideoEncoderObserver* observer)
        : m_encode_fps(1000, 1000)
        , m_clock(webrtc::Clock::GetRealTimeClock())
        , m_bitrateAdjuster(std::make_unique<webrtc::BitrateAdjuster>(0.5f, 0.95f))
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
        if(codec_settings->maxBitrate > 0 && codec_settings->startBitrate > codec_settings->maxBitrate)
        {
            return WEBRTC_VIDEO_CODEC_ERR_PARAMETER;
        }
        m_codec = codec_settings;

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
        this->m_setKeyFrame.disconnect_all();
        this->m_setRates.disconnect_all();
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

        m_encodedImage.SetEncodedData(webrtc::EncodedImageBuffer::Create(&frameDataBuffer[0], frameDataBuffer.size()));
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

        int64_t now_ms = m_clock->TimeInMilliseconds();
        m_encode_fps.Update(1, now_ms);

        m_bitrateAdjuster->Update(frameDataBuffer.size());

        return WEBRTC_VIDEO_CODEC_OK;
    }

    void DummyVideoEncoder::SetRates(const RateControlParameters& parameters)
    {

        //
        // "parameters.framerate_fps" which the parameter of the argument of
        // "SetRates" method, in many cases this parameter is higher than
        // the frequency of the encoding.
        // Need to determine the right framerate to set to the hardware encoder,
        // so collect timestamp of encoding and get stats.
        // 
        int64_t now_ms = m_clock->TimeInMilliseconds();
        absl::optional<int64_t> encodeFrameRate = m_encode_fps.Rate(now_ms);
        int64_t frameRate = encodeFrameRate.value_or(30);

        //
        // The bitrate adjuster is using for avoiding overshoot the bitrate.
        // But, when a frame rate is low, estimation of bitrate may be low
        // so it can not recover video quality.
        // If it determine the low frame rate (defined as 15fps),
        // use original bps to avoid bitrate undershoot.
        // 
        m_bitrateAdjuster->SetTargetBitrateBps(parameters.bitrate.get_sum_bps());
        uint32_t bitRate = m_bitrateAdjuster->GetAdjustedBitrateBps();
        const uint32_t kLowFrameRate = 15;
        if (frameRate < kLowFrameRate)
        {
            bitRate = parameters.bitrate.get_sum_bps();
        }


        m_setRates(m_encoderId, bitRate, frameRate);
    }

} // end namespace webrtc
} // end namespace unity
