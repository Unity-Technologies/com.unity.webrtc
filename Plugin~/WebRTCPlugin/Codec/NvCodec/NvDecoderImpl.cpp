#include "pch.h"

#include <api/video/i420_buffer.h>
#include <api/video/video_codec_type.h>
#include <modules/video_coding/include/video_error_codes.h>
#include <third_party/libyuv/include/libyuv/convert.h>

#include "NvCodecUtils.h"
#include "NvDecoder/NvDecoder.h"
#include "NvDecoderImpl.h"
#include "ProfilerMarkerFactory.h"
#include "ScopedProfiler.h"

namespace unity
{
namespace webrtc
{
    using namespace ::webrtc;

    ColorSpace ExtractH264ColorSpace(const CUVIDEOFORMAT& format)
    {
        return ColorSpace(
            static_cast<ColorSpace::PrimaryID>(format.video_signal_description.color_primaries),
            static_cast<ColorSpace::TransferID>(format.video_signal_description.transfer_characteristics),
            static_cast<ColorSpace::MatrixID>(format.video_signal_description.matrix_coefficients),
            static_cast<ColorSpace::RangeID>(format.video_signal_description.video_full_range_flag));
    }

    NvDecoderImpl::NvDecoderImpl(CUcontext context, ProfilerMarkerFactory* profiler)
        : m_context(context)
        , m_decoder(nullptr)
        , m_isConfiguredDecoder(false)
        , m_decodedCompleteCallback(nullptr)
        , m_buffer_pool(false)
        , m_profiler(profiler)
    {
        if (profiler)
            m_marker = profiler->CreateMarker(
                "NvDecoderImpl.ConvertNV12ToI420", kUnityProfilerCategoryOther, kUnityProfilerMarkerFlagDefault, 0);
    }

    NvDecoderImpl::~NvDecoderImpl() { Release(); }

    VideoDecoder::DecoderInfo NvDecoderImpl::GetDecoderInfo() const
    {
        VideoDecoder::DecoderInfo info;
        info.implementation_name = "NvCodec";
        info.is_hardware_accelerated = true;
        return info;
    }

    bool NvDecoderImpl::Configure(const Settings& settings)
    {
        if (settings.codec_type() != kVideoCodecH264)
        {
            RTC_LOG(LS_ERROR) << "initialization failed on codectype is not kVideoCodecH264";
            return false;
        }
        if (!settings.max_render_resolution().Valid())
        {
            RTC_LOG(LS_ERROR) << "initialization failed on codec_settings width < 0 or height < 0";
            return false;
        }

        m_settings = settings;

        const CUresult result = cuCtxSetCurrent(m_context);
        if (!ck(result))
        {
            RTC_LOG(LS_ERROR) << "initialization failed on cuCtxSetCurrent result" << result;
            return false;
        }

        // todo(kazuki): Max resolution is differred each architecture.
        // Refer to the table in Video Decoder Capabilities.
        // https://docs.nvidia.com/video-technologies/video-codec-sdk/nvdec-video-decoder-api-prog-guide
        int maxWidth = 4096;
        int maxHeight = 4096;

        // bUseDeviceFrame: allocate in memory or cuda device memory
        m_decoder = std::make_unique<NvDecoderInternal>(
            m_context, false, cudaVideoCodec_H264, true, false, nullptr, nullptr, false, maxWidth, maxHeight);
        return true;
    }

    int32_t NvDecoderImpl::RegisterDecodeCompleteCallback(DecodedImageCallback* callback)
    {
        this->m_decodedCompleteCallback = callback;
        return WEBRTC_VIDEO_CODEC_OK;
    }

    int32_t NvDecoderImpl::Release()
    {
        m_buffer_pool.Release();
        return WEBRTC_VIDEO_CODEC_OK;
    }

    int32_t NvDecoderImpl::Decode(const EncodedImage& input_image, bool missing_frames, int64_t render_time_ms)
    {
        CUcontext current;
        if (!ck(cuCtxGetCurrent(&current)))
        {
            RTC_LOG(LS_ERROR) << "decode failed on cuCtxGetCurrent is failed";
            return WEBRTC_VIDEO_CODEC_UNINITIALIZED;
        }
        if (current != m_context)
        {
            RTC_LOG(LS_ERROR) << "decode failed on not match current context and hold context";
            return WEBRTC_VIDEO_CODEC_UNINITIALIZED;
        }
        if (m_decodedCompleteCallback == nullptr)
        {
            RTC_LOG(LS_ERROR) << "decode failed on not set m_decodedCompleteCallback";
            return WEBRTC_VIDEO_CODEC_UNINITIALIZED;
        }
        if (!input_image.data() || !input_image.size())
        {
            RTC_LOG(LS_ERROR) << "decode failed on input image is null";
            return WEBRTC_VIDEO_CODEC_ERR_PARAMETER;
        }

        m_h264_bitstream_parser.ParseBitstream(input_image);
        absl::optional<int> qp = m_h264_bitstream_parser.GetLastSliceQp();
        absl::optional<SpsParser::SpsState> sps = m_h264_bitstream_parser.sps();

        if (m_isConfiguredDecoder)
        {
            if (!sps || sps.value().width != static_cast<uint32_t>(m_decoder->GetWidth()) ||
                sps.value().height != static_cast<uint32_t>(m_decoder->GetHeight()))
            {
                m_decoder->setReconfigParams(nullptr, nullptr);
            }
        }

        int nFrameReturnd = 0;
        do
        {
            nFrameReturnd = m_decoder->Decode(
                input_image.data(), static_cast<int>(input_image.size()), CUVID_PKT_TIMESTAMP, input_image.Timestamp());
        } while (nFrameReturnd == 0);

        m_isConfiguredDecoder = true;

        // todo: support other output format
        // Chromium's H264 Encoder is output on NV12, so currently only NV12 is supported.
        if (m_decoder->GetOutputFormat() != cudaVideoSurfaceFormat_NV12)
        {
            RTC_LOG(LS_ERROR) << "not supported this format: " << m_decoder->GetOutputFormat();
            return WEBRTC_VIDEO_CODEC_ERR_PARAMETER;
        }

        // Pass on color space from input frame if explicitly specified.
        const ColorSpace& color_space = input_image.ColorSpace()
            ? *input_image.ColorSpace()
            : ExtractH264ColorSpace(m_decoder->GetVideoFormatInfo());

        for (int i = 0; i < nFrameReturnd; i++)
        {
            int64_t timeStamp;
            uint8_t* pFrame = m_decoder->GetFrame(&timeStamp);

            rtc::scoped_refptr<webrtc::I420Buffer> i420_buffer =
                m_buffer_pool.CreateI420Buffer(m_decoder->GetWidth(), m_decoder->GetHeight());

            int result;
            {
                std::unique_ptr<const ScopedProfiler> profiler;
                if (m_profiler)
                    profiler = m_profiler->CreateScopedProfiler(*m_marker);

                result = libyuv::NV12ToI420(
                    pFrame,
                    m_decoder->GetDeviceFramePitch(),
                    pFrame + m_decoder->GetHeight() * m_decoder->GetDeviceFramePitch(),
                    m_decoder->GetDeviceFramePitch(),
                    i420_buffer->MutableDataY(),
                    i420_buffer->StrideY(),
                    i420_buffer->MutableDataU(),
                    i420_buffer->StrideU(),
                    i420_buffer->MutableDataV(),
                    i420_buffer->StrideV(),
                    m_decoder->GetWidth(),
                    m_decoder->GetHeight());
            }

            if (result)
            {
                RTC_LOG(LS_INFO) << "libyuv::NV12ToI420 failed. error:" << result;
            }

            VideoFrame decoded_frame = VideoFrame::Builder()
                                           .set_video_frame_buffer(i420_buffer)
                                           .set_timestamp_rtp(static_cast<uint32_t>(timeStamp))
                                           .set_color_space(color_space)
                                           .build();

            // todo: measurement decoding time
            absl::optional<int32_t> decodetime;
            m_decodedCompleteCallback->Decoded(decoded_frame, decodetime, qp);
        }

        return WEBRTC_VIDEO_CODEC_OK;
    }

} // end namespace webrtc
} // end namespace unity
