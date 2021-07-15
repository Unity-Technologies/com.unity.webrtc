#pragma once

namespace unity
{
namespace webrtc
{
    namespace webrtc = ::webrtc;

    // This class is only used for status testing.
    class UnityVideoDecoderFactory : public webrtc::VideoDecoderFactory
    {
    public:
        std::vector<webrtc::SdpVideoFormat> GetSupportedFormats() const override;
        std::unique_ptr<webrtc::VideoDecoder> CreateVideoDecoder(const webrtc::SdpVideoFormat& format) override;
        UnityVideoDecoderFactory(bool forTest);
    private:
        const std::unique_ptr<VideoDecoderFactory> internal_decoder_factory_;
        bool forTest_;
    };
}
}
