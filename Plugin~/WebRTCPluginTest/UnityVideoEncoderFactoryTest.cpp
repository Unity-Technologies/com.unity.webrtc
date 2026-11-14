#include "pch.h"

#include <api/make_ref_counted.h>
#include <api/video/video_frame.h>
#include <api/video_codecs/video_encoder.h>
#include <modules/video_coding/include/video_error_codes.h>

#include "GraphicsDevice/IGraphicsDevice.h"
#include "GraphicsDeviceContainer.h"
#include "UnityVideoEncoderFactory.h"

namespace unity
{
namespace webrtc
{
    class UnityVideoEncoderFactoryTest : public testing::TestWithParam<UnityGfxRenderer>
    {
    public:
        UnityVideoEncoderFactoryTest()
            : container_(CreateGraphicsDeviceContainer(GetParam()))
            , device_(container_->device())
        {
        }

    protected:
        void SetUp() override
        {
            if (!device_)
                GTEST_SKIP() << "The graphics driver is not installed on the device.";
        }
        std::unique_ptr<GraphicsDeviceContainer> container_;
        IGraphicsDevice* device_;
    };

    TEST_P(UnityVideoEncoderFactoryTest, GetSupportedFormats)
    {
        auto factory = std::make_unique<UnityVideoEncoderFactory>(container_->device(), nullptr);
        auto formats = factory->GetSupportedFormats();
        EXPECT_GT(formats.size(), 0);
    }

    // The encoder must reject a failed I420 conversion even when no Unity profiler is installed.
    class FailedI420Buffer : public VideoFrameBuffer
    {
    public:
        Type type() const override { return Type::kI420; }
        int width() const override { return 16; }
        int height() const override { return 16; }
        rtc::scoped_refptr<I420BufferInterface> ToI420() override { return nullptr; }
        const I420BufferInterface* GetI420() const override { return nullptr; }
    };

    TEST(UnityVideoEncoderReadbackTest, DropsFailedReadbackWithoutProfiler)
    {
        UnityVideoEncoderFactory factory(nullptr, nullptr);
        auto encoder = factory.CreateVideoEncoder(SdpVideoFormat("VP8"));
        ASSERT_NE(encoder, nullptr);
        auto frame = ::webrtc::VideoFrame::Builder()
                         .set_video_frame_buffer(rtc::make_ref_counted<FailedI420Buffer>())
                         .build();

        EXPECT_EQ(encoder->Encode(frame, nullptr), WEBRTC_VIDEO_CODEC_ERROR);
    }

    INSTANTIATE_TEST_SUITE_P(GfxDevice, UnityVideoEncoderFactoryTest, testing::ValuesIn(supportedGfxDevices));
} // namespace webrtc
} // namespace unity
