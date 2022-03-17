#include "pch.h"

#include "GpuMemoryBuffer.h"
#include "GraphicsDevice/IGraphicsDevice.h"
#include "GraphicsDevice/ITexture2D.h"
#include "GraphicsDeviceTestBase.h"

namespace unity
{
namespace webrtc
{

class GraphicsDeviceTest : public GraphicsDeviceTestBase {};
TEST_P(GraphicsDeviceTest, GraphicsDeviceIsNotNull) {
    EXPECT_NE(nullptr, device());
}

TEST_P(GraphicsDeviceTest, CreateDefaultTextureV) {
    const auto width = 256;
    const auto height = 256;
    const std::unique_ptr<ITexture2D> tex(device()->CreateDefaultTextureV(width, height, m_textureFormat));
    EXPECT_TRUE(tex->IsSize(width, height));
    EXPECT_NE(nullptr, tex->GetNativeTexturePtrV());
    EXPECT_FALSE(tex->IsSize(0, 0));
}

#if defined(SUPPORT_SOFTWARE_ENCODER)
TEST_P(GraphicsDeviceTest, CreateCPUReadTextureV) {
    const auto width = 256;
    const auto height = 256;
    const std::unique_ptr<ITexture2D> tex(device()->CreateCPUReadTextureV(width, height, m_textureFormat));
    EXPECT_TRUE(tex->IsSize(width, height));
    EXPECT_NE(nullptr, tex->GetNativeTexturePtrV());
    EXPECT_FALSE(tex->IsSize(0, 0));
}
#endif

//[Note-sin: 2019-12-19] Real Unity Interface is required for testing the following functions, and it is not 
//possible to create a dummy Unity interface (with its command buffer) on Metal devices
#if !defined(SUPPORT_METAL) && !defined(SUPPORT_OPENGL_CORE)

TEST_P(GraphicsDeviceTest, CopyResourceV) {
    const auto width = 256;
    const auto height = 256;
    const std::unique_ptr<ITexture2D> src(device()->CreateDefaultTextureV(width, height, m_textureFormat));
    const std::unique_ptr<ITexture2D> dst(device()->CreateDefaultTextureV(width, height, m_textureFormat));
    EXPECT_TRUE(device()->CopyResourceV(dst.get(), src.get()));
}

TEST_P(GraphicsDeviceTest, CopyResourceVFromCPURead)
{
    const auto width = 256;
    const auto height = 256;
    const std::unique_ptr<ITexture2D> src(device()->CreateCPUReadTextureV(width, height, m_textureFormat));
    const std::unique_ptr<ITexture2D> dst(device()->CreateDefaultTextureV(width, height, m_textureFormat));
    EXPECT_TRUE(device()->CopyResourceV(dst.get(), src.get()));
}

TEST_P(GraphicsDeviceTest, CopyResourceNativeV) {
    const auto width = 256;
    const auto height = 256;
    const std::unique_ptr<ITexture2D> src(device()->CreateDefaultTextureV(width, height, m_textureFormat));
    const std::unique_ptr<ITexture2D> dst(device()->CreateDefaultTextureV(width, height, m_textureFormat));
    EXPECT_TRUE(device()->CopyResourceFromNativeV(dst.get(), src->GetNativeTexturePtrV()));
}

TEST_P(GraphicsDeviceTest, ConvertRGBToI420)
{
    const uint32_t width = 256;
    const uint32_t height = 256;
    const std::unique_ptr<ITexture2D> src(device()->CreateDefaultTextureV(width, height, m_textureFormat));
    const std::unique_ptr<ITexture2D> dst(device()->CreateCPUReadTextureV(width, height, m_textureFormat));
    EXPECT_TRUE(device()->CopyResourceFromNativeV(dst.get(), src->GetNativeTexturePtrV()));
    const auto frameBuffer = device()->ConvertRGBToI420(dst.get());
    EXPECT_NE(nullptr, frameBuffer);
    EXPECT_EQ(width, frameBuffer->width());
    EXPECT_EQ(height, frameBuffer->height());
}

TEST_P(GraphicsDeviceTest, Map)
{
    const uint32_t width = 256;
    const uint32_t height = 256;
    const std::unique_ptr<ITexture2D> src(device()->CreateDefaultTextureV(width, height, m_textureFormat));
    std::unique_ptr<GpuMemoryBufferHandle> handle = device()->Map(src.get());
    EXPECT_NE(handle, nullptr);
}

TEST_P(GraphicsDeviceTest, MapWithCPUReadTexture)
{
    // On Vulkan device, the Map method don't success when using the texture
    // which creating for reading from CPU.
    // It is unclear whether this is the bug or the specification of CUDA.
    if (device()->GetGfxRenderer() == kUnityGfxRendererVulkan)
        GTEST_SKIP_SUCCESS() << "The Map method throw exception on vulkan platform";

    const uint32_t width = 256;
    const uint32_t height = 256;
    const std::unique_ptr<ITexture2D> src2(device()->CreateCPUReadTextureV(width, height, m_textureFormat));
    std::unique_ptr<GpuMemoryBufferHandle> handle2 = device()->Map(src2.get());
}
#endif

INSTANTIATE_TEST_SUITE_P(GfxDeviceAndColorSpece, GraphicsDeviceTest, testing::ValuesIn(VALUES_TEST_ENV));

} // end namespace webrtc
} // end namespace unity
