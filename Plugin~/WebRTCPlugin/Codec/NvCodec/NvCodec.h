#pragma once
#include "api/video_codecs/h264_profile_level_id.h"
#include "api/video_codecs/sdp_video_format.h"
#include "api/video_codecs/video_decoder.h"
#include "api/video_codecs/video_encoder.h"
#include "media/base/codec.h"
#include "nvEncodeAPI.h"
#include <cuda.h>
#include <vector>

namespace unity
{
namespace webrtc
{
    using namespace ::webrtc;

    int SupportedEncoderCount(CUcontext context);
    std::vector<SdpVideoFormat> SupportedNvEncoderCodecs(CUcontext context);
    std::vector<SdpVideoFormat> SupportedNvDecoderCodecs(CUcontext context);

    class NvEncoder : public VideoEncoder
    {
    public:
        static std::unique_ptr<NvEncoder> Create(
            const cricket::VideoCodec& codec,
            CUcontext context,
            CUmemorytype memoryType,
            NV_ENC_BUFFER_FORMAT format);
        // If H.264 is supported (any implementation).
        static bool IsSupported();
        static bool SupportsScalabilityMode(absl::string_view scalability_mode);

        ~NvEncoder() override { }
    };

    class NvDecoder : public VideoDecoder
    {
    public:
        static std::unique_ptr<NvDecoder> Create(const cricket::VideoCodec& codec, CUcontext context);
        static bool IsSupported();

        ~NvDecoder() override { }
    };

    class NvEncoderFactory : public VideoEncoderFactory
    {
    public:
        NvEncoderFactory(CUcontext context, NV_ENC_BUFFER_FORMAT format);
        ~NvEncoderFactory();

        std::vector<SdpVideoFormat> GetSupportedFormats() const override;
        VideoEncoderFactory::CodecInfo QueryVideoEncoder(const SdpVideoFormat& format) const override;
        std::unique_ptr<VideoEncoder> CreateVideoEncoder(const SdpVideoFormat& format) override;

    private:
        CUcontext context_;
        NV_ENC_BUFFER_FORMAT format_;
    };

    class NvDecoderFactory : public VideoDecoderFactory
    {
    public:
        NvDecoderFactory(CUcontext context);
        ~NvDecoderFactory();

        std::vector<webrtc::SdpVideoFormat> GetSupportedFormats() const override;
        std::unique_ptr<webrtc::VideoDecoder> CreateVideoDecoder(const webrtc::SdpVideoFormat& format) override;

    private:
        CUcontext context_;
    };

#ifndef _WIN32
    constexpr bool operator==(const GUID& a, const GUID& b)
    {
        return a.Data1 == b.Data1 && a.Data2 == b.Data2 && a.Data3 == b.Data3 && std::memcmp(a.Data4, b.Data4, 8) == 0;
    }
#endif

    constexpr absl::optional<H264Profile> GuidToProfile(GUID& guid)
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

    constexpr absl::optional<GUID> ProfileToGuid(H264Profile profile)
    {
        if (profile == H264Profile::kProfileBaseline)
            return NV_ENC_H264_PROFILE_BASELINE_GUID;
        if (profile == H264Profile::kProfileMain)
            return NV_ENC_H264_PROFILE_MAIN_GUID;
        if (profile == H264Profile::kProfileHigh)
            return NV_ENC_H264_PROFILE_HIGH_GUID;
        if (profile == H264Profile::kProfileConstrainedHigh)
            return NV_ENC_H264_PROFILE_CONSTRAINED_HIGH_GUID;
        return absl::nullopt;
    }
}
}
