#pragma once

namespace unity
{
namespace webrtc
{
    constexpr char kInternalImpl[] = "Internal";
    constexpr char kNvCodecImpl[] = "NvCodec";
    constexpr char kAndroidMediaCodecImpl[] = "MediaCodec";
    constexpr char kVideoToolboxImpl[] = "VideoToolbox";

    constexpr char kSdpKeyNameCodecImpl[] = "implementation_name";

    class VideoEncoderFactory;
    class IGraphicsDevice;
    static VideoEncoderFactory* CreateEncoderFactory(const std::string& impl, IGraphicsDevice* gfxDevice);
    static VideoDecoderFactory* CreateDecoderFactory(const std::string& impl, IGraphicsDevice* gfxDevice);
}
}
