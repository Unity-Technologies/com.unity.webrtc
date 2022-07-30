#pragma once

#include <api/video_codecs/h264_profile_level_id.h>
#include <api/video_codecs/sdp_video_format.h>
#include <api/video_codecs/video_decoder.h>
#include <api/video_codecs/video_decoder_factory.h>
#include <api/video_codecs/video_encoder.h>
#include <api/video_codecs/video_encoder_factory.h>
#include <media/base/codec.h>

namespace unity
{
namespace webrtc
{
    using namespace ::webrtc;

    class ProfilerMarkerFactory;

    class MediaCodecEncoder : public VideoEncoder
    {
    public:
        static std::unique_ptr<MediaCodecEncoder> Create(
            const cricket::VideoCodec& codec,
            ProfilerMarkerFactory* profiler);
    };

    class MediaCodecDecoder : public VideoDecoder
    {
    public:
        static std::unique_ptr<MediaCodecDecoder>
        Create(const cricket::VideoCodec& codec, ProfilerMarkerFactory* profiler);
    };

    class MedicCodecEncoderFactory : public VideoEncoderFactory
    {
    public:
        MedicCodecEncoderFactory(ProfilerMarkerFactory* profiler);
        ~MedicCodecEncoderFactory() override;

        std::vector<SdpVideoFormat> GetSupportedFormats() const override;
        VideoEncoderFactory::CodecInfo QueryVideoEncoder(const SdpVideoFormat& format) const override;
        std::unique_ptr<VideoEncoder> CreateVideoEncoder(const SdpVideoFormat& format) override;

    private:
        ProfilerMarkerFactory* profiler_;

        // Cache of capability to reduce calling SessionOpenAPI of NvEncoder
        std::vector<SdpVideoFormat> m_cachedSupportedFormats;
    };

    class MedicCodecDecoderFactory : public VideoDecoderFactory
    {
    public:
        MedicCodecDecoderFactory(ProfilerMarkerFactory* profiler);
        ~MedicCodecDecoderFactory() override;

        std::vector<webrtc::SdpVideoFormat> GetSupportedFormats() const override;
        std::unique_ptr<webrtc::VideoDecoder> CreateVideoDecoder(const webrtc::SdpVideoFormat& format) override;

    private:
        ProfilerMarkerFactory* profiler_;
    };
}
}