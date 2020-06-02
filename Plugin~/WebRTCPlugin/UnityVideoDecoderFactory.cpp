#include "pch.h"
#include "UnityVideoDecoderFactory.h"

namespace unity
{
namespace webrtc
{
    UnityVideoDecoderFactory::UnityVideoDecoderFactory(): internal_decoder_factory_(new webrtc::InternalDecoderFactory())
    {
    }

    std::vector<webrtc::SdpVideoFormat> UnityVideoDecoderFactory::GetSupportedFormats() const
    {
        return { webrtc::CreateH264Format(webrtc::H264::kProfileConstrainedBaseline, webrtc::H264::kLevel5_1, "1") };
    }

    std::unique_ptr<webrtc::VideoDecoder> UnityVideoDecoderFactory::CreateVideoDecoder(const webrtc::SdpVideoFormat & format)
    {
        return internal_decoder_factory_->CreateVideoDecoder(format);
    }

}  // namespace webrtc
}  // namespace unity
