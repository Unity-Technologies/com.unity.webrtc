#include "pch.h"
#include "D3D11GraphicsDeviceTestBase.h"
#include "../unity/include/IUnityGraphics.h"
#include "../WebRTCPlugin/GraphicsDevice/GraphicsDevice.h"
#include "../WebRTCPlugin/GraphicsDevice/ITexture2D.h"

using namespace WebRTC;

class D3D11GraphicsDeviceTest : public D3D11GraphicsDeviceTestBase {
protected:
    UnityGfxRenderer unityGfxRenderer;
    void *pGraphicsDevice;
    void SetUp() override
    {
        std::tie(unityGfxRenderer, pGraphicsDevice) = GetParam();
    }
};



TEST_P(D3D11GraphicsDeviceTest, InitAndShutdown) {
    GraphicsDevice::GetInstance().Init(unityGfxRenderer, pGraphicsDevice);
    GraphicsDevice::GetInstance().Shutdown();
}

TEST_P(D3D11GraphicsDeviceTest, CreateDefaultTextureV) {
    GraphicsDevice::GetInstance().Init(unityGfxRenderer, pGraphicsDevice);
    auto device = GraphicsDevice::GetInstance().GetDevice();
    EXPECT_NE(nullptr, device);
    auto width = 256;
    auto height = 256;
    auto tex = device->CreateDefaultTextureV(width, height);
    EXPECT_TRUE(tex->IsSize(width, height));
    EXPECT_FALSE(tex->IsSize(0, 0));
    GraphicsDevice::GetInstance().Shutdown();
}

TEST_P(D3D11GraphicsDeviceTest, CopyResourceV) {
    GraphicsDevice::GetInstance().Init(unityGfxRenderer, pGraphicsDevice);
    auto device = GraphicsDevice::GetInstance().GetDevice();
    EXPECT_NE(nullptr, device);
    const auto width = 256;
    const auto height = 256;
    const auto src = device->CreateDefaultTextureV(width, height);
    const auto dst = device->CreateDefaultTextureV(width, height);
    EXPECT_TRUE(device->CopyResourceV(dst, src));
    EXPECT_FALSE(device->CopyResourceV(src, src));
    GraphicsDevice::GetInstance().Shutdown();
}

TEST_P(D3D11GraphicsDeviceTest, CopyResourceNativeV) {
    GraphicsDevice::GetInstance().Init(unityGfxRenderer, pGraphicsDevice);
    auto device = GraphicsDevice::GetInstance().GetDevice();
    EXPECT_NE(nullptr, device);
    const auto width = 256;
    const auto height = 256;
    const auto src = device->CreateDefaultTextureV(width, height);
    const auto dst = device->CreateDefaultTextureV(width, height);
    EXPECT_TRUE(device->CopyResourceFromNativeV(dst, src->GetNativeTexturePtrV()));
    EXPECT_FALSE(device->CopyResourceFromNativeV(dst, dst->GetNativeTexturePtrV()));
    GraphicsDevice::GetInstance().Shutdown();
}

INSTANTIATE_TEST_CASE_P(
        GraphicsDeviceParameters,
        D3D11GraphicsDeviceTest,
        testing::Values(std::make_tuple(kUnityGfxRendererOpenGLCore, nullptr))
);