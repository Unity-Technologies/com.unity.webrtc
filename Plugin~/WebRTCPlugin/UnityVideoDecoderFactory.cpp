#include "pch.h"
#include "UnityVideoDecoderFactory.h"
#include "absl/strings/match.h"

namespace unity
{
namespace webrtc
{
    UnityVideoDecoderFactory::UnityVideoDecoderFactory() : internal_decoder_factory_(new webrtc::InternalDecoderFactory())
    {
    }

    std::vector<webrtc::SdpVideoFormat> UnityVideoDecoderFactory::GetSupportedFormats() const
    {
        std::vector<webrtc::SdpVideoFormat> formats;

        formats.push_back(webrtc::SdpVideoFormat(cricket::kVp8CodecName));
        
        for (const webrtc::SdpVideoFormat& format : webrtc::SupportedVP9Codecs())
        {
            formats.push_back(format);
        }

        return formats;
    }

    std::unique_ptr<webrtc::VideoDecoder> UnityVideoDecoderFactory::CreateVideoDecoder(const webrtc::SdpVideoFormat & format)
    {
        if (absl::EqualsIgnoreCase(format.name, cricket::kVp8CodecName))
        {
            return webrtc::VP8Decoder::Create();
        }

        if (absl::EqualsIgnoreCase(format.name, cricket::kVp9CodecName))
        {
            return webrtc::VP9Decoder::Create();
        }

        return nullptr;
    }

}  // namespace webrtc
}  // namespace unity
