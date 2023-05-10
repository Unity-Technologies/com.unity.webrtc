#include "pch.h"

#include <absl/strings/match.h>
#include <api/video/video_codec_constants.h>
#include <api/video/video_codec_type.h>
#include <common_video/h264/h264_common.h>
#include <media/base/media_constants.h>
#include <modules/video_coding/include/video_codec_interface.h>

#include "Codec/H264ProfileLevelId.h"
#include "Codec/NvCodec/NvEncoderCudaWithCUarray.h"
#include "GraphicsDevice/Cuda/GpuMemoryBufferCudaHandle.h"
#include "NvCodecUtils.h"
#include "NvEncoder/NvEncoder.h"
#include "NvEncoder/NvEncoderCuda.h"
#include "NvEncoderImpl.h"
#include "ProfilerMarkerFactory.h"
#include "ScopedProfiler.h"
#include "UnityVideoTrackSource.h"
#include "VideoFrameAdapter.h"

namespace unity
{
namespace webrtc
{
    inline bool operator==(const CUDA_ARRAY_DESCRIPTOR& lhs, const CUDA_ARRAY_DESCRIPTOR& rhs)
    {
        return lhs.Width == rhs.Width && lhs.Height == rhs.Height && lhs.NumChannels == rhs.NumChannels &&
            lhs.Format == rhs.Format;
    }

    inline bool operator!=(const CUDA_ARRAY_DESCRIPTOR& lhs, const CUDA_ARRAY_DESCRIPTOR& rhs) { return !(lhs == rhs); }

    inline absl::optional<webrtc::H264Level> NvEncSupportedLevel(std::vector<SdpVideoFormat>& formats, const GUID& guid)
    {
        for (const auto& format : formats)
        {
            const auto profileLevelId = webrtc::ParseSdpForH264ProfileLevelId(format.parameters);
            if (!profileLevelId.has_value())
                continue;
            const auto guid2 = ProfileToGuid(profileLevelId.value().profile);
            if (guid2.has_value() && guid == guid2.value())
            {
                return profileLevelId.value().level;
            }
        }
        return absl::nullopt;
    }

    inline absl::optional<NV_ENC_LEVEL>
    NvEncRequiredLevel(const VideoCodec& codec, std::vector<SdpVideoFormat>& formats, const GUID& guid)
    {
        int pixelCount = codec.width * codec.height;
        auto requiredLevel = unity::webrtc::H264SupportedLevel(
            pixelCount, static_cast<int>(codec.maxFramerate), static_cast<int>(codec.maxBitrate));

        if (!requiredLevel)
        {
            return absl::nullopt;
        }

        // Check NvEnc supported level.
        auto supportedLevel = NvEncSupportedLevel(formats, guid);
        if (!supportedLevel)
        {
            return absl::nullopt;
        }

        // The supported level must be over the required level.
        if (static_cast<int>(requiredLevel.value()) > static_cast<int>(supportedLevel.value()))
        {
            return absl::nullopt;
        }
        return static_cast<NV_ENC_LEVEL>(requiredLevel.value());
    }

    absl::optional<H264Level> NvEncoderImpl::s_maxSupportedH264Level;
    std::vector<SdpVideoFormat> NvEncoderImpl::s_formats;

    NvEncoderImpl::NvEncoderImpl(
        const cricket::VideoCodec& codec,
        CUcontext context,
        CUmemorytype memoryType,
        NV_ENC_BUFFER_FORMAT format,
        ProfilerMarkerFactory* profiler)
        : m_context(context)
        , m_memoryType(memoryType)
        , m_scaledArray(nullptr)
        , m_encoder(nullptr)
        , m_format(format)
        , m_encodedCompleteCallback(nullptr)
        , m_encode_fps(1000, 1000)
        , m_clock(Clock::GetRealTimeClock())
        , m_profiler(profiler)
    {
        RTC_CHECK(absl::EqualsIgnoreCase(codec.name, cricket::kH264CodecName));
        // not implemented for host memory
        RTC_CHECK_NE(memoryType, CU_MEMORYTYPE_HOST);
        std::string profileLevelIdString;
        RTC_CHECK(codec.GetParam(cricket::kH264FmtpProfileLevelId, &profileLevelIdString));

        auto profileLevelId = ParseH264ProfileLevelId(profileLevelIdString.c_str());
        m_profileGuid = ProfileToGuid(profileLevelId.value().profile).value();
        m_level = static_cast<NV_ENC_LEVEL>(profileLevelId.value().level);
        m_configurations.reserve(kMaxSimulcastStreams);

        if (profiler)
            m_marker = profiler->CreateMarker(
                "NvEncoderImpl.CopyResource", kUnityProfilerCategoryOther, kUnityProfilerMarkerFlagDefault, 0);

        // SupportedNvEncoderCodecs and SupportedMaxH264Level function consume the session of NvEnc and the number of
        // the sessions is limited by NVIDIA device. So it caches the return value here.
        if (s_formats.empty())
            s_formats = SupportedNvEncoderCodecs(m_context);
        if (!s_maxSupportedH264Level.has_value())
            s_maxSupportedH264Level = SupportedMaxH264Level(m_context);
    }

    NvEncoderImpl::~NvEncoderImpl() { Release(); }

    VideoEncoder::EncoderInfo NvEncoderImpl::GetEncoderInfo() const
    {
        VideoEncoder::EncoderInfo info;
        info.implementation_name = "NvCodec";
        info.is_hardware_accelerated = true;
        return info;
    }

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

        m_codec = *codec;

        // Check required level.
        auto requiredLevel = NvEncRequiredLevel(m_codec, s_formats, m_profileGuid);
        if (!requiredLevel)
        {
            // workaround
            // Use supported max framerate that calculated by h264 level define.
            m_codec.maxFramerate = static_cast<uint32_t>(
                SupportedMaxFramerate(s_maxSupportedH264Level.value(), m_codec.width * m_codec.height));
            requiredLevel = NvEncRequiredLevel(m_codec, s_formats, m_profileGuid);
            if (!requiredLevel)
            {
                return WEBRTC_VIDEO_CODEC_ERR_PARAMETER;
            }
        }

        // workaround
        // Use required level if the profile level is lower than required level.
        if (requiredLevel.value() > m_level)
        {
            m_level = requiredLevel.value();
        }

        int32_t ret = Release();
        if (ret != WEBRTC_VIDEO_CODEC_OK)
        {
            return ret;
        }

        const int number_of_streams = 1;
        m_configurations.resize(number_of_streams);

        const CUresult result = cuCtxSetCurrent(m_context);
        if (!ck(result))
        {
            return WEBRTC_VIDEO_CODEC_ENCODER_FAILURE;
        }

        // Some NVIDIA GPUs have a limited Encode Session count.
        // We can't get the Session count, so catching NvEncThrow to avoid the crash.
        // refer: https://developer.nvidia.com/video-encode-and-decode-gpu-support-matrix-new
        try
        {
            if (m_memoryType == CU_MEMORYTYPE_DEVICE)
            {
                m_encoder = std::make_unique<NvEncoderCuda>(m_context, m_codec.width, m_codec.height, m_format, 0);
            }
            else if (m_memoryType == CU_MEMORYTYPE_ARRAY)
            {
                m_encoder =
                    std::make_unique<NvEncoderCudaWithCUarray>(m_context, m_codec.width, m_codec.height, m_format, 0);
            }
            else
            {
                RTC_DCHECK_NOTREACHED();
            }
        }
        catch (const NVENCException& e)
        {
            // todo: If Encoder initialization fails, need to notify for Managed side.
            RTC_LOG(LS_ERROR) << "Failed Initialize NvEncoder " << e.what();
            return WEBRTC_VIDEO_CODEC_ERROR;
        }

        // todo(kazuki): Add multiple configurations to support simulcast
        m_configurations[0].width = m_codec.width;
        m_configurations[0].height = m_codec.height;
        m_configurations[0].sending = false;
        m_configurations[0].max_frame_rate = static_cast<float>(m_codec.maxFramerate);
        m_configurations[0].key_frame_interval = m_codec.H264()->keyFrameInterval;
        m_configurations[0].max_bps = m_codec.maxBitrate * 1000;
        m_configurations[0].target_bps = m_codec.startBitrate * 1000;

        m_bitrateAdjuster = std::make_unique<BitrateAdjuster>(0.5f, 0.95f);
        m_bitrateAdjuster->SetTargetBitrateBps(m_configurations[0].target_bps);

        m_initializeParams.version = NV_ENC_INITIALIZE_PARAMS_VER;
        m_encodeConfig.version = NV_ENC_CONFIG_VER;
        m_initializeParams.encodeConfig = &m_encodeConfig;

        GUID encodeGuid = NV_ENC_CODEC_H264_GUID;
        GUID presetGuid = NV_ENC_PRESET_P4_GUID;

        m_encoder->CreateDefaultEncoderParams(
            &m_initializeParams, encodeGuid, presetGuid, NV_ENC_TUNING_INFO_ULTRA_LOW_LATENCY);

        m_initializeParams.frameRateNum = static_cast<uint32_t>(m_configurations[0].max_frame_rate);
        m_initializeParams.frameRateDen = 1;

        m_encodeConfig.profileGUID = m_profileGuid;
        m_encodeConfig.gopLength = NVENC_INFINITE_GOPLENGTH;
        m_encodeConfig.frameIntervalP = 1;
        m_encodeConfig.encodeCodecConfig.h264Config.level = m_level;
        m_encodeConfig.encodeCodecConfig.h264Config.idrPeriod = NVENC_INFINITE_GOPLENGTH;
        m_encodeConfig.rcParams.version = NV_ENC_RC_PARAMS_VER;
        m_encodeConfig.rcParams.rateControlMode = NV_ENC_PARAMS_RC_CBR;
        m_encodeConfig.rcParams.averageBitRate = m_configurations[0].target_bps;
        m_encodeConfig.rcParams.vbvBufferSize = (m_encodeConfig.rcParams.averageBitRate *
                                                 m_initializeParams.frameRateDen / m_initializeParams.frameRateNum) *
            5;
        m_encodeConfig.rcParams.vbvInitialDelay = m_encodeConfig.rcParams.vbvBufferSize;

        try
        {
            m_encoder->CreateEncoder(&m_initializeParams);
        }
        catch (const NVENCException& e)
        {
            RTC_LOG(LS_ERROR) << "Failed Initialize NvEncoder " << e.what();
            return WEBRTC_VIDEO_CODEC_ERROR;
        }

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
        if (m_scaledArray)
        {
            cuArrayDestroy(m_scaledArray);
            m_scaledArray = nullptr;
        }
        m_configurations.clear();

        return WEBRTC_VIDEO_CODEC_OK;
    }

    void NvEncoderImpl::Resize(const CUarray& src, CUarray& dst, const Size& size)
    {
        CUDA_ARRAY_DESCRIPTOR srcDesc = {};
        CUresult result = cuArrayGetDescriptor(&srcDesc, src);
        if (result != CUDA_SUCCESS)
        {
            RTC_LOG(LS_ERROR) << "cuArrayGetDescriptor failed. error:" << result;
            return;
        }
        CUDA_ARRAY_DESCRIPTOR dstDesc = {};
        dstDesc.Format = srcDesc.Format;
        dstDesc.NumChannels = srcDesc.NumChannels;
        dstDesc.Width = static_cast<size_t>(size.width());
        dstDesc.Height = static_cast<size_t>(size.height());

        bool create = false;
        if (!dst)
        {
            create = true;
        }
        else
        {
            CUDA_ARRAY_DESCRIPTOR desc = {};
            result = cuArrayGetDescriptor(&desc, dst);
            if (result != CUDA_SUCCESS)
            {
                RTC_LOG(LS_ERROR) << "cuArrayGetDescriptor failed. error:" << result;
                return;
            }
            if (desc != dstDesc)
            {
                result = cuArrayDestroy(dst);
                if (result != CUDA_SUCCESS)
                {
                    RTC_LOG(LS_ERROR) << "cuArrayDestroy failed. error:" << result;
                    return;
                }
                dst = nullptr;
                create = true;
            }
        }

        if (create)
        {
            CUresult result = cuArrayCreate(&dst, &dstDesc);
            if (result != CUDA_SUCCESS)
            {
                RTC_LOG(LS_ERROR) << "cuArrayCreate failed. error:" << result;
                return;
            }
        }
    }

    bool NvEncoderImpl::CopyResource(
        const NvEncInputFrame* encoderInputFrame,
        GpuMemoryBufferInterface* buffer,
        Size& size,
        CUcontext context,
        CUmemorytype memoryType)
    {
        std::unique_ptr<const ScopedProfiler> profiler;
        if (m_profiler)
            profiler = m_profiler->CreateScopedProfiler(*m_marker);

        const GpuMemoryBufferCudaHandle* handle = static_cast<const GpuMemoryBufferCudaHandle*>(buffer->handle());
        if (!handle)
        {
            RTC_LOG(LS_INFO) << "GpuMemoryBufferCudaHandle is null";
            return false;
        }

        if (memoryType == CU_MEMORYTYPE_DEVICE)
        {
            NvEncoderCuda::CopyToDeviceFrame(
                context,
                reinterpret_cast<void*>(handle->mappedPtr),
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
            void* pSrcArray = static_cast<void*>(handle->mappedArray);

            // Resize cuda array when the resolution of input buffer is different from output one.
            // The output buffer named m_scaledArray is reused while the resolution is matched.
            if (buffer->GetSize() != size)
            {
                Resize(handle->mappedArray, m_scaledArray, size);
                pSrcArray = static_cast<void*>(m_scaledArray);
            }

            NvEncoderCudaWithCUarray::CopyToDeviceFrame(
                context,
                pSrcArray,
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
        return true;
    }

    int32_t NvEncoderImpl::Encode(const ::webrtc::VideoFrame& frame, const std::vector<VideoFrameType>* frameTypes)
    {
        RTC_DCHECK_EQ(frame.width(), m_codec.width);
        RTC_DCHECK_EQ(frame.height(), m_codec.height);

        if (!m_encoder)
            return WEBRTC_VIDEO_CODEC_UNINITIALIZED;
        if (!m_encodedCompleteCallback)
            return WEBRTC_VIDEO_CODEC_UNINITIALIZED;

        auto frameBuffer = frame.video_frame_buffer();
        if (frameBuffer->type() != VideoFrameBuffer::Type::kNative || frameBuffer->width() != m_codec.width ||
            frameBuffer->height() != m_codec.height)
            return WEBRTC_VIDEO_CODEC_ERR_PARAMETER;

        auto videoFrameBuffer = static_cast<ScalableBufferInterface*>(frameBuffer.get());
        rtc::scoped_refptr<VideoFrame> video_frame = videoFrameBuffer->scaled()
            ? static_cast<VideoFrameAdapter::ScaledBuffer*>(videoFrameBuffer)->GetVideoFrame()
            : static_cast<VideoFrameAdapter*>(videoFrameBuffer)->GetVideoFrame();

        if (!video_frame)
        {
            return WEBRTC_VIDEO_CODEC_ENCODER_FAILURE;
        }

        bool send_key_frame = false;
        if (m_configurations[0].key_frame_request && m_configurations[0].sending)
            send_key_frame = true;

        if (!send_key_frame && frameTypes)
        {
            if (m_configurations[0].sending && (*frameTypes)[0] == VideoFrameType::kVideoFrameKey)
            {
                send_key_frame = true;
            }
        }

        Size encodeSize(m_encoder->GetEncodeWidth(), m_encoder->GetEncodeHeight());

        const NvEncInputFrame* encoderInputFrame = m_encoder->GetNextInputFrame();

        // Copy CUDA buffer in VideoFrame to encoderInputFrame.
        auto buffer = video_frame->GetGpuMemoryBuffer();
        if (!CopyResource(encoderInputFrame, buffer, encodeSize, m_context, m_memoryType))
            return WEBRTC_VIDEO_CODEC_ENCODER_FAILURE;

        NV_ENC_PIC_PARAMS picParams = NV_ENC_PIC_PARAMS();
        picParams.version = NV_ENC_PIC_PARAMS_VER;
        picParams.encodePicFlags = 0;
        if (send_key_frame)
        {
            picParams.encodePicFlags =
                NV_ENC_PIC_FLAG_FORCEINTRA | NV_ENC_PIC_FLAG_FORCEIDR | NV_ENC_PIC_FLAG_OUTPUT_SPSPPS;
            m_configurations[0].key_frame_request = false;
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
        return WEBRTC_VIDEO_CODEC_OK;
    }

    int32_t NvEncoderImpl::ProcessEncodedFrame(std::vector<uint8_t>& packet, const ::webrtc::VideoFrame& inputFrame)
    {
        m_encodedImage._encodedWidth = inputFrame.video_frame_buffer()->width();
        m_encodedImage._encodedHeight = inputFrame.video_frame_buffer()->height();
        m_encodedImage.SetTimestamp(inputFrame.timestamp());
        m_encodedImage.SetSimulcastIndex(0);
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
            m_configurations[0].SetStreamState(false);
            return;
        }

        m_bitrateAdjuster->SetTargetBitrateBps(parameters.bitrate.get_sum_bps());
        const uint32_t bitrate = m_bitrateAdjuster->GetAdjustedBitrateBps();

        m_codec.maxFramerate = static_cast<uint32_t>(parameters.framerate_fps);
        m_codec.maxBitrate = bitrate;

        // Check required level.
        auto requiredLevel = NvEncRequiredLevel(m_codec, s_formats, m_profileGuid);
        if (!requiredLevel)
        {
            // workaround
            // Use supported max framerate that calculated by h264 level define.
            m_codec.maxFramerate = static_cast<uint32_t>(
                SupportedMaxFramerate(s_maxSupportedH264Level.value(), m_codec.width * m_codec.height));
            requiredLevel = NvEncRequiredLevel(m_codec, s_formats, m_profileGuid);
            if (!requiredLevel)
            {
                RTC_LOG(LS_WARNING) << "Not supported codec parameter "
                                    << "width:" << m_codec.width << " "
                                    << "height:" << m_codec.height << " "
                                    << "maxFramerate:" << m_codec.maxFramerate;
                m_configurations[0].SetStreamState(false);
                return;
            }
        }

        // workaround:
        // Use required level if the profile level is lower than required level.
        if (requiredLevel.value() > m_level)
        {
            m_level = requiredLevel.value();
        }

        m_configurations[0].target_bps = m_codec.maxBitrate;
        m_configurations[0].max_frame_rate = static_cast<float>(m_codec.maxFramerate);

        NV_ENC_RECONFIGURE_PARAMS reconfigureParams = NV_ENC_RECONFIGURE_PARAMS();
        reconfigureParams.version = NV_ENC_RECONFIGURE_PARAMS_VER;
        std::memcpy(&reconfigureParams.reInitEncodeParams, &m_initializeParams, sizeof(m_initializeParams));
        NV_ENC_CONFIG reInitCodecConfig = NV_ENC_CONFIG();
        reInitCodecConfig.version = NV_ENC_CONFIG_VER;
        std::memcpy(&reInitCodecConfig, m_initializeParams.encodeConfig, sizeof(reInitCodecConfig));
        reconfigureParams.reInitEncodeParams.encodeConfig = &reInitCodecConfig;

        // Change framerate and bitrate
        reconfigureParams.reInitEncodeParams.frameRateNum = static_cast<uint32_t>(m_configurations[0].max_frame_rate);
        reInitCodecConfig.encodeCodecConfig.h264Config.level = m_level;
        reInitCodecConfig.rcParams.averageBitRate = m_configurations[0].target_bps;
        reInitCodecConfig.rcParams.vbvBufferSize =
            (reInitCodecConfig.rcParams.averageBitRate * reconfigureParams.reInitEncodeParams.frameRateDen /
             reconfigureParams.reInitEncodeParams.frameRateNum) *
            5;
        reInitCodecConfig.rcParams.vbvInitialDelay = m_encodeConfig.rcParams.vbvBufferSize;

        try
        {
            m_encoder->Reconfigure(&reconfigureParams);
        }
        catch (const NVENCException& e)
        {
            RTC_LOG(LS_ERROR) << "Failed Reconfigure NvEncoder " << e.what();
            return;
        }

        // Force send Keyframe
        m_configurations[0].SetStreamState(true);
    }

    void NvEncoderImpl::LayerConfig::SetStreamState(bool sendStream)
    {
        if (sendStream && !sending)
        {
            key_frame_request = true;
        }
        sending = sendStream;
    }
} // end namespace webrtc
} // end namespace unity
