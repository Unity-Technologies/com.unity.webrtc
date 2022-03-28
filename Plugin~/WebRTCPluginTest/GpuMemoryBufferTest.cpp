#include "pch.h"

#include "GpuMemoryBuffer.h"
#include "GraphicsDeviceContainer.h"
#include "Size.h"
#include "UnityVideoTrackSource.h"
#include "VideoFrameUtil.h"
#include "GraphicsDevice/IGraphicsDevice.h"
#include "GraphicsDevice/ITexture2D.h"

namespace unity
{
namespace webrtc
{

    class GpuMemoryBufferTest : public testing::TestWithParam<UnityGfxRenderer>
    {
    public:
        explicit GpuMemoryBufferTest()
            : container_(CreateGraphicsDeviceContainer(GetParam()))
            , device_(container_->device())
        {
        }

    protected:
        void SetUp() override
        {
            if (!device_)
                GTEST_SKIP() << "The graphics driver is not installed on the device.";
            if (!device_->IsCudaSupport())
                GTEST_SKIP() << "CUDA is not supported on this device.";
        }
        std::unique_ptr<GraphicsDeviceContainer> container_;
        IGraphicsDevice* device_;
    };

    TEST_P(GpuMemoryBufferTest, WidthAndHeight)
    {
        const Size kSize(1280, 960);
        const UnityRenderingExtTextureFormat kFormat = kUnityRenderingExtFormatR8G8B8A8_SRGB;
        IGraphicsDevice* device = container_->device();
        std::unique_ptr<const ITexture2D> texture(
            device->CreateDefaultTextureV(kSize.width(), kSize.height(), kFormat));
        auto testFrame = CreateTestFrame(device, texture.get(), kFormat);

        auto frame = VideoFrameAdapter::CreateVideoFrame(testFrame);
        auto buffer = frame.video_frame_buffer();

        EXPECT_EQ(buffer->width(), kSize.width());
        EXPECT_EQ(buffer->height(), kSize.height());
    }

    INSTANTIATE_TEST_SUITE_P(GfxDevice, GpuMemoryBufferTest, testing::ValuesIn(supportedGfxDevices));

} // end namespace webrtc
} // end namespace unity
