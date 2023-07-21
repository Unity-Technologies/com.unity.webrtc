#pragma once

#include <cuda.h>

#include <common_video/h264/h264_bitstream_parser.h>
#include <common_video/include/video_frame_buffer_pool.h>

#include "NvCodec.h"
#include "NvDecoder/NvDecoder.h"

using namespace webrtc;

class UnityProfilerMarkerDesc;

namespace unity
{
namespace webrtc
{

    using NvDecoderInternal = ::NvDecoder;

    class H264BitstreamParser : public ::webrtc::H264BitstreamParser
    {
    public:
        absl::optional<SpsParser::SpsState> sps() { return sps_; }
        absl::optional<PpsParser::PpsState> pps() { return pps_; }
    };

    class ProfilerMarkerFactory;
    class NvDecoderImpl : public unity::webrtc::NvDecoder
    {
    public:
        NvDecoderImpl(CUcontext context, ProfilerMarkerFactory* profiler);
        NvDecoderImpl(const NvDecoderImpl&) = delete;
        NvDecoderImpl& operator=(const NvDecoderImpl&) = delete;
        ~NvDecoderImpl() override;

        bool Configure(const Settings& settings) override;
        int32_t Decode(const EncodedImage& input_image, bool missing_frames, int64_t render_time_ms) override;
        int32_t RegisterDecodeCompleteCallback(DecodedImageCallback* callback) override;
        int32_t Release() override;
        DecoderInfo GetDecoderInfo() const override;

    private:
        CUcontext m_context;
        std::unique_ptr<NvDecoderInternal> m_decoder;
        bool m_isConfiguredDecoder;

        Settings m_settings;

        DecodedImageCallback* m_decodedCompleteCallback = nullptr;
        webrtc::VideoFrameBufferPool m_buffer_pool;
        H264BitstreamParser m_h264_bitstream_parser;

        ProfilerMarkerFactory* m_profiler;
        const UnityProfilerMarkerDesc* m_marker;
    };

} // end namespace webrtc
} // end namespace unity
