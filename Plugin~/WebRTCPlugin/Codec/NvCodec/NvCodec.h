#pragma once

#include <cuda.h>
#include <vector>

#include <api/video_codecs/h264_profile_level_id.h>
#include <api/video_codecs/sdp_video_format.h>
#include <api/video_codecs/video_decoder.h>
#include <api/video_codecs/video_decoder_factory.h>
#include <api/video_codecs/video_encoder.h>
#include <api/video_codecs/video_encoder_factory.h>
#include <media/base/codec.h>

#include "nvEncodeAPI.h"

namespace unity
{
namespace webrtc
{
    using namespace ::webrtc;

    int SupportedEncoderCount(CUcontext context);
    H264Level SupportedMaxH264Level(CUcontext context);
    std::vector<SdpVideoFormat> SupportedNvEncoderCodecs(CUcontext context);
    std::vector<SdpVideoFormat> SupportedNvDecoderCodecs(CUcontext context);

    class ProfilerMarkerFactory;

    class NvEncoder : public VideoEncoder
    {
    public:
        static std::unique_ptr<NvEncoder> Create(
            const cricket::VideoCodec& codec,
            CUcontext context,
            CUmemorytype memoryType,
            NV_ENC_BUFFER_FORMAT format,
            ProfilerMarkerFactory* profiler);
        static bool IsSupported(CUcontext context);
        ~NvEncoder() override { }
    };

    class NvDecoder : public VideoDecoder
    {
    public:
        static std::unique_ptr<NvDecoder>
        Create(const cricket::VideoCodec& codec, CUcontext context, ProfilerMarkerFactory* profiler);
        static bool IsSupported();

        ~NvDecoder() override { }
    };

    class NvEncoderFactory : public VideoEncoderFactory
    {
    public:
        NvEncoderFactory(CUcontext context, NV_ENC_BUFFER_FORMAT format, ProfilerMarkerFactory* profiler);
        ~NvEncoderFactory() override;

        std::vector<SdpVideoFormat> GetSupportedFormats() const override;
        std::unique_ptr<VideoEncoder> CreateVideoEncoder(const SdpVideoFormat& format) override;

    private:
        CUcontext context_;
        NV_ENC_BUFFER_FORMAT format_;
        ProfilerMarkerFactory* profiler_;

        // Cache of capability to reduce calling SessionOpenAPI of NvEncoder
        std::vector<SdpVideoFormat> m_cachedSupportedFormats;
    };

    class NvDecoderFactory : public VideoDecoderFactory
    {
    public:
        NvDecoderFactory(CUcontext context, ProfilerMarkerFactory* profiler);
        ~NvDecoderFactory() override;

        std::vector<webrtc::SdpVideoFormat> GetSupportedFormats() const override;
        std::unique_ptr<webrtc::VideoDecoder> CreateVideoDecoder(const webrtc::SdpVideoFormat& format) override;

    private:
        CUcontext context_;
        ProfilerMarkerFactory* profiler_;
    };

#ifndef _WIN32
    static bool operator==(const GUID& a, const GUID& b) { return !std::memcmp(&a, &b, sizeof(GUID)); }
#endif

// todo(kazuki):: fix workaround
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Wunused-function"

    static absl::optional<H264Profile> GuidToProfile(GUID& guid)
    {
        if (guid == NV_ENC_H264_PROFILE_BASELINE_GUID)
            return H264Profile::kProfileBaseline;
        if (guid == NV_ENC_H264_PROFILE_MAIN_GUID)
            return H264Profile::kProfileMain;
        if (guid == NV_ENC_H264_PROFILE_HIGH_GUID)
            return H264Profile::kProfileHigh;
        if (guid == NV_ENC_H264_PROFILE_CONSTRAINED_HIGH_GUID)
            return H264Profile::kProfileConstrainedHigh;
        return absl::nullopt;
    }

    static absl::optional<GUID> ProfileToGuid(H264Profile profile)
    {
        if (profile == H264Profile::kProfileBaseline)
            return NV_ENC_H264_PROFILE_BASELINE_GUID;
        // Returns Baseline Profile instead because NVCodec is not supported Constrained Baseline Profile
        // officially, but WebRTC use the profile in default.
        if (profile == H264Profile::kProfileConstrainedBaseline)
            return NV_ENC_H264_PROFILE_BASELINE_GUID;
        if (profile == H264Profile::kProfileMain)
            return NV_ENC_H264_PROFILE_MAIN_GUID;
        if (profile == H264Profile::kProfileHigh)
            return NV_ENC_H264_PROFILE_HIGH_GUID;
        if (profile == H264Profile::kProfileConstrainedHigh)
            return NV_ENC_H264_PROFILE_CONSTRAINED_HIGH_GUID;
        return absl::nullopt;
    }
#pragma clang diagnostic pop
}
}
