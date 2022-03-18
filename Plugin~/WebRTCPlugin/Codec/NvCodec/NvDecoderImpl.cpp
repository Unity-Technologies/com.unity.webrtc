#include "pch.h"

#include "NvDecoderImpl.h"
#include "NvDecoder/NvDecoder.h"
#include "../Utils/NvCodecUtils.h"
#include "api/video/i420_buffer.h"
#include "api/video/video_codec_type.h"
#include "third_party/libyuv/include/libyuv/convert.h"

using namespace webrtc;

namespace unity
{
namespace webrtc
{
    using namespace ::webrtc;

    NvDecoderImpl::NvDecoderImpl(CUcontext context)
        : m_context(context)
        , m_decoder(nullptr)
        , m_decodedCompleteCallback(nullptr)
        , m_buffer_pool(false)
    {
    }

    NvDecoderImpl::~NvDecoderImpl() { Release(); }

    VideoDecoder::DecoderInfo NvDecoderImpl::GetDecoderInfo() const
    {
        VideoDecoder::DecoderInfo info;
        info.implementation_name = "UnityNvDecoder";
        info.is_hardware_accelerated = true;
        return info;
    }

    int NvDecoderImpl::InitDecode(const VideoCodec* codec_settings, int32_t number_of_cores)
    {
        if (codec_settings == nullptr)
        {
            RTC_LOG(LS_ERROR) << "initialization failed on codec_settings is null ";
            return WEBRTC_VIDEO_CODEC_ERR_PARAMETER;
        }

        if (codec_settings->codecType != kVideoCodecH264)
        {
            RTC_LOG(LS_ERROR) << "initialization failed on codectype is not kVideoCodecH264";
            return WEBRTC_VIDEO_CODEC_ERR_PARAMETER;
        }
        if (codec_settings->width < 1 || codec_settings->height < 1)
        {
            RTC_LOG(LS_ERROR) << "initialization failed on codec_settings width < 0 or height < 0";
            return WEBRTC_VIDEO_CODEC_ERR_PARAMETER;
        }

        m_codec = *codec_settings;

        const CUresult result = cuCtxSetCurrent(m_context);
        if (!ck(result))
        {
            RTC_LOG(LS_ERROR) << "initialization failed on cuCtxSetCurrent result" << result;
            return WEBRTC_VIDEO_CODEC_ENCODER_FAILURE;
        }

        // bUseDeviceFrame: allocate in memory or cuda device memory
        m_decoder = std::make_unique<NvDecoderInternal>(m_context, false, cudaVideoCodec_H264);
        return WEBRTC_VIDEO_CODEC_OK;
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

        int nFrameReturnd = 0;
        do
        {
            nFrameReturnd =
                m_decoder->Decode(input_image.data(), input_image.size(), CUVID_PKT_TIMESTAMP, input_image.Timestamp());
        } while (nFrameReturnd == 0);

        for (int i = 0; i < nFrameReturnd; i++)
        {
            int64_t timeStamp;
            uint8_t* pFrame = m_decoder->GetFrame(&timeStamp);

            rtc::scoped_refptr<webrtc::I420Buffer> i420_buffer =
                m_buffer_pool.CreateI420Buffer(m_decoder->GetWidth(), m_decoder->GetHeight());

            libyuv::NV12ToI420(
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

            VideoFrame decoded_frame = VideoFrame::Builder()
                                           .set_video_frame_buffer(i420_buffer)
                                           .set_timestamp_rtp(timeStamp)
                                           .build();

            // todo: measurement decoding time
            absl::optional<int32_t> decodetime;
            m_decodedCompleteCallback->Decoded(decoded_frame, decodetime, qp);
        }

        return WEBRTC_VIDEO_CODEC_OK;
    }

} // end namespace webrtc
} // end namespace unity
