#include "pch.h"

#include "Codec/NvCodec/NvDecoderImpl.h"
#include "GraphicsDevice/IGraphicsDevice.h"
#include "GraphicsDeviceContainer.h"
#include "NvCodecUtils.h"
#include "VideoCodecTest.h"

namespace unity
{
namespace webrtc
{
    using namespace webrtc;
    using testing::Values;

    class NvDecoderImplTest : public testing::TestWithParam<UnityGfxRenderer>
    {
    public:
        NvDecoderImplTest()
        {
            container_ = CreateGraphicsDeviceContainer(GetParam());
            context_ = container_->device()->GetCUcontext();
        }
        ~NvDecoderImplTest() override { }

    protected:
        CUcontext context_;
        std::unique_ptr<GraphicsDeviceContainer> container_;
    };

    TEST_P(NvDecoderImplTest, CanInitializeWithDefaultParameters)
    {
        NvDecoderImpl decoder(context_);

        VideoCodec codec_settings;
        SetDefaultSettings(&codec_settings);
        EXPECT_EQ(WEBRTC_VIDEO_CODEC_OK, decoder.InitDecode(&codec_settings, 1));
    }

    INSTANTIATE_TEST_SUITE_P(GfxDevice, NvDecoderImplTest, testing::ValuesIn(supportedGfxDevices));

} // end namespace webrtc
} // end namespace unity
