#include "pch.h"
#include "D3D11GraphicsDeviceTestBase.h"
#include "../WebRTCPlugin/GraphicsDevice/ITexture2D.h"

using namespace WebRTC;

class D3D11GraphicsDeviceTest : public D3D11GraphicsDeviceTestBase {};



TEST_P(D3D11GraphicsDeviceTest, GraphicsDeviceIsNotNull) {
    EXPECT_NE(nullptr, m_device);
}

TEST_P(D3D11GraphicsDeviceTest, CreateDefaultTextureV) {
    auto width = 256;
    auto height = 256;
    auto tex = m_device->CreateDefaultTextureV(width, height);
    EXPECT_TRUE(tex->IsSize(width, height));
    EXPECT_FALSE(tex->IsSize(0, 0));
}

TEST_P(D3D11GraphicsDeviceTest, CopyResourceV) {
    const auto width = 256;
    const auto height = 256;
    const auto src = m_device->CreateDefaultTextureV(width, height);
    const auto dst = m_device->CreateDefaultTextureV(width, height);
    EXPECT_TRUE(m_device->CopyResourceV(dst, src));
    EXPECT_FALSE(m_device->CopyResourceV(src, src));
}

TEST_P(D3D11GraphicsDeviceTest, CopyResourceNativeV) {
    const auto width = 256;
    const auto height = 256;
    const auto src = m_device->CreateDefaultTextureV(width, height);
    const auto dst = m_device->CreateDefaultTextureV(width, height);
    EXPECT_TRUE(m_device->CopyResourceFromNativeV(dst, src->GetNativeTexturePtrV()));
    EXPECT_FALSE(m_device->CopyResourceFromNativeV(dst, dst->GetNativeTexturePtrV()));
}

INSTANTIATE_TEST_CASE_P(
        GraphicsDeviceParameters,
        D3D11GraphicsDeviceTest,
        testing::Values(std::make_tuple(kUnityGfxRendererOpenGLCore, nullptr))
);