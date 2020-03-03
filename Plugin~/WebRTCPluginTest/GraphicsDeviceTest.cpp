#include "pch.h"
#include "GraphicsDeviceTestBase.h"
#include "../WebRTCPlugin/GraphicsDevice/ITexture2D.h"

using namespace WebRTC;
using namespace testing;

class GraphicsDeviceTest : public GraphicsDeviceTestBase {};
TEST_P(GraphicsDeviceTest, GraphicsDeviceIsNotNull) {
    EXPECT_NE(nullptr, m_device);
}

TEST_P(GraphicsDeviceTest, CreateDefaultTextureV) {
    auto width = 256;
    auto height = 256;
    auto tex = m_device->CreateDefaultTextureV(width, height);
    EXPECT_TRUE(tex->IsSize(width, height));
    EXPECT_NE(nullptr, tex->GetEncodeTexturePtrV());
    EXPECT_NE(nullptr, tex->GetNativeTexturePtrV());
    EXPECT_FALSE(tex->IsSize(0, 0));
}

#if defined(SUPPORT_SOFTWARE_ENCODER)
TEST_P(GraphicsDeviceTest, CreateCPUReadTextureV) {
    auto width = 256;
    auto height = 256;
    auto tex = m_device->CreateCPUReadTextureV(width, height);
    EXPECT_TRUE(tex->IsSize(width, height));
    EXPECT_NE(nullptr, tex->GetEncodeTexturePtrV());
    EXPECT_NE(nullptr, tex->GetNativeTexturePtrV());
    EXPECT_FALSE(tex->IsSize(0, 0));
}
#endif

//[Note-sin: 2019-12-19] Real Unity Interface is required for testing the following functions, and it is not 
//possible to create a dummy Unity interface (with its command buffer) on Metal devices
#if !defined(SUPPORT_METAL)

TEST_P(GraphicsDeviceTest, CopyResourceV) {
    const auto width = 256;
    const auto height = 256;
    const auto src = m_device->CreateDefaultTextureV(width, height);
    const auto dst = m_device->CreateDefaultTextureV(width, height);
}

TEST_P(GraphicsDeviceTest, CopyResourceNativeV) {
    const auto width = 256;
    const auto height = 256;
    const auto src = m_device->CreateDefaultTextureV(width, height);
    const auto dst = m_device->CreateDefaultTextureV(width, height);
    EXPECT_TRUE(m_device->CopyResourceFromNativeV(dst, src->GetEncodeTexturePtrV()));
    EXPECT_FALSE(m_device->CopyResourceFromNativeV(dst, dst->GetEncodeTexturePtrV()));
}
#endif

INSTANTIATE_TEST_CASE_P(GraphicsDeviceParameters, GraphicsDeviceTest, ValuesIn(VALUES_TEST_ENV));

