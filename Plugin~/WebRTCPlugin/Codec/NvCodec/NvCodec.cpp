#include "pch.h"

#include <absl/strings/match.h>
#include <api/video_codecs/video_encoder_factory.h>
#include <modules/video_coding/codecs/h264/include/h264.h>

#include "Codec/CreateVideoCodecFactory.h"
#include "NvCodec.h"
#include "NvDecoder/NvDecoder.h"
#include "NvDecoderImpl.h"
#include "NvEncoder/NvEncoderCuda.h"
#include "NvEncoderImpl.h"
#include "ProfilerMarkerFactory.h"

namespace unity
{
namespace webrtc
{
    using namespace ::webrtc;

    constexpr char kCodecName[] = "NvCodec";

    class NvEncoderCudaCapability : public NvEncoderCuda
    {
    public:
        NvEncoderCudaCapability(CUcontext cuContext)
            : NvEncoderCuda(cuContext, 0, 0, NV_ENC_BUFFER_FORMAT_UNDEFINED)
        {
        }

        std::vector<GUID> GetEncodeProfileGUIDs(GUID encodeGUID)
        {
            uint32_t count = 0;
            m_nvenc.nvEncGetEncodeProfileGUIDCount(m_hEncoder, encodeGUID, &count);
            uint32_t validCount = 0;
            std::vector<GUID> guids(count);
            m_nvenc.nvEncGetEncodeProfileGUIDs(m_hEncoder, encodeGUID, guids.data(), count, &validCount);
            return guids;
        }

        int GetLevelMax(GUID encodeGUID) { return GetCapabilityValue(encodeGUID, NV_ENC_CAPS_LEVEL_MAX); }
        int GetLevelMin(GUID encodeGUID) { return GetCapabilityValue(encodeGUID, NV_ENC_CAPS_LEVEL_MIN); }
        int GetMaxWidth(GUID encodeGUID) { return GetCapabilityValue(encodeGUID, NV_ENC_CAPS_WIDTH_MAX); }
        int GetMaxHeight(GUID encodeGUID) { return GetCapabilityValue(encodeGUID, NV_ENC_CAPS_HEIGHT_MAX); }
        int GetEncoderCount(GUID encodeGUID) { return GetCapabilityValue(encodeGUID, NV_ENC_CAPS_NUM_ENCODER_ENGINES); }
    };

    int SupportedEncoderCount(CUcontext context)
    {
        auto encoder = std::make_unique<NvEncoderCudaCapability>(context);
        return encoder->GetEncoderCount(NV_ENC_CODEC_H264_GUID);
    }

    H264Level SupportedMaxH264Level(CUcontext context)
    {
        auto encoder = std::make_unique<NvEncoderCudaCapability>(context);

        int maxLevel = encoder->GetLevelMax(NV_ENC_CODEC_H264_GUID);
        // The max profile level supported by almost browsers is 5.2.
        maxLevel = std::min(maxLevel, 52);
        return static_cast<H264Level>(maxLevel);
    }

    std::vector<SdpVideoFormat> SupportedNvEncoderCodecs(CUcontext context)
    {
        auto encoder = std::make_unique<NvEncoderCudaCapability>(context);

        int maxLevel = encoder->GetLevelMax(NV_ENC_CODEC_H264_GUID);
        // The max profile level supported by almost browsers is 5.2.
        maxLevel = std::min(maxLevel, 52);
        H264Level supportedMaxLevel = static_cast<H264Level>(maxLevel);

        std::vector<GUID> profileGUIDs = encoder->GetEncodeProfileGUIDs(NV_ENC_CODEC_H264_GUID);

        std::vector<H264Profile> supportedProfiles;
        for (auto& guid : profileGUIDs)
        {
            absl::optional<H264Profile> profile = GuidToProfile(guid);
            if (profile.has_value())
                supportedProfiles.push_back(profile.value());
        }

        std::vector<SdpVideoFormat> supportedFormats;
        for (auto& profile : supportedProfiles)
        {
            supportedFormats.push_back(CreateH264Format(profile, supportedMaxLevel, "1"));
        }
        return supportedFormats;
    }

    static int GetCudaDeviceCapabilityMajorVersion(CUcontext context)
    {
        cuCtxSetCurrent(context);

        CUdevice device;
        cuCtxGetDevice(&device);

        int major;
        cuDeviceGetAttribute(&major, CU_DEVICE_ATTRIBUTE_COMPUTE_CAPABILITY_MAJOR, device);

        return major;
    }

    std::vector<SdpVideoFormat> SupportedNvDecoderCodecs(CUcontext context)
    {
        std::vector<SdpVideoFormat> supportedFormats;

        // HardwareGeneration Kepler is 3.x
        // https://docs.nvidia.com/deploy/cuda-compatibility/index.html#faq
        // Kepler support h264 profile Main, Highprofile up to Level4.1
        // https://docs.nvidia.com/video-technologies/video-codec-sdk/nvdec-video-decoder-api-prog-guide/index.html#video-decoder-capabilities__table_o3x_fms_3lb
        if (GetCudaDeviceCapabilityMajorVersion(context) <= 3)
        {
            supportedFormats = {
                CreateH264Format(webrtc::H264Profile::kProfileHigh, webrtc::H264Level::kLevel4_1, "1"),
                CreateH264Format(webrtc::H264Profile::kProfileMain, webrtc::H264Level::kLevel4_1, "1"),
            };
        }
        else
        {
            supportedFormats = {
                // Constrained Baseline Profile does not support NvDecoder, but WebRTC uses this profile by default,
                // so it must be returned in this method.
                CreateH264Format(webrtc::H264Profile::kProfileConstrainedBaseline, webrtc::H264Level::kLevel5_1, "1"),
                CreateH264Format(webrtc::H264Profile::kProfileBaseline, webrtc::H264Level::kLevel5_1, "1"),
                CreateH264Format(webrtc::H264Profile::kProfileHigh, webrtc::H264Level::kLevel5_1, "1"),
                CreateH264Format(webrtc::H264Profile::kProfileMain, webrtc::H264Level::kLevel5_1, "1"),
            };
        }

        for (auto& format : supportedFormats)
        {
            format.parameters.emplace(kSdpKeyNameCodecImpl, kCodecName);
        }

        return supportedFormats;
    }

    std::unique_ptr<NvEncoder> NvEncoder::Create(
        const cricket::VideoCodec& codec,
        CUcontext context,
        CUmemorytype memoryType,
        NV_ENC_BUFFER_FORMAT format,
        ProfilerMarkerFactory* profiler)
    {
        return std::make_unique<NvEncoderImpl>(codec, context, memoryType, format, profiler);
    }

    bool NvEncoder::IsSupported(CUcontext context)
    {
        uint32_t version = 0;
        uint32_t currentVersion = (NVENCAPI_MAJOR_VERSION << 4) | NVENCAPI_MINOR_VERSION;
        NVENCSTATUS result = NvEncodeAPIGetMaxSupportedVersion(&version);
        if (result != NV_ENC_SUCCESS || currentVersion > version)
        {
            return false;
        }

// Check if this device can get the function list of nvencoder API
#pragma GCC diagnostic ignored "-Wmissing-field-initializers"
        NV_ENCODE_API_FUNCTION_LIST funclist = { NV_ENCODE_API_FUNCTION_LIST_VER };
        result = NvEncodeAPICreateInstance(&funclist);
        if (result != NV_ENC_SUCCESS || funclist.nvEncOpenEncodeSession == nullptr)
        {
            return false;
        }

// Check if this device can open encode session
#pragma GCC diagnostic ignored "-Wmissing-field-initializers"
        NV_ENC_OPEN_ENCODE_SESSION_EX_PARAMS encodeSessionExParams = { NV_ENC_OPEN_ENCODE_SESSION_EX_PARAMS_VER };
        encodeSessionExParams.device = context;
        encodeSessionExParams.deviceType = NV_ENC_DEVICE_TYPE_CUDA;
        encodeSessionExParams.apiVersion = NVENCAPI_VERSION;
        void* hEncoder = nullptr;
        result = funclist.nvEncOpenEncodeSessionEx(&encodeSessionExParams, &hEncoder);
        if (result != NV_ENC_SUCCESS || hEncoder == nullptr)
        {
            return false;
        }

        funclist.nvEncDestroyEncoder(hEncoder);
        hEncoder = nullptr;
        return true;
    }

    std::unique_ptr<NvDecoder>
    NvDecoder::Create(const cricket::VideoCodec& codec, CUcontext context, ProfilerMarkerFactory* profiler)
    {
        return std::make_unique<NvDecoderImpl>(context, profiler);
    }

    NvEncoderFactory::NvEncoderFactory(CUcontext context, NV_ENC_BUFFER_FORMAT format, ProfilerMarkerFactory* profiler)
        : context_(context)
        , format_(format)
        , profiler_(profiler)
    {
        // Some NVIDIA GPUs have a limited Encode Session count.
        // refer: https://developer.nvidia.com/video-encode-and-decode-gpu-support-matrix-new
        // It consumes a session to check the encoder capability.
        // Therefore, we check encoder capability only once in the constructor and cache it.
        m_cachedSupportedFormats = SupportedNvEncoderCodecs(context_);
    }
    NvEncoderFactory::~NvEncoderFactory() = default;

    std::vector<SdpVideoFormat> NvEncoderFactory::GetSupportedFormats() const
    {
        // If NvCodec Encoder is not supported, return empty vector.
        if (m_cachedSupportedFormats.empty())
            return std::vector<SdpVideoFormat>();

        // In RTCRtpTransceiver.SetCodecPreferences, the codec passed must be supported by both encoder and decoder.
        // https://source.chromium.org/chromium/chromium/src/+/main:third_party/webrtc/pc/rtp_transceiver.cc;l=36
        // About H264, Profile and its Level must also match in this implementation.
        // NvEncoder supports a higher level of H264 Profile than NvDecoder.
        // Therefore, return the support codec of NvDecoder as Workaround.
        return SupportedNvDecoderCodecs(context_);
    }

    std::unique_ptr<VideoEncoder> NvEncoderFactory::CreateVideoEncoder(const SdpVideoFormat& format)
    {
        // todo(kazuki):: add CUmemorytype::CU_MEMORYTYPE_DEVICE option
        return NvEncoder::Create(cricket::CreateVideoCodec(format), context_, CU_MEMORYTYPE_ARRAY, format_, profiler_);
    }

    NvDecoderFactory::NvDecoderFactory(CUcontext context, ProfilerMarkerFactory* profiler)
        : context_(context)
        , profiler_(profiler)
    {
    }
    NvDecoderFactory::~NvDecoderFactory() = default;

    std::vector<SdpVideoFormat> NvDecoderFactory::GetSupportedFormats() const
    {
        return SupportedNvDecoderCodecs(context_);
    }

    std::unique_ptr<VideoDecoder> NvDecoderFactory::CreateVideoDecoder(const SdpVideoFormat& format)
    {
        return NvDecoder::Create(cricket::CreateVideoCodec(format), context_, profiler_);
    }
}
}
