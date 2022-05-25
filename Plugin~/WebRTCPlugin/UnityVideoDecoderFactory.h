#pragma once

#include <api/video_codecs/sdp_video_format.h>
#include <api/video_codecs/video_decoder_factory.h>

namespace unity
{
namespace webrtc
{
    class IGraphicsDevice;
    using namespace ::webrtc;

    // This class is only used for status testing.
    class UnityVideoDecoderFactory : public VideoDecoderFactory
    {
    public:
        virtual std::vector<webrtc::SdpVideoFormat> GetSupportedFormats() const override;
        virtual std::unique_ptr<webrtc::VideoDecoder> CreateVideoDecoder(const webrtc::SdpVideoFormat& format) override;

        UnityVideoDecoderFactory(IGraphicsDevice* gfxDevice);
        ~UnityVideoDecoderFactory() override;

    private:
        const std::unique_ptr<VideoDecoderFactory> internal_decoder_factory_;
        const std::unique_ptr<VideoDecoderFactory> native_decoder_factory_;
    };
}
}
