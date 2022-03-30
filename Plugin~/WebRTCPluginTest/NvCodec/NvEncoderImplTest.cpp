#include "pch.h"

#include "Codec/NvCodec/NvEncoderImpl.h"
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

    class NvEncoderImplTest : public testing::TestWithParam<UnityGfxRenderer>
    {
    public:
        NvEncoderImplTest()
            : container_(CreateGraphicsDeviceContainer(GetParam()))
            , device_(container_->device())
        {
        }
        ~NvEncoderImplTest() override { }

    protected:
        void SetUp() override
        {
            if (!device_)
                GTEST_SKIP() << "The graphics driver is not installed on the device.";
            if (!device_->IsCudaSupport())
                GTEST_SKIP() << "CUDA is not supported on this device.";

            context_ = container_->device()->GetCUcontext();
        }
        CUcontext context_;
        std::unique_ptr<GraphicsDeviceContainer> container_;
        IGraphicsDevice* device_;
    };

    TEST_P(NvEncoderImplTest, CanInitializeWithDefaultParameters)
    {
        cricket::VideoCodec codec = cricket::VideoCodec(cricket::kH264CodecName);
        codec.SetParam(cricket::kH264FmtpProfileLevelId, kProfileLevelIdString());
        NvEncoderImpl encoder(codec, context_, CU_MEMORYTYPE_ARRAY, NV_ENC_BUFFER_FORMAT_ARGB);

        VideoCodec codec_settings;
        SetDefaultSettings(&codec_settings);
        EXPECT_EQ(WEBRTC_VIDEO_CODEC_OK, encoder.InitEncode(&codec_settings, kSettings()));
    }

    INSTANTIATE_TEST_SUITE_P(GfxDevice, NvEncoderImplTest, testing::ValuesIn(supportedGfxDevices));

} // end namespace webrtc
} // end namespace unity
