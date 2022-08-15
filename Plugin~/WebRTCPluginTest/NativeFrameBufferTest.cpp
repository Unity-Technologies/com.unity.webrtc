#include "pch.h"

#include "GpuMemoryBuffer.h"
#include "GraphicsDevice/IGraphicsDevice.h"
#include "GraphicsDevice/ITexture2D.h"
#include "GraphicsDeviceContainer.h"
#include "Size.h"
#include "UnityVideoTrackSource.h"
#include "VideoFrameAdapter.h"
#include "VideoFrameUtil.h"

#if UNITY_OSX || UNITY_IOS
#else
#include "NativeFrameBuffer.h"
#endif

namespace unity
{
namespace webrtc
{

    class NativeFrameBufferTest : public testing::TestWithParam<UnityGfxRenderer>
    {
    public:
        explicit NativeFrameBufferTest()
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
        const Size kSize = { static_cast<int>(kWidth), static_cast<int>(kHeight) };
        const UnityRenderingExtTextureFormat kFormat = kUnityRenderingExtFormatR8G8B8A8_SRGB;
    };

    TEST_P(NativeFrameBufferTest, WidthAndHeight)
    {
        auto buffer =  NativeFrameBuffer::Create(kWidth, kHeight, kFormat, device_);
        EXPECT_TRUE(buffer);
        EXPECT_EQ(buffer->width(), kWidth);
        EXPECT_EQ(buffer->height(), kHeight);
    }

    TEST_P(NativeFrameBufferTest, Handle)
    {
        auto buffer =  NativeFrameBuffer::Create(kWidth, kHeight, kFormat, device_);
#if UNITY_OSX || UNITY_IOS
#else
        NativeFrameBuffer* nativeFrameBuffer = static_cast<NativeFrameBuffer*>(buffer.get());
        auto handle = nativeFrameBuffer->handle();
        ASSERT_TRUE(handle);
#endif
    }

    INSTANTIATE_TEST_SUITE_P(GfxDevice, NativeFrameBufferTest, testing::ValuesIn(supportedGfxDevices));

} // end namespace webrtc
} // end namespace unity
