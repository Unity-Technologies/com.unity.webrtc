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
        : public testing::TestWithParam<tuple<UnityGfxRenderer, UnityRenderingExtTextureFormat>>
    {
    public:
        explicit GraphicsDeviceTestBase();
        virtual ~GraphicsDeviceTestBase();
        IGraphicsDevice* device();
        UnityRenderingExtTextureFormat format();

    private:
        UnityGfxRenderer m_unityGfxRenderer;
        UnityRenderingExtTextureFormat m_textureFormat;
        std::unique_ptr<GraphicsDeviceContainer> container_;
        IGraphicsDevice* device_;
    };

    static tuple<UnityGfxRenderer, UnityRenderingExtTextureFormat> VALUES_TEST_ENV[] = {
#if SUPPORT_D3D11
        { kUnityGfxRendererD3D11, kUnityRenderingExtFormatB8G8R8A8_SRGB },
        { kUnityGfxRendererD3D11, kUnityRenderingExtFormatB8G8R8A8_UNorm },
#endif // SUPPORT_D3D11
#if SUPPORT_D3D12
        { kUnityGfxRendererD3D12, kUnityRenderingExtFormatB8G8R8A8_SRGB },
        { kUnityGfxRendererD3D12, kUnityRenderingExtFormatB8G8R8A8_UNorm },
#endif // SUPPORT_D3D12
#if SUPPORT_METAL
        { kUnityGfxRendererMetal, kUnityRenderingExtFormatB8G8R8A8_SRGB },
        { kUnityGfxRendererMetal, kUnityRenderingExtFormatB8G8R8A8_UNorm }
#endif // SUPPORT_METAL
// todo::(kazuki) windows support
#if SUPPORT_OPENGL_UNIFIED & UNITY_LINUX
        { kUnityGfxRendererOpenGLCore, kUnityRenderingExtFormatB8G8R8A8_SRGB },
        { kUnityGfxRendererOpenGLCore, kUnityRenderingExtFormatB8G8R8A8_UNorm },
#endif // SUPPORT_OPENGL_UNIFIED
#if SUPPORT_OPENGL_ES
        { kUnityGfxRendererOpenGLES30, kUnityRenderingExtFormatB8G8R8A8_SRGB },
        { kUnityGfxRendererOpenGLES30, kUnityRenderingExtFormatB8G8R8A8_UNorm },
#endif // SUPPORT_OPENGL_ES
#if SUPPORT_VULKAN
        { kUnityGfxRendererVulkan, kUnityRenderingExtFormatB8G8R8A8_SRGB },
        { kUnityGfxRendererVulkan, kUnityRenderingExtFormatB8G8R8A8_UNorm },
#endif // SUPPORT_VULKAN
    };

} // end namespace webrtc
} // end namespace unity
