#pragma once
#include "absl/strings/match.h"

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
        UnityVideoDecoderFactory();
    private:
        const std::unique_ptr<VideoDecoderFactory> internal_decoder_factory_;
    };
}
}
