#pragma once

#include <api/video_codecs/sdp_video_format.h>
#include <api/video_codecs/video_decoder_factory.h>
#include <api/video_codecs/video_encoder_factory.h>

namespace unity
{
namespace webrtc
{
    using namespace ::webrtc;

    class IGraphicsDevice;
    class ProfilerMarkerFactory;
    class UnityVideoDecoderFactory : public VideoDecoderFactory
    {
    public:
        std::vector<webrtc::SdpVideoFormat> GetSupportedFormats() const override;
        std::unique_ptr<webrtc::VideoDecoder> CreateVideoDecoder(const webrtc::SdpVideoFormat& format) override;

        UnityVideoDecoderFactory(IGraphicsDevice* gfxDevice, ProfilerMarkerFactory* profiler);
        ~UnityVideoDecoderFactory() override;

        // workaround:
        //
        bool IsAvailableFormat(const SdpVideoFormat& format);

    private:
        ProfilerMarkerFactory* profiler_;
        std::map<std::string, std::unique_ptr<VideoEncoderFactory>> encoderFactories_;
        std::map<std::string, std::unique_ptr<VideoDecoderFactory>> decoderFactories_;
    };
}
}
