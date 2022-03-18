#include "pch.h"

#include "NvCodec.h"
#include "NvEncoder/NvEncoderCuda.h"
#include "NvEncoderImpl.h"
#include "NvDecoder/NvDecoder.h"
#include "NvDecoderImpl.h"

#include "absl/strings/match.h"
#include "api/video_codecs/video_encoder_factory.h"

namespace unity
{
namespace webrtc
{
    using namespace ::webrtc;

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

    int GetCudaDeviceCapabilityMajorVersion(CUcontext context)
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
                CreateH264Format(webrtc::H264Profile::kProfileConstrainedHigh, webrtc::H264Level::kLevel4_1, "1"),
                CreateH264Format(webrtc::H264Profile::kProfileHigh, webrtc::H264Level::kLevel4_1, "1"),
                CreateH264Format(webrtc::H264Profile::kProfileMain, webrtc::H264Level::kLevel4_1, "1"),
            };
        }
        else
        {
            supportedFormats = {
                CreateH264Format(webrtc::H264Profile::kProfileBaseline, webrtc::H264Level::kLevel5_1, "1"),
                CreateH264Format(webrtc::H264Profile::kProfileConstrainedBaseline, webrtc::H264Level::kLevel5_1, "1"),
                CreateH264Format(webrtc::H264Profile::kProfileConstrainedHigh, webrtc::H264Level::kLevel5_1, "1"),
                CreateH264Format(webrtc::H264Profile::kProfileHigh, webrtc::H264Level::kLevel5_1, "1"),
                CreateH264Format(webrtc::H264Profile::kProfileMain, webrtc::H264Level::kLevel5_1, "1"),
            };
        }

        return supportedFormats;
    }

    std::unique_ptr<NvEncoder> NvEncoder::Create(
        const cricket::VideoCodec& codec, CUcontext context, CUmemorytype memoryType, NV_ENC_BUFFER_FORMAT format)
    {
        return std::make_unique<NvEncoderImpl>(codec, context, memoryType, format);
    }

    std::unique_ptr<NvDecoder> NvDecoder::Create(const cricket::VideoCodec& codec, CUcontext context)
    {
        return std::make_unique<NvDecoderImpl>(context);
    }

    NvEncoderFactory::NvEncoderFactory(CUcontext context, NV_ENC_BUFFER_FORMAT format)
        : context_(context)
        , format_(format)
    {
    }
    NvEncoderFactory::~NvEncoderFactory() = default;

    std::vector<SdpVideoFormat> NvEncoderFactory::GetSupportedFormats() const
    {
        return SupportedNvEncoderCodecs(context_);
    }

    VideoEncoderFactory::CodecInfo NvEncoderFactory::QueryVideoEncoder(const SdpVideoFormat& format) const
    {
        return VideoEncoderFactory::CodecInfo();
    }

    std::unique_ptr<VideoEncoder> NvEncoderFactory::CreateVideoEncoder(const SdpVideoFormat& format)
    {
        // todo(kazuki):: add CUmemorytype::CU_MEMORYTYPE_DEVICE option
        return NvEncoder::Create(cricket::VideoCodec(format), context_, CU_MEMORYTYPE_ARRAY, format_);
    }

    NvDecoderFactory::NvDecoderFactory(CUcontext context)
        : context_(context)
    {
    }
    NvDecoderFactory::~NvDecoderFactory() = default;

    std::vector<SdpVideoFormat> NvDecoderFactory::GetSupportedFormats() const
    {
        return SupportedNvDecoderCodecs(context_);
    }

    std::unique_ptr<VideoDecoder> NvDecoderFactory::CreateVideoDecoder(const SdpVideoFormat& format)
    {
        return NvDecoder::Create(cricket::VideoCodec(format), context_);
    }
}
}
