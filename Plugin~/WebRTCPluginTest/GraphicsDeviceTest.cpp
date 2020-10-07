#include "pch.h"
#include "GraphicsDeviceTestBase.h"
#include "GraphicsDevice/IGraphicsDevice.h"
#include "GraphicsDevice/ITexture2D.h"

namespace unity
{
namespace webrtc
{

class GraphicsDeviceTest : public GraphicsDeviceTestBase {};
TEST_P(GraphicsDeviceTest, GraphicsDeviceIsNotNull) {
    EXPECT_NE(nullptr, m_device);
}

TEST_P(GraphicsDeviceTest, CreateDefaultTextureV) {
    const auto width = 256;
    const auto height = 256;
    const std::unique_ptr<ITexture2D> tex(m_device->CreateDefaultTextureV(width, height));
    EXPECT_TRUE(tex->IsSize(width, height));
    EXPECT_NE(nullptr, tex->GetEncodeTexturePtrV());
    EXPECT_NE(nullptr, tex->GetNativeTexturePtrV());
    EXPECT_FALSE(tex->IsSize(0, 0));
}

#if defined(SUPPORT_SOFTWARE_ENCODER)
TEST_P(GraphicsDeviceTest, CreateCPUReadTextureV) {
    const auto width = 256;
    const auto height = 256;
    const std::unique_ptr<ITexture2D> tex(m_device->CreateCPUReadTextureV(width, height));
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
    const std::unique_ptr<ITexture2D> src(m_device->CreateDefaultTextureV(width, height));
    const std::unique_ptr<ITexture2D> dst(m_device->CreateDefaultTextureV(width, height));
    EXPECT_TRUE(m_device->CopyResourceV(dst.get(), src.get()));
}

TEST_P(GraphicsDeviceTest, CopyResourceNativeV) {
    const auto width = 256;
    const auto height = 256;
    const std::unique_ptr<ITexture2D> src(m_device->CreateDefaultTextureV(width, height));
    const std::unique_ptr<ITexture2D> dst(m_device->CreateDefaultTextureV(width, height));
    EXPECT_TRUE(m_device->CopyResourceFromNativeV(dst.get(), src->GetNativeTexturePtrV()));
}

TEST_P(GraphicsDeviceTest, ConvertRGBToI420) {
    const auto width = 256;
    const auto height = 256;
    const std::unique_ptr<ITexture2D> src(m_device->CreateDefaultTextureV(width, height));
    const std::unique_ptr <ITexture2D> dst(m_device->CreateCPUReadTextureV(width, height));
    EXPECT_TRUE(m_device->CopyResourceFromNativeV(dst.get(), src->GetNativeTexturePtrV()));
    const auto frameBuffer = m_device->ConvertRGBToI420(dst.get());
    EXPECT_NE(nullptr, frameBuffer);
    EXPECT_EQ(width, frameBuffer->width());
    EXPECT_EQ(height, frameBuffer->height());
}
#endif

INSTANTIATE_TEST_CASE_P(GraphicsDeviceParameters, GraphicsDeviceTest, testing::ValuesIn(VALUES_TEST_ENV));

} // end namespace webrtc
} // end namespace unity
