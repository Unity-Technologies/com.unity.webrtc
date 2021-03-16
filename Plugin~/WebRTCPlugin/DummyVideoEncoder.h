#pragma once

namespace unity
{
namespace webrtc
{
    namespace webrtc = ::webrtc;

    // todo(kazuki): this interface class is used to access hardware encoder from webrtc::VideoEncoder for controlling bitrate
    //               remove this after webrtc::Video Encoder can access hw encoder directly.
    class IVideoEncoderObserver : public sigslot::has_slots<>
    {
    public:
        virtual void SetKeyFrame(uint32_t id) = 0;
        virtual void SetRates(uint32_t id, uint32_t bitRate, int64_t frameRate) = 0;
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
        webrtc::EncodedImage m_encodedImage;
        webrtc::H264BitstreamParser m_h264BitstreamParser;
        webrtc::VideoCodec m_codec;

        webrtc::RateStatistics m_encode_fps;
        webrtc::Clock* m_clock;
        std::unique_ptr<webrtc::BitrateAdjuster> m_bitrateAdjuster;

        // todo(kazuki): this member is for identify video encoder instance (IEncoder implemented).
        uint32_t m_encoderId = 0;

        // todo(kazuki): remove these signals when moving hardware encoder instance to this class
        sigslot::signal1<uint32_t> m_setKeyFrame;
        sigslot::signal3<uint32_t, uint32_t, int64_t> m_setRates;
    };

    // todo::(kazuki)
    class FrameBuffer : public webrtc::VideoFrameBuffer
    {
    public:
        FrameBuffer(int width,
            int height,
            std::vector<uint8>& data,
            const int encoderId)
            : m_frameWidth(width),
            m_frameHeight(height),
            m_encoderId(encoderId),
            m_buffer(data)
        {}

        //webrtc::VideoFrameBuffer pure virtual functions
        // This function specifies in what pixel format the data is stored in.
        virtual Type type() const override
        {
            //fake I420 to avoid ToI420() being called
            return Type::kI420;
        }
        // The resolution of the frame in pixels. For formats where some planes are
        // subsampled, this is the highest-resolution plane.
        virtual int width() const override
        {
            return m_frameWidth;
        }
        virtual int height() const override
        {
            return m_frameHeight;
        }

        std::vector<uint8>& buffer() const
        {
            return m_buffer;
        }

        // todo(kazuki): remove the method by refactoring video encoding.
        // The id is for identifying encoder which encoded this frame.
        int encoderId() const
        {
            return m_encoderId;
        }

        // Returns a memory-backed frame buffer in I420 format. If the pixel data is
        // in another format, a conversion will take place. All implementations must
        // provide a fallback to I420 for compatibility with e.g. the internal WebRTC
        // software encoders.
        virtual rtc::scoped_refptr<webrtc::I420BufferInterface> ToI420() override
        {
            return nullptr;
        }

    private:
        int m_frameWidth;
        int m_frameHeight;
        int m_encoderId;
        std::vector<uint8>& m_buffer;
    };
} // end namespace webrtc
} // end namespace unity
