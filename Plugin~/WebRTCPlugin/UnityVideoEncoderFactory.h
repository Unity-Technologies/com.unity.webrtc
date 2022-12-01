#pragma once

#include <api/video_codecs/sdp_video_format.h>
#include <api/video_codecs/video_encoder_factory.h>

namespace unity
{
namespace webrtc
{
    using namespace ::webrtc;

    class IGraphicsDevice;
    class ProfilerMarkerFactory;
    class UnityVideoEncoderFactory : public VideoEncoderFactory
    {
    public:
        // VideoEncoderFactory
        // Returns a list of supported video formats in order of preference, to use
        // for signaling etc.
        std::vector<SdpVideoFormat> GetSupportedFormats() const override;
        // Returns information about how this format will be encoded. The specified
        // format must be one of the supported formats by this factory.
        CodecSupport
        QueryCodecSupport(const SdpVideoFormat& format, absl::optional<std::string> scalability_mode) const override;
        // Creates a VideoEncoder for the specified format.
        std::unique_ptr<VideoEncoder> CreateVideoEncoder(const SdpVideoFormat& format) override;

        UnityVideoEncoderFactory(IGraphicsDevice* gfxDevice, ProfilerMarkerFactory* profiler);
        ~UnityVideoEncoderFactory() override;

    private:
        ProfilerMarkerFactory* profiler_;
        std::map<std::string, std::unique_ptr<VideoEncoderFactory>> factories_;
    };
}
}
