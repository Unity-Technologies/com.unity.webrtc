#include "pch.h"
#include "UnityVideoDecoderFactory.h"
#include "absl/strings/match.h"

#if defined(__APPLE__)
#import "sdk/objc/components/video_codec/RTCDefaultVideoDecoderFactory.h"
#import "sdk/objc/native/api/video_decoder_factory.h"
#endif

namespace unity
{
namespace webrtc
{
    webrtc::VideoDecoderFactory* CreateDecoderFactory()
    {
#if defined(__APPLE__)
        return webrtc::ObjCToNativeVideoDecoderFactory(
            [[RTCDefaultVideoDecoderFactory alloc] init]).release();
#endif
        return new webrtc::InternalDecoderFactory();
    }

    UnityVideoDecoderFactory::UnityVideoDecoderFactory()
    : internal_decoder_factory_(CreateDecoderFactory())
    {
    }

    std::vector<webrtc::SdpVideoFormat> UnityVideoDecoderFactory::GetSupportedFormats() const
    {
        return internal_decoder_factory_->GetSupportedFormats();
    }

    std::unique_ptr<webrtc::VideoDecoder> UnityVideoDecoderFactory::CreateVideoDecoder(const webrtc::SdpVideoFormat & format)
    {
        return internal_decoder_factory_->CreateVideoDecoder(format);
    }

}  // namespace webrtc
}  // namespace unity
