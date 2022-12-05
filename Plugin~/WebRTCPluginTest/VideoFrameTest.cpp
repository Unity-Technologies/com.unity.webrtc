#include "pch.h"

#include "GpuMemoryBuffer.h"
#include "VideoFrameUtil.h"

#include "GraphicsDevice/ITexture2D.h"
#include "GraphicsDeviceContainer.h"

namespace unity
{
namespace webrtc
{
    class VideoFrameTest : public testing::TestWithParam<UnityGfxRenderer>
    {
    public:
        explicit VideoFrameTest()
            : container_(CreateGraphicsDeviceContainer(GetParam()))
            , device_(container_->device())
        {
        }

    protected:
        void SetUp() override
        {
            if (!device_)
                GTEST_SKIP() << "The graphics driver is not installed on the device.";

            std::unique_ptr<ITexture2D> texture(device_->CreateDefaultTextureV(kWidth, kHeight, kFormat));
            EXPECT_TRUE(device_->WaitIdleForTest());

            if (!texture)
                GTEST_SKIP() << "The graphics driver cannot create a texture resource.";
        }
        std::unique_ptr<GraphicsDeviceContainer> container_;
        IGraphicsDevice* device_;
        const uint32_t kWidth = 256;
        const uint32_t kHeight = 256;
        const Size kSize = { static_cast<int>(kWidth), static_cast<int>(kHeight) };
        const UnityRenderingExtTextureFormat kFormat = kUnityRenderingExtFormatR8G8B8A8_SRGB;
    };

    TEST_P(VideoFrameTest, WrapExternalGpuMemoryBuffer)
    {
        std::unique_ptr<ITexture2D> tex =
            std::unique_ptr<ITexture2D>(device_->CreateDefaultTextureV(kWidth, kHeight, kFormat));
        EXPECT_TRUE(device_->WaitIdleForTest());
        rtc::scoped_refptr<VideoFrame> videoFrame = CreateTestFrame(device_, tex.get(), kFormat);
        EXPECT_TRUE(device_->WaitIdleForTest());
        ASSERT_NE(videoFrame, nullptr);
    }

    INSTANTIATE_TEST_SUITE_P(GfxDevice, VideoFrameTest, testing::ValuesIn(supportedGfxDevices));

} // end namespace webrtc
} // end namespace unity
