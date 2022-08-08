#pragma once

#include <api/video_codecs/video_decoder_factory.h>
#include <api/video_codecs/video_encoder_factory.h>
#include <string>

namespace unity
{
namespace webrtc
{
    using namespace ::webrtc;

    constexpr char kInternalImpl[] = "Internal";
    constexpr char kNvCodecImpl[] = "NvCodec";
    constexpr char kAndroidMediaCodecImpl[] = "MediaCodec";
    constexpr char kVideoToolboxImpl[] = "VideoToolbox";

    constexpr char kSdpKeyNameCodecImpl[] = "implementation_name";

    class IGraphicsDevice;
    class ProfilerMarkerFactory;
    VideoEncoderFactory*
    CreateVideoEncoderFactory(const std::string& impl, IGraphicsDevice* gfxDevice, ProfilerMarkerFactory* profiler);
    VideoDecoderFactory*
    CreateVideoDecoderFactory(const std::string& impl, IGraphicsDevice* gfxDevice, ProfilerMarkerFactory* profiler);

    template<typename Factory>
    Factory* FindCodecFactory(
        const std::map<std::string, std::unique_ptr<Factory>>& factories, const webrtc::SdpVideoFormat& format)
    {
        for (const auto& pair : factories)
        {
            for (const webrtc::SdpVideoFormat& other : pair.second->GetSupportedFormats())
            {
                if (format.IsSameCodec(other))
                    return pair.second.get();
            }
        }
        return nullptr;
    }

    template<typename Factory>
    std::vector<webrtc::SdpVideoFormat>
    GetSupportedFormatsInFactories(const std::map<std::string, std::unique_ptr<Factory>>& factories)
    {
        std::vector<SdpVideoFormat> supported_codecs;
        for (const auto& pair : factories)
        {
            for (const webrtc::SdpVideoFormat& format : pair.second->GetSupportedFormats())
            {
                webrtc::SdpVideoFormat newFormat = format;
                if (!pair.first.empty())
                    newFormat.parameters.emplace(kSdpKeyNameCodecImpl, pair.first);
                supported_codecs.push_back(newFormat);
            }
        }
        return supported_codecs;
    }
}
}
