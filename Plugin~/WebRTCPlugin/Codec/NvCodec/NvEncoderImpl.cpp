#include "pch.h"

// todo::
// CMake doesn't support building CUDA kernel with Clang compiler on Windows.
// https://gitlab.kitware.com/cmake/cmake/-/issues/20776
#if !(_WIN32 && __clang__)
#define SUPPORT_CUDA_KERNEL 1
#endif

#include <absl/strings/match.h>
#include <api/video/video_codec_constants.h>
#include <api/video/video_codec_type.h>
#include <common_video/h264/h264_common.h>
#include <media/base/media_constants.h>
#include <modules/video_coding/include/video_codec_interface.h>
#include <modules/video_coding/utility/simulcast_rate_allocator.h>
#include <modules/video_coding/utility/simulcast_utility.h>

#include "Codec/H264ProfileLevelId.h"
#include "Codec/NvCodec/NvEncoderCudaWithCUarray.h"
#include "GraphicsDevice/Cuda/GpuMemoryBufferCudaHandle.h"
#include "NvEncoder/NvEncoder.h"
#include "NvEncoder/NvEncoderCuda.h"
#include "NvEncoderImpl.h"
#include "ProfilerMarkerFactory.h"
#if SUPPORT_CUDA_KERNEL
#include "ResizeSurf.h"
#endif
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

#if SUPPORT_CUDA_KERNEL
    CUresult Resize(const CUarray& src, CUarray& dst, const Size& size)
    {
        CUDA_ARRAY_DESCRIPTOR srcDesc = {};
        CUresult result = cuArrayGetDescriptor(&srcDesc, src);
        if (result != CUDA_SUCCESS)
        {
            RTC_LOG(LS_ERROR) << "cuArrayGetDescriptor failed. error:" << result;
            return result;
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
                return result;
            }
            if (desc != dstDesc)
            {
                result = cuArrayDestroy(dst);
                if (result != CUDA_SUCCESS)
                {
                    RTC_LOG(LS_ERROR) << "cuArrayDestroy failed. error:" << result;
                    return result;
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
                return result;
            }
        }
        return ResizeSurf(src, dst);
    }
#endif

    NvEncoderImpl::NvEncoderImpl(
        const cricket::VideoCodec& codec,
        CUcontext context,
        CUmemorytype memoryType,
        NV_ENC_BUFFER_FORMAT format,
        ProfilerMarkerFactory* profiler)
        : m_context(context)
        , m_memoryType(memoryType)
        , m_format(format)
        , m_encodedCompleteCallback(nullptr)
        , m_encode_fps(1000, 1000)
        , m_clock(Clock::GetRealTimeClock())
        , m_profiler(profiler)
    {
        m_downscaledBuffers.reserve(kMaxSimulcastStreams - 1);
        m_encodedImages.reserve(kMaxSimulcastStreams);
        m_encoders.reserve(kMaxSimulcastStreams);
        m_configurations.reserve(kMaxSimulcastStreams);
        m_initializeParams.reserve(kMaxSimulcastStreams);
        m_encodeConfigs.reserve(kMaxSimulcastStreams);

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

        const int number_of_streams = SimulcastUtility::NumberOfSimulcastStreams(m_codec);
        if (number_of_streams > 1 && !SimulcastUtility::ValidSimulcastParameters(m_codec, number_of_streams))
        {
            return WEBRTC_VIDEO_CODEC_ERR_SIMULCAST_PARAMETERS_NOT_SUPPORTED;
        }
        m_downscaledBuffers.resize(number_of_streams - 1);
        m_encodedImages.resize(number_of_streams);
        m_encoders.resize(number_of_streams);
        m_configurations.resize(number_of_streams);
        m_initializeParams.resize(number_of_streams);
        m_encodeConfigs.resize(number_of_streams);

        // Code expects simulcastStream resolutions to be correct, make sure they are
        // filled even when there are no simulcast layers.
        if (m_codec.numberOfSimulcastStreams == 0)
        {
            m_codec.simulcastStream[0].width = m_codec.width;
            m_codec.simulcastStream[0].height = m_codec.height;
            m_codec.simulcastStream[0].maxBitrate = m_codec.maxBitrate;
            m_codec.simulcastStream[0].maxFramerate = m_codec.maxFramerate;
        }

        const CUresult result = cuCtxSetCurrent(m_context);
        if (!ck(result))
        {
            return WEBRTC_VIDEO_CODEC_ENCODER_FAILURE;
        }

        for (int i = 0, idx = number_of_streams - 1; i < number_of_streams; ++i, --idx)
        {
            std::unique_ptr<NvEncoderInternal> encoder;

            SimulcastStream simlcastStream = m_codec.simulcastStream[idx];
            int width = simlcastStream.width;
            int height = simlcastStream.height;
            float maxFramerate = simlcastStream.maxFramerate;
            uint32_t maxBitrate = simlcastStream.maxBitrate;
            uint32_t targetBitrate = simlcastStream.targetBitrate;

            // Some NVIDIA GPUs have a limited Encode Session count.
            // We can't get the Session count, so catching NvEncThrow to avoid the crash.
            // refer: https://developer.nvidia.com/video-encode-and-decode-gpu-support-matrix-new
            try
            {
                if (m_memoryType == CU_MEMORYTYPE_DEVICE)
                {
                    encoder = std::make_unique<NvEncoderCuda>(m_context, width, height, m_format, 0);
                }
                else if (m_memoryType == CU_MEMORYTYPE_ARRAY)
                {
                    encoder = std::make_unique<NvEncoderCudaWithCUarray>(m_context, width, height, m_format, 0);
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
            m_encoders[i] = std::move(encoder);

            m_configurations[i].width = width;
            m_configurations[i].height = height;
            m_configurations[i].simulcast_idx = idx;
            m_configurations[i].sending = false;
            m_configurations[i].max_frame_rate = maxFramerate;
            m_configurations[i].key_frame_interval = m_codec.H264()->keyFrameInterval;

            // Set nullptr for downscaled image buffers.
            // Allocate buffers in Encode method.
            if (i > 0)
            {
                m_downscaledBuffers[i - 1] = nullptr;
            }

            // Codec_settings uses kbits/second; encoder uses bits/second.
            m_configurations[i].max_bps = m_codec.maxFramerate * 1000;
            m_configurations[i].target_bps = m_codec.startBitrate * 1000;

            // Create encoder parameters based on the layer configuration.
            m_initializeParams[i] = CreateEncoderParams(i);

            try
            {
                m_encoders[i]->CreateEncoder(&m_initializeParams[i]);
            }
            catch (const NVENCException& e)
            {
                RTC_LOG(LS_ERROR) << "Failed Initialize NvEncoder " << e.what();
                return WEBRTC_VIDEO_CODEC_ERROR;
            }
        }

        SimulcastRateAllocator init_allocator(m_codec);
        VideoBitrateAllocation allocation = init_allocator.Allocate(
            VideoBitrateAllocationParameters(DataRate::KilobitsPerSec(m_codec.startBitrate), m_codec.maxFramerate));
        SetRates(RateControlParameters(allocation, m_codec.maxFramerate));
        return WEBRTC_VIDEO_CODEC_OK;
    }

    NV_ENC_INITIALIZE_PARAMS NvEncoderImpl::CreateEncoderParams(size_t i)
    {
        NV_ENC_INITIALIZE_PARAMS init = {};
        NV_ENC_CONFIG config = {};
        init.version = NV_ENC_INITIALIZE_PARAMS_VER;
        m_encodeConfigs[i].version = NV_ENC_CONFIG_VER;
        init.encodeConfig = &m_encodeConfigs[i];

        m_encoders[i]->CreateDefaultEncoderParams(
            &init, NV_ENC_CODEC_H264_GUID, NV_ENC_PRESET_P4_GUID, NV_ENC_TUNING_INFO_ULTRA_LOW_LATENCY);

        init.frameRateNum = static_cast<uint32_t>(m_configurations[i].max_frame_rate);
        init.frameRateDen = 1;

        m_encodeConfigs[i].profileGUID = m_profileGuid;
        m_encodeConfigs[i].gopLength = NVENC_INFINITE_GOPLENGTH;
        m_encodeConfigs[i].frameIntervalP = 1;
        m_encodeConfigs[i].encodeCodecConfig.h264Config.level = m_level;
        m_encodeConfigs[i].encodeCodecConfig.h264Config.idrPeriod = NVENC_INFINITE_GOPLENGTH;
        m_encodeConfigs[i].rcParams.version = NV_ENC_RC_PARAMS_VER;
        m_encodeConfigs[i].rcParams.rateControlMode = NV_ENC_PARAMS_RC_CBR;
        m_encodeConfigs[i].rcParams.averageBitRate = m_configurations[i].target_bps;
        m_encodeConfigs[i].rcParams.vbvBufferSize =
            (m_encodeConfigs[i].rcParams.averageBitRate * init.frameRateDen / init.frameRateNum) * 5;
        m_encodeConfigs[i].rcParams.vbvInitialDelay = m_encodeConfigs[i].rcParams.vbvBufferSize;

        return init;
    }

    int32_t NvEncoderImpl::RegisterEncodeCompleteCallback(EncodedImageCallback* callback)
    {
        m_encodedCompleteCallback = callback;
        return WEBRTC_VIDEO_CODEC_OK;
    }

    int32_t NvEncoderImpl::Release()
    {
        for (auto it = m_downscaledBuffers.rbegin(); it != m_downscaledBuffers.rend(); ++it)
        {
            cuArrayDestroy(*it);
        }
        for (auto it = m_encoders.rbegin(); it != m_encoders.rend(); ++it)
        {
            (*it)->DestroyEncoder();
            it->release();
        }
        m_downscaledBuffers.clear();
        m_encodedImages.clear();
        m_configurations.clear();
        m_encoders.clear();
        m_initializeParams.clear();
        m_encodeConfigs.clear();

        return WEBRTC_VIDEO_CODEC_OK;
    }

    bool NvEncoderImpl::CopyResource(
        const NvEncInputFrame* encoderInputFrame,
        GpuMemoryBufferInterface* buffer,
        const Size& size,
        CUcontext context,
        CUarray downscaledBuffer,
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
#if SUPPORT_CUDA_KERNEL
            if (buffer->GetSize() != size)
            {
                CUresult result = Resize(handle->mappedArray, downscaledBuffer, size);
                if (result != CUDA_SUCCESS)
                {
                    RTC_LOG(LS_INFO) << "Resize failed. original size=" << buffer->GetSize().width() << ","
                                     << buffer->GetSize().height() << " output size=" << size.width() << ","
                                     << size.height();
                    return false;
                }
                pSrcArray = static_cast<void*>(downscaledBuffer);
            }
#endif
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
        if (m_encoders.empty())
            return WEBRTC_VIDEO_CODEC_UNINITIALIZED;
        if (!m_encodedCompleteCallback)
        {
            RTC_LOG(LS_WARNING) << "InitEncode() has been called, but a callback function "
                                   "has not been set with RegisterEncodeCompleteCallback()";
            return WEBRTC_VIDEO_CODEC_UNINITIALIZED;
        }

        auto frameBuffer = frame.video_frame_buffer();
        RTC_CHECK(frameBuffer->type() == VideoFrameBuffer::Type::kNative);

        if (frameBuffer->type() != VideoFrameBuffer::Type::kNative || frameBuffer->width() != m_codec.width ||
            frameBuffer->height() != m_codec.height)
            return WEBRTC_VIDEO_CODEC_ERR_PARAMETER;

        auto videoFrameBuffer = static_cast<ScalableBufferInterface*>(frameBuffer.get());
        rtc::scoped_refptr<VideoFrame> video_frame = videoFrameBuffer->scaled()
            ? static_cast<VideoFrameAdapter::ScaledBuffer*>(videoFrameBuffer)->GetVideoFrame()
            : static_cast<VideoFrameAdapter*>(videoFrameBuffer)->GetVideoFrame();

        if (!video_frame)
        {
            RTC_LOG(LS_ERROR) << "Failed to convert "
                              << VideoFrameBufferTypeToString(frame.video_frame_buffer()->type())
                              << " image to VideoFrame. Can't encode frame.";
            return WEBRTC_VIDEO_CODEC_ENCODER_FAILURE;
        }

        bool is_keyframe_needed = false;
        for (size_t i = 0; i < m_configurations.size(); ++i)
        {
            if (m_configurations[i].key_frame_request && m_configurations[i].sending)
            {
                // This is legacy behavior, generating a keyframe on all layers
                // when generating one for a layer that became active for the first time
                // or after being disabled.
                is_keyframe_needed = true;
                break;
            }
        }

        RTC_DCHECK_EQ(m_configurations[0].width, videoFrameBuffer->width());
        RTC_DCHECK_EQ(m_configurations[0].height, videoFrameBuffer->height());

        // Encode image for each layer.
        for (size_t i = 0; i < m_encoders.size(); ++i)
        {
            if (!m_configurations[i].sending)
                continue;
            if (frameTypes != nullptr && i < frameTypes->size())
            {
                // Skip frame?
                if ((*frameTypes)[i] == VideoFrameType::kEmptyFrame)
                {
                    continue;
                }
            }
            // Send a key frame either when this layer is configured to require one
            // or we have explicitly been asked to.
            const size_t simulcast_idx = static_cast<size_t>(m_configurations[i].simulcast_idx);
            bool send_key_frame = is_keyframe_needed ||
                (frameTypes && simulcast_idx < frameTypes->size() &&
                 (*frameTypes)[simulcast_idx] == VideoFrameType::kVideoFrameKey);

            const Size encodeSize(m_encoders[i]->GetEncodeWidth(), m_encoders[i]->GetEncodeHeight());
            const NvEncInputFrame* encoderInputFrame = m_encoders[i]->GetNextInputFrame();

            // Copy CUDA buffer in VideoFrame to encoderInputFrame.
            auto buffer = video_frame->GetGpuMemoryBuffer();
            CUarray downscaledBuffer = i > 0 ? m_downscaledBuffers[i - 1] : nullptr;

            if (!CopyResource(encoderInputFrame, buffer, encodeSize, m_context, downscaledBuffer, m_memoryType))
                return WEBRTC_VIDEO_CODEC_ENCODER_FAILURE;

            NV_ENC_PIC_PARAMS picParams = NV_ENC_PIC_PARAMS();
            picParams.version = NV_ENC_PIC_PARAMS_VER;
            picParams.encodePicFlags = 0;
            if (send_key_frame)
            {
                picParams.encodePicFlags =
                    NV_ENC_PIC_FLAG_FORCEINTRA | NV_ENC_PIC_FLAG_FORCEIDR | NV_ENC_PIC_FLAG_OUTPUT_SPSPPS;
                m_configurations[i].key_frame_request = false;
            }

            std::vector<std::vector<uint8_t>> vPacket;
            m_encoders[i]->EncodeFrame(vPacket, &picParams);

            for (std::vector<uint8_t>& packet : vPacket)
            {
                int32_t result = ProcessEncodedFrame(i, packet, frame);
                if (result != WEBRTC_VIDEO_CODEC_OK)
                {
                    RTC_LOG(LS_ERROR) << "NvCodec frame encoding failed, EncodeFrame returned " << result << ".";
                    return result;
                }
                int64_t now_ms = m_clock->TimeInMilliseconds();
                m_encode_fps.Update(1, now_ms);
            }
        }
        return WEBRTC_VIDEO_CODEC_OK;
    }

    int32_t
    NvEncoderImpl::ProcessEncodedFrame(size_t i, std::vector<uint8_t>& packet, const ::webrtc::VideoFrame& inputFrame)
    {
        m_encodedImages[i]._encodedWidth = m_configurations[i].width;
        m_encodedImages[i]._encodedHeight = m_configurations[i].height;
        m_encodedImages[i].SetTimestamp(inputFrame.timestamp());
        m_encodedImages[i].SetColorSpace(inputFrame.color_space());
        m_encodedImages[i].ntp_time_ms_ = inputFrame.ntp_time_ms();
        m_encodedImages[i].capture_time_ms_ = inputFrame.render_time_ms();
        m_encodedImages[i].rotation_ = inputFrame.rotation();
        m_encodedImages[i].content_type_ = VideoContentType::UNSPECIFIED;
        m_encodedImages[i].timing_.flags = VideoSendTiming::kInvalid;
        m_encodedImages[i]._frameType = VideoFrameType::kVideoFrameDelta;
        // TODO(kazuki): Change EncodedImage::SetSimulcastIndex when upgrading libwebrtc.
        m_encodedImages[i].SetSpatialIndex(m_configurations[i].simulcast_idx);

        std::vector<H264::NaluIndex> naluIndices = H264::FindNaluIndices(packet.data(), packet.size());
        for (uint32_t naluIdx = 0; naluIdx < naluIndices.size(); naluIdx++)
        {
            const H264::NaluType naluType = H264::ParseNaluType(packet[naluIndices[naluIdx].payload_start_offset]);
            if (naluType == H264::kIdr)
            {
                m_encodedImages[i]._frameType = VideoFrameType::kVideoFrameKey;
                break;
            }
        }
        m_encodedImages[i].SetEncodedData(EncodedImageBuffer::Create(packet.data(), packet.size()));
        m_encodedImages[i].set_size(packet.size());

        m_h264BitstreamParser.ParseBitstream(m_encodedImages[i]);
        m_encodedImages[i].qp_ = m_h264BitstreamParser.GetLastSliceQp().value_or(-1);

        CodecSpecificInfo codecSpecific;
        codecSpecific.codecType = kVideoCodecH264;
        codecSpecific.codecSpecific.H264.packetization_mode = H264PacketizationMode::NonInterleaved;

        m_encodedCompleteCallback->OnEncodedImage(m_encodedImages[i], &codecSpecific);
        return WEBRTC_VIDEO_CODEC_OK;
    }

    void NvEncoderImpl::SetRates(const RateControlParameters& parameters)
    {
        if (m_encoders.empty())
        {
            RTC_LOG(LS_WARNING) << "SetRates() while uninitialized.";
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
            for (size_t i = 0; i < m_configurations.size(); ++i)
                m_configurations[i].SetStreamState(false);
            return;
        }
        m_codec.maxFramerate = static_cast<uint32_t>(parameters.framerate_fps);

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
                for (size_t i = 0; i < m_configurations.size(); ++i)
                    m_configurations[i].SetStreamState(false);
                return;
            }
        }

        // workaround:
        // Use required level if the profile level is lower than required level.
        if (requiredLevel.value() > m_level)
        {
            m_level = requiredLevel.value();
        }

        size_t stream_idx = m_encoders.size() - 1;
        for (size_t i = 0; i < m_encoders.size(); ++i, --stream_idx)
        {
            // Update layer config.
            m_configurations[i].target_bps = parameters.bitrate.GetSpatialLayerSum(stream_idx);
            m_configurations[i].max_frame_rate = parameters.framerate_fps;

            NV_ENC_RECONFIGURE_PARAMS reconfigureParams = NV_ENC_RECONFIGURE_PARAMS();
            reconfigureParams.version = NV_ENC_RECONFIGURE_PARAMS_VER;
            std::memcpy(&reconfigureParams.reInitEncodeParams, &m_initializeParams[i], sizeof(m_initializeParams[i]));
            NV_ENC_CONFIG reInitCodecConfig = NV_ENC_CONFIG();
            reInitCodecConfig.version = NV_ENC_CONFIG_VER;
            std::memcpy(&reInitCodecConfig, m_initializeParams[i].encodeConfig, sizeof(reInitCodecConfig));
            reconfigureParams.reInitEncodeParams.encodeConfig = &reInitCodecConfig;

            // Change framerate and bitrate
            reconfigureParams.reInitEncodeParams.frameRateNum =
                static_cast<uint32_t>(m_configurations[i].max_frame_rate);
            reInitCodecConfig.encodeCodecConfig.h264Config.level = m_level;
            reInitCodecConfig.rcParams.averageBitRate = m_configurations[i].target_bps;
            reInitCodecConfig.rcParams.vbvBufferSize =
                (reInitCodecConfig.rcParams.averageBitRate * reconfigureParams.reInitEncodeParams.frameRateDen /
                 reconfigureParams.reInitEncodeParams.frameRateNum) *
                5;
            reInitCodecConfig.rcParams.vbvInitialDelay = m_initializeParams[i].encodeConfig->rcParams.vbvBufferSize;

            try
            {
                m_encoders[i]->Reconfigure(&reconfigureParams);
            }
            catch (const NVENCException& e)
            {
                RTC_LOG(LS_ERROR) << "Failed Reconfigure NvEncoder " << e.what();
                return;
            }

            // Force send Keyframe
            m_configurations[i].SetStreamState(true);
        }
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
