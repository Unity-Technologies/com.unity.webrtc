#include "pch.h"

#include "Codec/NvCodec/NvCodec.h"
#include "FrameGenerator.h"
#include "GraphicsDevice/IGraphicsDevice.h"
#include "GraphicsDeviceContainer.h"
#include "NvCodecUtils.h"
#include "VideoCodecTest.h"

namespace unity
{
namespace webrtc
{
    using testing::Values;

    class InternalCodecsTest : public VideoCodecTest
    {
    public:
        InternalCodecsTest() { container_ = CreateGraphicsDeviceContainer(GetParam()); }
        ~InternalCodecsTest() override
        {
            if (encoder_)
                encoder_ = nullptr;
        }

    protected:
        SdpVideoFormat FindFormat(std::string name, const std::vector<SdpVideoFormat>& formats)
        {
            auto result = std::find_if(
                formats.begin(),
                formats.end(), [name](SdpVideoFormat& x) { return x.name == name; });
            return *result;
        }

        std::unique_ptr<VideoEncoder> CreateEncoder() override
        {
            SdpVideoFormat format = FindFormat(codecName, encoderFactory.SupportedFormats());
            return encoderFactory.CreateVideoEncoder(format);
        }

        std::unique_ptr<VideoDecoder> CreateDecoder() override
        {
            SdpVideoFormat format = FindFormat(codecName, decoderFactory.GetSupportedFormats());
            return decoderFactory.CreateVideoDecoder(format);
        }

        std::string codecName = "VP8";
        InternalEncoderFactory encoderFactory;
        InternalDecoderFactory decoderFactory;
        std::unique_ptr<GraphicsDeviceContainer> container_;
    };

}
}
