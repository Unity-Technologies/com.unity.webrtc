#include "pch.h"

#include "GpuMemoryBuffer.h"
#include "VideoFrameUtil.h"

#include "GraphicsDevice/ITexture2D.h"
#include "GraphicsDeviceContainer.h"

//#include "api/utils/timedelta.h"

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
            if (!texture)
                GTEST_SKIP() << "The graphics driver cannot create a texture resource.";
        }
        std::unique_ptr<GraphicsDeviceContainer> container_;
        IGraphicsDevice* device_;
        const uint32_t kWidth = 256;
        const uint32_t kHeight = 256;
        const UnityRenderingExtTextureFormat kFormat = kUnityRenderingExtFormatR8G8B8A8_SRGB;
    };

    TEST_P(VideoFrameTest, WrapExternalGpuMemoryBuffer)
    {
        const Size kSize(kWidth, kHeight);
        std::unique_ptr<ITexture2D> tex = std::unique_ptr<ITexture2D>(
            container_->device()->CreateDefaultTextureV(kSize.width(), kSize.height(), kFormat));
        rtc::scoped_refptr<VideoFrame> videoFrame = CreateTestFrame(container_->device(), tex.get(), kFormat);
        ASSERT_NE(videoFrame, nullptr);
    }

    INSTANTIATE_TEST_SUITE_P(GfxDevice, VideoFrameTest, testing::ValuesIn(supportedGfxDevices));

} // end namespace webrtc
} // end namespace unity
