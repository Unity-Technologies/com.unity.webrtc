#include "pch.h"
#include "DummyVideoEncoder.h"
#include "modules/video_coding/utility/simulcast_rate_allocator.h"

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

        m_codec = *codec_settings;
        webrtc::SimulcastRateAllocator init_allocator(m_codec);
        webrtc::VideoBitrateAllocation allocation =
            init_allocator.Allocate(webrtc::VideoBitrateAllocationParameters(
                webrtc::DataRate::KilobitsPerSec(m_codec.startBitrate), m_codec.maxFramerate));
        SetRates(RateControlParameters(allocation, m_codec.maxFramerate));

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

        m_h264BitstreamParser.ParseBitstream(m_encodedImage);
        m_encodedImage.qp_ = m_h264BitstreamParser.GetLastSliceQp().value_or(-1);

        webrtc::CodecSpecificInfo codecInfo;
        codecInfo.codecType = webrtc::kVideoCodecH264;
        codecInfo.codecSpecific.H264.packetization_mode = webrtc::H264PacketizationMode::NonInterleaved;

        const auto result = callback->OnEncodedImage(m_encodedImage, &codecInfo);
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
        int64_t frameRate = parameters.framerate_fps;

        uint32_t bitRate = parameters.bitrate.get_sum_bps();

        m_setRates(m_encoderId, bitRate, frameRate);
    }

} // end namespace webrtc
} // end namespace unity
