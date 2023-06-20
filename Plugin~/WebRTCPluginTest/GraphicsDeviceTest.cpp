#include "pch.h"

#include <rtc_base/thread.h>

#include "GpuMemoryBuffer.h"
#include "GraphicsDevice/IGraphicsDevice.h"
#include "GraphicsDevice/ITexture2D.h"
#include "GraphicsDeviceTestBase.h"

namespace unity
{
namespace webrtc
{

    class GraphicsDeviceTest : public GraphicsDeviceTestBase
    {
    protected:
        void SetUp() override
        {
            if (!device())
                GTEST_SKIP() << "The graphics driver is not installed on the device.";

            std::unique_ptr<ITexture2D> texture(device()->CreateDefaultTextureV(kWidth, kHeight, format()));
            if (!texture)
                GTEST_SKIP() << "The graphics driver cannot create a texture resource.";
        }

        const uint32_t kWidth = 256;
        const uint32_t kHeight = 256;
    };

    TEST_P(GraphicsDeviceTest, GraphicsDeviceIsNotNull) { EXPECT_NE(nullptr, device()); }

    TEST_P(GraphicsDeviceTest, CreateDefaultTextureV)
    {
        const auto width = 256;
        const auto height = 256;
        const std::unique_ptr<ITexture2D> tex(device()->CreateDefaultTextureV(width, height, format()));
        EXPECT_TRUE(device()->WaitIdleForTest());
        EXPECT_TRUE(tex->IsSize(width, height));
        EXPECT_NE(nullptr, tex->GetNativeTexturePtrV());
        EXPECT_FALSE(tex->IsSize(0, 0));
    }

    TEST_P(GraphicsDeviceTest, CreateCPUReadTextureV)
    {
        const auto width = 256;
        const auto height = 256;
        const std::unique_ptr<ITexture2D> tex(device()->CreateCPUReadTextureV(width, height, format()));
        EXPECT_TRUE(device()->WaitIdleForTest());
        EXPECT_TRUE(tex->IsSize(width, height));
        EXPECT_FALSE(tex->IsSize(0, 0));
    }

    TEST_P(GraphicsDeviceTest, ReleaseTextureOnOtherThread)
    {
        const uint32_t width = 256;
        const uint32_t height = 256;

        std::unique_ptr<rtc::Thread> thread = rtc::Thread::CreateWithSocketServer();
        thread->Start();
        std::unique_ptr<ITexture2D> texture(device()->CreateDefaultTextureV(width, height, format()));
        EXPECT_TRUE(device()->WaitIdleForTest());
        thread->BlockingCall([&]() { texture = nullptr; });
        EXPECT_EQ(texture, nullptr);
    }

    TEST_P(GraphicsDeviceTest, CopyResourceV)
    {
        const auto width = 256;
        const auto height = 256;
        const std::unique_ptr<ITexture2D> src(device()->CreateDefaultTextureV(width, height, format()));
        const std::unique_ptr<ITexture2D> dst(device()->CreateDefaultTextureV(width, height, format()));
        EXPECT_TRUE(device()->WaitIdleForTest());
        EXPECT_TRUE(device()->CopyResourceV(dst.get(), src.get()));
        EXPECT_TRUE(device()->WaitIdleForTest());
    }

    TEST_P(GraphicsDeviceTest, CopyResourceVFromCPURead)
    {
        const auto width = 256;
        const auto height = 256;
        const std::unique_ptr<ITexture2D> src(device()->CreateCPUReadTextureV(width, height, format()));
        const std::unique_ptr<ITexture2D> dst(device()->CreateDefaultTextureV(width, height, format()));
        EXPECT_TRUE(device()->WaitIdleForTest());
        EXPECT_TRUE(device()->CopyResourceV(dst.get(), src.get()));
        EXPECT_TRUE(device()->WaitIdleForTest());
    }

    TEST_P(GraphicsDeviceTest, CopyResourceNativeV)
    {
        const auto width = 256;
        const auto height = 256;
        const std::unique_ptr<ITexture2D> src(device()->CreateDefaultTextureV(width, height, format()));
        const std::unique_ptr<ITexture2D> dst(device()->CreateDefaultTextureV(width, height, format()));
        EXPECT_TRUE(device()->WaitIdleForTest());
        EXPECT_TRUE(device()->CopyResourceFromNativeV(dst.get(), src->GetNativeTexturePtrV()));
        EXPECT_TRUE(device()->WaitIdleForTest());
    }

    TEST_P(GraphicsDeviceTest, ConvertRGBToI420)
    {
        const uint32_t width = 256;
        const uint32_t height = 256;
        const std::unique_ptr<ITexture2D> src(device()->CreateDefaultTextureV(width, height, format()));
        const std::unique_ptr<ITexture2D> dst(device()->CreateCPUReadTextureV(width, height, format()));
        EXPECT_TRUE(device()->WaitIdleForTest());
        EXPECT_TRUE(device()->CopyResourceFromNativeV(dst.get(), src->GetNativeTexturePtrV()));
        EXPECT_TRUE(device()->WaitIdleForTest());
        const auto frameBuffer = device()->ConvertRGBToI420(dst.get());
        EXPECT_NE(nullptr, frameBuffer);
        EXPECT_EQ(width, frameBuffer->width());
        EXPECT_EQ(height, frameBuffer->height());
    }

    TEST_P(GraphicsDeviceTest, Map)
    {
        const uint32_t width = 256;
        const uint32_t height = 256;
        const std::unique_ptr<ITexture2D> src(device()->CreateDefaultTextureV(width, height, format()));
        EXPECT_TRUE(device()->WaitIdleForTest());
        std::unique_ptr<GpuMemoryBufferHandle> handle = device()->Map(src.get());
#if CUDA_PLATFORM
        if (device()->IsCudaSupport())
            EXPECT_NE(handle, nullptr);
        else
            EXPECT_EQ(handle, nullptr);
#else
        EXPECT_EQ(handle, nullptr);
#endif
    }

    TEST_P(GraphicsDeviceTest, MapWithCPUReadTexture)
    {
        // On Vulkan device, the Map method don't success when using the texture
        // which creating for reading from CPU.
        // It is unclear whether this is the bug or the specification of CUDA.
        if (device()->GetGfxRenderer() == kUnityGfxRendererVulkan)
            GTEST_SKIP() << "The Map method throw exception on vulkan platform";

        const uint32_t width = 256;
        const uint32_t height = 256;
        const std::unique_ptr<ITexture2D> src2(device()->CreateCPUReadTextureV(width, height, format()));
        EXPECT_TRUE(device()->WaitIdleForTest());
        std::unique_ptr<GpuMemoryBufferHandle> handle2 = device()->Map(src2.get());
    }

    INSTANTIATE_TEST_SUITE_P(GfxDeviceAndColorSpece, GraphicsDeviceTest, testing::ValuesIn(VALUES_TEST_ENV));

} // end namespace webrtc
} // end namespace unity
