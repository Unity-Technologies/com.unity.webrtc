#pragma once

namespace unity
{
namespace webrtc
{
    class IGraphicsDevice;
    using namespace ::webrtc;

    // This class is only used for status testing.
    class UnityVideoDecoderFactory : public VideoDecoderFactory
    {
    public:
        std::vector<webrtc::SdpVideoFormat> GetSupportedFormats() const override;
        std::unique_ptr<webrtc::VideoDecoder> CreateVideoDecoder(const webrtc::SdpVideoFormat& format) override;
        UnityVideoDecoderFactory(IGraphicsDevice* gfxDevice);
    private:
        const std::unique_ptr<VideoDecoderFactory> internal_decoder_factory_;
        const std::unique_ptr<VideoDecoderFactory> native_decoder_factory_;
    };
}
}
