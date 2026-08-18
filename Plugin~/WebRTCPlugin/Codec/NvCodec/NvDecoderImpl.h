#pragma once

#include <cuda.h>

#include <common_video/h264/h264_bitstream_parser.h>
#include <common_video/h265/h265_bitstream_parser.h>
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

    class H264BitstreamParser final : public ::webrtc::H264BitstreamParser
    {
    public:
        std::optional<SpsParser::SpsState> sps() { return sps_; }
        std::optional<PpsParser::PpsState> pps() { return pps_; }
    };

    class H265BitstreamParser final : public ::webrtc::H265BitstreamParser
    {
    public:
        std::optional<H265SpsParser::SpsState> sps(const uint32_t id = 0)
        {
            if (H265SpsParser::SpsState const* sps_state = GetSPS(id))
            {
                return *sps_state;
            }
            return std::nullopt;
        }
        std::optional<H265PpsParser::PpsState> pps(const uint32_t id = 0)
        {
            if (H265PpsParser::PpsState const* pps_state = GetPPS(id))
            {
                return *pps_state;
            }
            return std::nullopt;
        }
    };

    class ProfilerMarkerFactory;
    class NvDecoderImplH264 : public unity::webrtc::NvDecoder
    {
    public:
        NvDecoderImplH264(CUcontext context, ProfilerMarkerFactory* profiler);
        NvDecoderImplH264(const NvDecoderImplH264&) = delete;
        NvDecoderImplH264& operator=(const NvDecoderImplH264&) = delete;
        ~NvDecoderImplH264() override;

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

    class NvDecoderImplH265 : public unity::webrtc::NvDecoder
    {
    public:
        NvDecoderImplH265(CUcontext context, ProfilerMarkerFactory* profiler);
        NvDecoderImplH265(const NvDecoderImplH265&) = delete;
        NvDecoderImplH265& operator=(const NvDecoderImplH265&) = delete;
        ~NvDecoderImplH265() override;

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
        H265BitstreamParser m_h265_bitstream_parser;

        ProfilerMarkerFactory* m_profiler;
        const UnityProfilerMarkerDesc* m_marker;
    };

} // end namespace webrtc
} // end namespace unity
