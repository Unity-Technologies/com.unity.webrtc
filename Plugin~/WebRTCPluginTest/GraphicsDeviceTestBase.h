#pragma once
#include "gmock/gmock.h"
#include "gtest/gtest.h"

#include "IUnityGraphics.h"
#include "IUnityRenderingExtensions.h"

namespace unity
{
namespace webrtc
{

    using std::tuple;
    using testing::Values;

    class IGraphicsDevice;
    class GraphicsDeviceContainer;

    class GraphicsDeviceTestBase
        : public testing::TestWithParam<tuple<UnityGfxRenderer, UnityEncoderType, UnityRenderingExtTextureFormat>>
    {
    public:
        explicit GraphicsDeviceTestBase();
        virtual ~GraphicsDeviceTestBase();
        void SetUp() override;
        void TearDown() override;
        IGraphicsDevice* device();

    protected:
        UnityGfxRenderer m_unityGfxRenderer;
        UnityRenderingExtTextureFormat m_textureFormat;
        UnityEncoderType m_encoderType;
        std::unique_ptr<GraphicsDeviceContainer> container_;
    };

static tuple<UnityGfxRenderer, UnityEncoderType, UnityRenderingExtTextureFormat> VALUES_TEST_ENV[] = {
#if SUPPORT_D3D11
    { kUnityGfxRendererD3D11, UnityEncoderType::UnityEncoderHardware, UnityRenderingExtTextureFormat::kUnityRenderingExtFormatB8G8R8A8_SRGB},
    { kUnityGfxRendererD3D11, UnityEncoderType::UnityEncoderHardware, UnityRenderingExtTextureFormat::kUnityRenderingExtFormatB8G8R8A8_UNorm },
    { kUnityGfxRendererD3D11, UnityEncoderType::UnityEncoderSoftware, UnityRenderingExtTextureFormat::kUnityRenderingExtFormatB8G8R8A8_SRGB },
    { kUnityGfxRendererD3D11, UnityEncoderType::UnityEncoderSoftware, UnityRenderingExtTextureFormat::kUnityRenderingExtFormatB8G8R8A8_UNorm },
#endif // SUPPORT_D3D11
#if SUPPORT_D3D12
    { kUnityGfxRendererD3D12, UnityEncoderType::UnityEncoderHardware, UnityRenderingExtTextureFormat::kUnityRenderingExtFormatB8G8R8A8_SRGB },
    { kUnityGfxRendererD3D12, UnityEncoderType::UnityEncoderHardware, UnityRenderingExtTextureFormat::kUnityRenderingExtFormatB8G8R8A8_UNorm },
    { kUnityGfxRendererD3D12, UnityEncoderType::UnityEncoderSoftware, UnityRenderingExtTextureFormat::kUnityRenderingExtFormatB8G8R8A8_SRGB },
    { kUnityGfxRendererD3D12, UnityEncoderType::UnityEncoderSoftware, UnityRenderingExtTextureFormat::kUnityRenderingExtFormatB8G8R8A8_UNorm },
#endif // SUPPORT_D3D12
#if SUPPORT_METAL
    { kUnityGfxRendererMetal, UnityEncoderType::UnityEncoderSoftware, UnityRenderingExtTextureFormat::kUnityRenderingExtFormatB8G8R8A8_SRGB },
    { kUnityGfxRendererMetal, UnityEncoderType::UnityEncoderSoftware, UnityRenderingExtTextureFormat::kUnityRenderingExtFormatB8G8R8A8_UNorm }
#endif // SUPPORT_METAL
// todo::(kazuki) windows support
#if SUPPORT_OPENGL_UNIFIED & UNITY_LINUX
    { kUnityGfxRendererOpenGLCore, UnityEncoderType::UnityEncoderHardware, UnityRenderingExtTextureFormat::kUnityRenderingExtFormatB8G8R8A8_SRGB },
    { kUnityGfxRendererOpenGLCore, UnityEncoderType::UnityEncoderHardware, UnityRenderingExtTextureFormat::kUnityRenderingExtFormatB8G8R8A8_UNorm },
    { kUnityGfxRendererOpenGLCore, UnityEncoderType::UnityEncoderSoftware, UnityRenderingExtTextureFormat::kUnityRenderingExtFormatB8G8R8A8_SRGB },
    { kUnityGfxRendererOpenGLCore, UnityEncoderType::UnityEncoderSoftware, UnityRenderingExtTextureFormat::kUnityRenderingExtFormatB8G8R8A8_UNorm },
#endif // SUPPORT_OPENGL_UNIFIED
#if SUPPORT_OPENGL_ES
    { kUnityGfxRendererOpenGLES30, UnityEncoderType::UnityEncoderHardware, UnityRenderingExtTextureFormat::kUnityRenderingExtFormatB8G8R8A8_SRGB },
    { kUnityGfxRendererOpenGLES30, UnityEncoderType::UnityEncoderHardware, UnityRenderingExtTextureFormat::kUnityRenderingExtFormatB8G8R8A8_UNorm },
    { kUnityGfxRendererOpenGLES30, UnityEncoderType::UnityEncoderSoftware, UnityRenderingExtTextureFormat::kUnityRenderingExtFormatB8G8R8A8_SRGB },
    { kUnityGfxRendererOpenGLES30, UnityEncoderType::UnityEncoderSoftware, UnityRenderingExtTextureFormat::kUnityRenderingExtFormatB8G8R8A8_UNorm },
#endif // SUPPORT_OPENGL_ES
#if SUPPORT_VULKAN
    { kUnityGfxRendererVulkan, UnityEncoderType::UnityEncoderHardware, UnityRenderingExtTextureFormat::kUnityRenderingExtFormatB8G8R8A8_SRGB },
    { kUnityGfxRendererVulkan, UnityEncoderType::UnityEncoderHardware, UnityRenderingExtTextureFormat::kUnityRenderingExtFormatB8G8R8A8_UNorm },
    { kUnityGfxRendererVulkan, UnityEncoderType::UnityEncoderSoftware, UnityRenderingExtTextureFormat::kUnityRenderingExtFormatB8G8R8A8_SRGB },
    { kUnityGfxRendererVulkan, UnityEncoderType::UnityEncoderSoftware, UnityRenderingExtTextureFormat::kUnityRenderingExtFormatB8G8R8A8_UNorm }
#endif // SUPPORT_VULKAN
};

} // end namespace webrtc
} // end namespace unity
