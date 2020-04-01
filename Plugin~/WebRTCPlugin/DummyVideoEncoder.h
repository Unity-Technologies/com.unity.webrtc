#pragma once

namespace unity
{
namespace webrtc
{

    namespace webrtc = ::webrtc;

    class IVideoEncoderObserver : public sigslot::has_slots<>
    {
    public:
        virtual void SetKeyFrame(uint32_t id) = 0;
        virtual void SetRates(uint32_t id, const webrtc::VideoEncoder::RateControlParameters& parameters) = 0;
    };

    class DummyVideoEncoder : public webrtc::VideoEncoder
    {
    public:
        DummyVideoEncoder(IVideoEncoderObserver* observer);

        // webrtc::VideoEncoder
        // Initialize the encoder with the information from the codecSettings
        virtual int32_t InitEncode(const webrtc::VideoCodec* codec_settings, int32_t number_of_cores, size_t max_payload_size) override;
        // Register an encode complete callback object.
        virtual int32_t RegisterEncodeCompleteCallback(webrtc::EncodedImageCallback* callback) override;
        // Free encoder memory.
        virtual int32_t Release() override;
        // Encode an I420 image (as a part of a video stream). The encoded image
        // will be returned to the user through the encode complete callback.
        virtual int32_t Encode(const webrtc::VideoFrame& frame, const std::vector<webrtc::VideoFrameType>* frame_types) override;
        // Default fallback: Just use the sum of bitrates as the single target rate.
        virtual void SetRates(const RateControlParameters& parameters) override;
    private:
        webrtc::EncodedImageCallback* callback = nullptr;
        webrtc::EncodedImage encodedImage;
        webrtc::RTPFragmentationHeader fragHeader;
        webrtc::H264BitstreamParser m_h264BitstreamParser;

        uint32_t m_encoderId = 0;
        sigslot::signal1<uint32_t> m_setKeyFrame;
        sigslot::signal2<uint32_t, const RateControlParameters&> m_setRates;
        std::unique_ptr<webrtc::BitrateAdjuster>  m_bitrateAdjuster;
    };

} // end namespace webrtc
} // end namespace unity
