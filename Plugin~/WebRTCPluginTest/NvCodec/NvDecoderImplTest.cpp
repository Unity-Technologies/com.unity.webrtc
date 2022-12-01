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
            : container_(CreateGraphicsDeviceContainer(GetParam()))
            , device_(container_->device())
        {
        }

        ~NvDecoderImplTest() override { }
        void SetUp() override
        {
            if (!device_)
                GTEST_SKIP() << "The graphics driver is not installed on the device.";
            if (!device_->IsCudaSupport())
                GTEST_SKIP() << "CUDA is not supported on this device.";

            context_ = device_->GetCUcontext();
        }

    protected:
        CUcontext context_;
        std::unique_ptr<GraphicsDeviceContainer> container_;
        IGraphicsDevice* device_;
    };

    TEST_P(NvDecoderImplTest, CanInitializeWithDefaultParameters)
    {
        NvDecoderImpl decoder(context_, nullptr);

        VideoDecoder::Settings codec_settings;
        SetDefaultSettings(codec_settings);
        EXPECT_TRUE(decoder.Configure(codec_settings));
    }

    INSTANTIATE_TEST_SUITE_P(GfxDevice, NvDecoderImplTest, testing::ValuesIn(supportedGfxDevices));

} // end namespace webrtc
} // end namespace unity
