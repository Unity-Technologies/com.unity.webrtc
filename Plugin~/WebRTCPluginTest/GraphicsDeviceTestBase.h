#pragma once
#include "gtest/gtest.h"
#include "gmock/gmock.h"

namespace unity
{
namespace webrtc
{

using std::tuple;
using testing::Values;

class IGraphicsDevice;
class GraphicsDeviceTestBase
    : public testing::TestWithParam<tuple<UnityGfxRenderer, UnityEncoderType, UnityRenderingExtTextureFormat> >
{
public:
    GraphicsDeviceTestBase();
    virtual ~GraphicsDeviceTestBase();
    void SetUp() override;
    void TearDown() override;
protected:
    void* m_pNativeGfxDevice;
    IGraphicsDevice* m_device;
    UnityEncoderType m_encoderType;
    UnityGfxRenderer m_unityGfxRenderer;
    UnityRenderingExtTextureFormat m_textureFormat;
};

static tuple<UnityGfxRenderer, UnityEncoderType, UnityRenderingExtTextureFormat> VALUES_TEST_ENV[] = {
#if defined(SUPPORT_D3D11)
    { kUnityGfxRendererD3D11, UnityEncoderType::UnityEncoderHardware, UnityRenderingExtTextureFormat::kUnityRenderingExtFormatB8G8R8A8_SRGB},
    { kUnityGfxRendererD3D11, UnityEncoderType::UnityEncoderHardware, UnityRenderingExtTextureFormat::kUnityRenderingExtFormatB8G8R8A8_UNorm },
    { kUnityGfxRendererD3D11, UnityEncoderType::UnityEncoderSoftware, UnityRenderingExtTextureFormat::kUnityRenderingExtFormatB8G8R8A8_SRGB },
    { kUnityGfxRendererD3D11, UnityEncoderType::UnityEncoderSoftware, UnityRenderingExtTextureFormat::kUnityRenderingExtFormatB8G8R8A8_UNorm },
#endif // defined(SUPPORT_D3D11)
#if defined(SUPPORT_D3D12)
    { kUnityGfxRendererD3D12, UnityEncoderType::UnityEncoderHardware, UnityRenderingExtTextureFormat::kUnityRenderingExtFormatB8G8R8A8_SRGB },
    { kUnityGfxRendererD3D12, UnityEncoderType::UnityEncoderHardware, UnityRenderingExtTextureFormat::kUnityRenderingExtFormatB8G8R8A8_UNorm },
    { kUnityGfxRendererD3D12, UnityEncoderType::UnityEncoderSoftware, UnityRenderingExtTextureFormat::kUnityRenderingExtFormatB8G8R8A8_SRGB },
    { kUnityGfxRendererD3D12, UnityEncoderType::UnityEncoderSoftware, UnityRenderingExtTextureFormat::kUnityRenderingExtFormatB8G8R8A8_UNorm },
#endif // defined(SUPPORT_D3D12)
#if defined(SUPPORT_METAL)
    { kUnityGfxRendererMetal, UnityEncoderType::UnityEncoderSoftware, UnityRenderingExtTextureFormat::kUnityRenderingExtFormatB8G8R8A8_SRGB },
    { kUnityGfxRendererMetal, UnityEncoderType::UnityEncoderSoftware, UnityRenderingExtTextureFormat::kUnityRenderingExtFormatB8G8R8A8_UNorm }
#endif // defined(SUPPORT_METAL)
// todo::(kazuki) windows support
// todo::(kazuki) software encoder support
#if defined(SUPPORT_OPENGL_UNIFIED) & defined(UNITY_LINUX)
    { kUnityGfxRendererOpenGLCore, UnityEncoderType::UnityEncoderHardware, UnityRenderingExtTextureFormat::kUnityRenderingExtFormatB8G8R8A8_SRGB },
    { kUnityGfxRendererOpenGLCore, UnityEncoderType::UnityEncoderHardware, UnityRenderingExtTextureFormat::kUnityRenderingExtFormatB8G8R8A8_UNorm },
#endif // defined(SUPPORT_OPENGL_UNIFIED)
#if defined(SUPPORT_VULKAN)
    { kUnityGfxRendererVulkan, UnityEncoderType::UnityEncoderHardware, UnityRenderingExtTextureFormat::kUnityRenderingExtFormatB8G8R8A8_SRGB },
    { kUnityGfxRendererVulkan, UnityEncoderType::UnityEncoderHardware, UnityRenderingExtTextureFormat::kUnityRenderingExtFormatB8G8R8A8_UNorm },
    { kUnityGfxRendererVulkan, UnityEncoderType::UnityEncoderSoftware, UnityRenderingExtTextureFormat::kUnityRenderingExtFormatB8G8R8A8_SRGB },
    { kUnityGfxRendererVulkan, UnityEncoderType::UnityEncoderSoftware, UnityRenderingExtTextureFormat::kUnityRenderingExtFormatB8G8R8A8_UNorm }
#endif // defined(SUPPORT_VULKAN)
};

} // end namespace webrtc
} // end namespace unity
