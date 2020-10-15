#pragma once

namespace unity
{
namespace webrtc
{
    namespace webrtc = ::webrtc;

    class IVideoEncoderObserver;
    class UnityVideoEncoderFactory : public webrtc::VideoEncoderFactory
    {
    public:
        //VideoEncoderFactory
        // Returns a list of supported video formats in order of preference, to use
        // for signaling etc.
        virtual std::vector<webrtc::SdpVideoFormat> GetSupportedFormats() const override;
        // Returns information about how this format will be encoded. The specified
        // format must be one of the supported formats by this factory.
        virtual CodecInfo QueryVideoEncoder(const webrtc::SdpVideoFormat& format) const override;
        // Creates a VideoEncoder for the specified format.
        virtual std::unique_ptr<webrtc::VideoEncoder> CreateVideoEncoder(const webrtc::SdpVideoFormat& format) override;

        virtual std::vector<webrtc::SdpVideoFormat> GetHardwareEncoderFormats() const;

        UnityVideoEncoderFactory(IVideoEncoderObserver* observer);
        ~UnityVideoEncoderFactory();
    private:
        IVideoEncoderObserver* m_observer;
        std::unique_ptr<VideoEncoderFactory> internal_encoder_factory_;
    };
}
}
