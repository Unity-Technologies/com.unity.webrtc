#include "pch.h"

#include "Codec/MediaCodec/MediaCodecEncoderImpl.h"
#include "GraphicsDevice/IGraphicsDevice.h"
#include "GraphicsDeviceContainer.h"
#include "VideoCodecTest.h"

namespace unity
{
namespace webrtc
{
    using namespace webrtc;
    using testing::Values;

    class MediaCodecEncoderImplTest : public testing::TestWithParam<UnityGfxRenderer>
    {
    public:
        MediaCodecEncoderImplTest()
            : container_(CreateGraphicsDeviceContainer(GetParam()))
            , device_(container_->device())
        {
        }

        ~MediaCodecEncoderImplTest() override { }
        void SetUp() override
        {
            if (!device_)
                GTEST_SKIP() << "The graphics driver is not installed on the device.";
        }

    protected:
        std::unique_ptr<GraphicsDeviceContainer> container_;
        IGraphicsDevice* device_;
    };

    TEST_P(MediaCodecEncoderImplTest, CanInitializeWithDefaultParameters)
    {
        cricket::VideoCodec codec = cricket::VideoCodec(cricket::kH264CodecName);
        codec.SetParam(cricket::kH264FmtpProfileLevelId, kProfileLevelIdString());
        MediaCodecEncoderImpl encoder(codec, device_, nullptr);

        VideoCodec codec_settings;
        SetDefaultSettings(&codec_settings);
        EXPECT_EQ(WEBRTC_VIDEO_CODEC_OK, encoder.InitEncode(&codec_settings, kSettings()));
    }

    INSTANTIATE_TEST_SUITE_P(GfxDevice, MediaCodecEncoderImplTest, testing::ValuesIn(supportedGfxDevices));
}
}