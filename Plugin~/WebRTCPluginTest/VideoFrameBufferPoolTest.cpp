#include "pch.h"

#include "VideoFrameBufferPool.h"
#include "GraphicsDevice/ITexture2D.h"
#include "GraphicsDevice/IGraphicsDevice.h"
#include "GraphicsDeviceContainer.h"

namespace unity
{
namespace webrtc
{
    class VideoFrameBufferPoolTest : public testing::TestWithParam<UnityGfxRenderer>
    {
    public:
        VideoFrameBufferPoolTest()
            : container_(CreateGraphicsDeviceContainer(GetParam()))
            , device_(container_->device())
        {
        }
        ~VideoFrameBufferPoolTest() override = default;
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

    TEST_P(VideoFrameBufferPoolTest, SimpleFrameReuse)
    {
        VideoFrameBufferPool pool(device_, 1);
        auto buffer = pool.Create(kWidth, kHeight, kFormat);
        EXPECT_TRUE(buffer);
        EXPECT_EQ(kWidth, buffer->width());
        EXPECT_EQ(kHeight, buffer->height());
        EXPECT_EQ(VideoFrameBuffer::Type::kNative, buffer->type());

        // return buffer to the pool
        buffer = nullptr;

        // reuse buffer
        buffer = pool.Create(kWidth, kHeight, kFormat);
        EXPECT_TRUE(buffer);
    }

    TEST_P(VideoFrameBufferPoolTest, FailToReuseWrongSize)
    {
        // Set max frames to 1, just to make sure the first buffer is being released.
        VideoFrameBufferPool pool(device_, 1);
        auto buffer = pool.Create(kWidth, kHeight, kFormat);
        EXPECT_EQ(kWidth, buffer->width());
        EXPECT_EQ(kHeight, buffer->height());

        // Release buffer so that it is returned to the pool.
        buffer = nullptr;

        // Check that the pool doesn't try to reuse buffers of incorrect size.
        uint32_t width = kWidth * 2;
        buffer = pool.Create(width, kHeight, kFormat);
        ASSERT_TRUE(buffer);
        EXPECT_EQ(width, buffer->width());
        EXPECT_EQ(kHeight, buffer->height());
    }

    TEST_P(VideoFrameBufferPoolTest, MaxNumberOfBuffers)
    {
        VideoFrameBufferPool pool(device_, 1);
        auto buffer = pool.Create(kWidth, kHeight, kFormat);
        EXPECT_NE(nullptr, buffer.get());
        EXPECT_EQ(nullptr, pool.Create(kWidth, kHeight, kFormat).get());
    }
    INSTANTIATE_TEST_SUITE_P(GfxDevice, VideoFrameBufferPoolTest, testing::ValuesIn(supportedGfxDevices));
}
}
