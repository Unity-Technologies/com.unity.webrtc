#include "pch.h"

#include "NvCodecUtils.h"
#include "Codec/NvCodec/NvEncoderCudaWithCUarray.h"
#include "GraphicsDevice/Cuda/GpuMemoryBufferCudaHandle.h"
#include "NvEncoder/NvEncoder.h"
#include "NvEncoder/NvEncoderCuda.h"
#include "NvEncoderImpl.h"
#include "UnityVideoTrackSource.h"

#include "absl/strings/match.h"
#include "api/video/video_codec_type.h"
#include "api/video_codecs/h264_profile_level_id.h"
#include "media/base/media_constants.h"

namespace unity
{
namespace webrtc
{
    NvEncoderImpl::NvEncoderImpl(
        const cricket::VideoCodec& codec, CUcontext context, CUmemorytype memoryType, NV_ENC_BUFFER_FORMAT format)
        : m_context(context)
        , m_memoryType(memoryType)
        , m_encoder(nullptr)
        , m_format(format)
        , m_encodedCompleteCallback(nullptr)
        , m_encode_fps(1000, 1000)
        , m_clock(Clock::GetRealTimeClock())
    {
        RTC_CHECK(absl::EqualsIgnoreCase(codec.name, cricket::kH264CodecName));
        // not implemented for host memory
        RTC_CHECK_NE(memoryType, CU_MEMORYTYPE_HOST);
        std::string profileLevelIdString;
        RTC_CHECK(codec.GetParam(cricket::kH264FmtpProfileLevelId, &profileLevelIdString));

        auto profileLevelId = ParseH264ProfileLevelId(profileLevelIdString.c_str());
        m_profileGuid = ProfileToGuid(profileLevelId.value().profile).value();
        m_level = static_cast<NV_ENC_LEVEL>(profileLevelId.value().level);
    }

    NvEncoderImpl::~NvEncoderImpl() { Release(); }

    int NvEncoderImpl::InitEncode(const VideoCodec* codec, const VideoEncoder::Settings& settings)
    {
        if (!codec || codec->codecType != kVideoCodecH264)
        {
            return WEBRTC_VIDEO_CODEC_ERR_PARAMETER;
        }
        if (codec->maxFramerate == 0)
        {
            return WEBRTC_VIDEO_CODEC_ERR_PARAMETER;
        }
        if (codec->width < 1 || codec->height < 1)
        {
            return WEBRTC_VIDEO_CODEC_ERR_PARAMETER;
        }

        int32_t ret = Release();
        if (ret != WEBRTC_VIDEO_CODEC_OK)
        {
            return ret;
        }

        m_codec = *codec;

        const CUresult result = cuCtxSetCurrent(m_context);
        if (!ck(result))
        {
            return WEBRTC_VIDEO_CODEC_ENCODER_FAILURE;
        }

        if (m_memoryType == CU_MEMORYTYPE_DEVICE)
        {
            m_encoder = std::make_unique<NvEncoderCuda>(m_context, codec->width, codec->height, m_format, 0);
        }
        else if (m_memoryType == CU_MEMORYTYPE_ARRAY)
        {
            m_encoder = std::make_unique<NvEncoderCudaWithCUarray>(m_context, codec->width, codec->height, m_format, 0);
        }
        else
        {
            RTC_CHECK_NOTREACHED();
        }

        m_bitrateAdjuster = std::make_unique<BitrateAdjuster>(0.5f, 0.95f);

        m_initializeParams.version = NV_ENC_INITIALIZE_PARAMS_VER;
        m_encodeConfig.version = NV_ENC_CONFIG_VER;
        m_initializeParams.encodeConfig = &m_encodeConfig;

        GUID encodeGuid = NV_ENC_CODEC_H264_GUID;
        GUID presetGuid = NV_ENC_PRESET_P4_GUID;

        m_encoder->CreateDefaultEncoderParams(
            &m_initializeParams, encodeGuid, presetGuid, NV_ENC_TUNING_INFO_ULTRA_LOW_LATENCY);

        // todo(kazuki): Failed CreateEncoder method when maxFramerate is high.
        // Should calculate max framerate using the table of H264 profile level.
        // m_initializeParams.frameRateNum = std::min(m_codec.maxFramerate, 30u);

        m_encodeConfig.profileGUID = m_profileGuid;
        // m_encodeConfig.gopLength = NVENC_INFINITE_GOPLENGTH;
        // m_encodeConfig.frameIntervalP = 1;
        m_encodeConfig.encodeCodecConfig.h264Config.level = m_level;
        // m_encodeConfig.encodeCodecConfig.h264Config.idrPeriod = m_encodeConfig.gopLength;
        m_encodeConfig.rcParams.version = NV_ENC_RC_PARAMS_VER;
        m_encodeConfig.rcParams.rateControlMode = NV_ENC_PARAMS_RC_CBR;
        m_encodeConfig.rcParams.averageBitRate = m_bitrateAdjuster->GetAdjustedBitrateBps();
        m_encodeConfig.rcParams.vbvBufferSize = (m_encodeConfig.rcParams.averageBitRate *
                                                 m_initializeParams.frameRateDen / m_initializeParams.frameRateNum) * 5;
        m_encodeConfig.rcParams.maxBitRate = m_encodeConfig.rcParams.averageBitRate;
        m_encodeConfig.rcParams.vbvInitialDelay = m_encodeConfig.rcParams.vbvBufferSize;
        m_encoder->CreateEncoder(&m_initializeParams);

        return WEBRTC_VIDEO_CODEC_OK;
    }

    int32_t NvEncoderImpl::RegisterEncodeCompleteCallback(EncodedImageCallback* callback)
    {
        m_encodedCompleteCallback = callback;
        return WEBRTC_VIDEO_CODEC_OK;
    }

    int32_t NvEncoderImpl::Release()
    {
        if (m_encoder)
        {
            m_encoder->DestroyEncoder();
            m_encoder = nullptr;
        }
        return WEBRTC_VIDEO_CODEC_OK;
    }

    void NvEncoderImpl::CopyResource(
        const NvEncInputFrame* encoderInputFrame,
        GpuMemoryBufferInterface* buffer,
        Size& size,
        CUcontext context,
        CUmemorytype memoryType)
    {
        const GpuMemoryBufferCudaHandle* handle = static_cast<const GpuMemoryBufferCudaHandle*>(buffer->handle());

        if (memoryType == CU_MEMORYTYPE_DEVICE)
        {
            NvEncoderCuda::CopyToDeviceFrame(
                context,
                reinterpret_cast<void*>(handle->devicePtr),
                0,
                reinterpret_cast<CUdeviceptr>(encoderInputFrame->inputPtr),
                encoderInputFrame->pitch,
                size.width(),
                size.height(),
                CU_MEMORYTYPE_DEVICE,
                encoderInputFrame->bufferFormat,
                encoderInputFrame->chromaOffsets,
                encoderInputFrame->numChromaPlanes);
        }
        else if (memoryType == CU_MEMORYTYPE_ARRAY)
        {
            NvEncoderCudaWithCUarray::CopyToDeviceFrame(
                context,
                static_cast<void*>(handle->mappedArray),
                0,
                static_cast<CUarray>(encoderInputFrame->inputPtr),
                encoderInputFrame->pitch,
                size.width(),
                size.height(),
                CU_MEMORYTYPE_ARRAY,
                encoderInputFrame->bufferFormat,
                encoderInputFrame->chromaOffsets,
                encoderInputFrame->numChromaPlanes);
        }
    }

    int32_t NvEncoderImpl::Encode(const ::webrtc::VideoFrame& frame, const std::vector<VideoFrameType>* frameTypes)
    {
        RTC_DCHECK_EQ(frame.width(), m_codec.width);
        RTC_DCHECK_EQ(frame.height(), m_codec.height);

        if (!m_encoder)
            return WEBRTC_VIDEO_CODEC_UNINITIALIZED;
        if (!m_encodedCompleteCallback)
            return WEBRTC_VIDEO_CODEC_UNINITIALIZED;

        rtc::scoped_refptr<VideoFrame> video_frame =
            static_cast<VideoFrameAdapter*>(frame.video_frame_buffer().get())->GetVideoFrame();
        if (!video_frame)
        {
            return WEBRTC_VIDEO_CODEC_ENCODER_FAILURE;
        }

        bool send_key_frame = false;
        if (m_keyframeRequest)
            send_key_frame = true;

        if (!send_key_frame && frameTypes)
        {
            if ((*frameTypes)[0] == VideoFrameType::kVideoFrameKey)
            {
                send_key_frame = true;
            }
        }

        Size size = video_frame->size();
        RTC_DCHECK_EQ(m_encoder->GetEncodeWidth(), size.width());
        RTC_DCHECK_EQ(m_encoder->GetEncodeHeight(), size.height());

        const NvEncInputFrame* encoderInputFrame = m_encoder->GetNextInputFrame();

        // Copy CUDA buffer in VideoFrame to encoderInputFrame.
        auto buffer = video_frame->GetGpuMemoryBuffer();
        CopyResource(encoderInputFrame, buffer, size, m_context, m_memoryType);

        NV_ENC_PIC_PARAMS picParams = NV_ENC_PIC_PARAMS();
        picParams.version = NV_ENC_PIC_PARAMS_VER;
        picParams.encodePicFlags = 0;
        if (send_key_frame)
        {
            picParams.encodePicFlags = NV_ENC_PIC_FLAG_FORCEINTRA | NV_ENC_PIC_FLAG_FORCEIDR;
            m_keyframeRequest = false;
        }

        std::vector<std::vector<uint8_t>> vPacket;
        m_encoder->EncodeFrame(vPacket, &picParams);

        for (std::vector<uint8_t>& packet : vPacket)
        {
            int32_t result = ProcessEncodedFrame(packet, frame);
            if (result != WEBRTC_VIDEO_CODEC_OK)
            {
                return result;
            }
            m_bitrateAdjuster->Update(packet.size());

            int64_t now_ms = m_clock->TimeInMilliseconds();
            m_encode_fps.Update(1, now_ms);
        }
        // if (!ck(cuCtxPopCurrent(&m_context)))
        //{
        //    RTC_LOG(LS_ERROR) << "cuCtxPopCurrent";
        //    return WEBRTC_VIDEO_CODEC_ENCODER_FAILURE;
        //}
        return WEBRTC_VIDEO_CODEC_OK;
    }

    int32_t NvEncoderImpl::ProcessEncodedFrame(std::vector<uint8_t>& packet, const ::webrtc::VideoFrame& inputFrame)
    {
        m_encodedImage._encodedWidth = inputFrame.video_frame_buffer()->width();
        m_encodedImage._encodedHeight = inputFrame.video_frame_buffer()->height();
        m_encodedImage.SetTimestamp(inputFrame.timestamp());
        m_encodedImage.SetSpatialIndex(0);
        m_encodedImage.ntp_time_ms_ = inputFrame.ntp_time_ms();
        m_encodedImage.capture_time_ms_ = inputFrame.render_time_ms();
        m_encodedImage.rotation_ = inputFrame.rotation();
        m_encodedImage.content_type_ = VideoContentType::UNSPECIFIED;
        m_encodedImage.timing_.flags = VideoSendTiming::kInvalid;
        m_encodedImage._frameType = VideoFrameType::kVideoFrameDelta;
        m_encodedImage.SetColorSpace(inputFrame.color_space());
        std::vector<H264::NaluIndex> naluIndices = H264::FindNaluIndices(packet.data(), packet.size());
        for (uint32_t i = 0; i < naluIndices.size(); i++)
        {
            const H264::NaluType naluType = H264::ParseNaluType(packet[naluIndices[i].payload_start_offset]);
            if (naluType == H264::kIdr)
            {
                m_encodedImage._frameType = VideoFrameType::kVideoFrameKey;
                break;
            }
        }

        m_encodedImage.SetEncodedData(EncodedImageBuffer::Create(packet.data(), packet.size()));
        m_encodedImage.set_size(packet.size());

        m_h264BitstreamParser.ParseBitstream(m_encodedImage);
        m_encodedImage.qp_ = m_h264BitstreamParser.GetLastSliceQp().value_or(-1);

        CodecSpecificInfo codecInfo;
        codecInfo.codecType = kVideoCodecH264;
        codecInfo.codecSpecific.H264.packetization_mode = H264PacketizationMode::NonInterleaved;

        const auto result = m_encodedCompleteCallback->OnEncodedImage(m_encodedImage, &codecInfo);
        if (result.error != EncodedImageCallback::Result::OK)
        {
            RTC_LOG(LS_ERROR) << "Encode m_encodedCompleteCallback failed " << result.error;
            return WEBRTC_VIDEO_CODEC_ERROR;
        }
        return WEBRTC_VIDEO_CODEC_OK;
    }

    void NvEncoderImpl::SetRates(const RateControlParameters& parameters)
    {
        if (m_encoder == nullptr)
        {
            RTC_LOG(LS_WARNING) << "while uninitialized.";
            return;
        }

        if (parameters.framerate_fps < 1.0)
        {
            RTC_LOG(LS_WARNING) << "Invalid frame rate: " << parameters.framerate_fps;
            return;
        }

        if (parameters.bitrate.get_sum_bps() == 0)
        {
            RTC_LOG(LS_WARNING) << "Encoder paused, turn off all encoding";
            SetStreamState(false);
            return;
        }
        float fps = static_cast<float>(parameters.framerate_fps + 0.5);
        int32_t pixelCount = m_codec.width * m_codec.height;

        auto requireLevel = H264SupportedLevel(pixelCount, fps);

        if (!requireLevel || m_level < static_cast<NV_ENC_LEVEL>(requireLevel.value()))
        {
            RTC_LOG(LS_WARNING) << "Not supported pixel count:" << pixelCount << ", fps:" << fps;
            SetStreamState(false);
            return;
        }

        // todo
        fps = 30;

        m_codec.maxFramerate = static_cast<uint32_t>(fps);

        m_bitrateAdjuster->SetTargetBitrateBps(parameters.bitrate.get_sum_bps());
        const uint32_t bitRate = m_bitrateAdjuster->GetAdjustedBitrateBps();

        NV_ENC_RECONFIGURE_PARAMS reconfigureParams = NV_ENC_RECONFIGURE_PARAMS();
        reconfigureParams.version = NV_ENC_RECONFIGURE_PARAMS_VER;
        std::memcpy(&reconfigureParams.reInitEncodeParams, &m_initializeParams, sizeof(m_initializeParams));
        NV_ENC_CONFIG reInitCodecConfig = NV_ENC_CONFIG();
        reInitCodecConfig.version = NV_ENC_CONFIG_VER;
        std::memcpy(&reInitCodecConfig, m_initializeParams.encodeConfig, sizeof(reInitCodecConfig));
        reconfigureParams.reInitEncodeParams.encodeConfig = &reInitCodecConfig;

        // change framerate
        reconfigureParams.reInitEncodeParams.frameRateNum = m_codec.maxFramerate;
        // reconfigureParams.resetEncoder = 1;
        // reconfigureParams.forceIDR = 1;

        // change bitrate
        reconfigureParams.reInitEncodeParams.encodeConfig->rcParams.averageBitRate = bitRate;
        m_encoder->Reconfigure(&reconfigureParams);

        // Force send Keyframe
        SetStreamState(true);
    }

    void NvEncoderImpl::SetStreamState(bool sendStream) { m_keyframeRequest = sendStream; }
} // end namespace webrtc
} // end namespace unity
