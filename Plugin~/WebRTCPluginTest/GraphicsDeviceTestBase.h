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
    : public testing::TestWithParam<tuple<UnityGfxRenderer, UnityEncoderType, UnityColorSpace> >
{
public:
    GraphicsDeviceTestBase();
    virtual ~GraphicsDeviceTestBase();
protected:
    IGraphicsDevice* m_device;
    UnityEncoderType m_encoderType;
    UnityGfxRenderer m_unityGfxRenderer;
    UnityColorSpace m_colorSpace;
};

static tuple<UnityGfxRenderer, UnityEncoderType, UnityColorSpace> VALUES_TEST_ENV[] = {
#if defined(SUPPORT_D3D11)
    { kUnityGfxRendererD3D11, UnityEncoderType::UnityEncoderHardware, UnityColorSpace::Gamma },
    { kUnityGfxRendererD3D11, UnityEncoderType::UnityEncoderHardware, UnityColorSpace::Linear },
    { kUnityGfxRendererD3D11, UnityEncoderType::UnityEncoderSoftware, UnityColorSpace::Gamma },
    { kUnityGfxRendererD3D11, UnityEncoderType::UnityEncoderSoftware, UnityColorSpace::Linear },
#endif // defined(SUPPORT_D3D11)
#if defined(SUPPORT_D3D12)
    { kUnityGfxRendererD3D12, UnityEncoderType::UnityEncoderHardware, UnityColorSpace::Gamma },
    { kUnityGfxRendererD3D12, UnityEncoderType::UnityEncoderHardware, UnityColorSpace::Linear },
    { kUnityGfxRendererD3D12, UnityEncoderType::UnityEncoderSoftware, UnityColorSpace::Gamma },
    { kUnityGfxRendererD3D12, UnityEncoderType::UnityEncoderSoftware, UnityColorSpace::Linear },
#endif // defined(SUPPORT_D3D12)
#if defined(SUPPORT_METAL)
    { kUnityGfxRendererMetal, UnityEncoderType::UnityEncoderSoftware, UnityColorSpace::Gamma },
    { kUnityGfxRendererMetal, UnityEncoderType::UnityEncoderSoftware, UnityColorSpace::Linear }
#endif // defined(SUPPORT_METAL)
// todo::(kazuki) windows support
// todo::(kazuki) software encoder support
#if defined(SUPPORT_OPENGL_UNIFIED) & defined(UNITY_LINUX)
    { kUnityGfxRendererOpenGLCore, UnityEncoderType::UnityEncoderHardware, UnityColorSpace::Gamma },
    { kUnityGfxRendererOpenGLCore, UnityEncoderType::UnityEncoderHardware, UnityColorSpace::Linear },
#endif // defined(SUPPORT_OPENGL_UNIFIED)
#if defined(SUPPORT_VULKAN)
    { kUnityGfxRendererVulkan, UnityEncoderType::UnityEncoderHardware, UnityColorSpace::Gamma },
    { kUnityGfxRendererVulkan, UnityEncoderType::UnityEncoderHardware, UnityColorSpace::Linear },
    { kUnityGfxRendererVulkan, UnityEncoderType::UnityEncoderSoftware, UnityColorSpace::Gamma },
    { kUnityGfxRendererVulkan, UnityEncoderType::UnityEncoderSoftware, UnityColorSpace::Linear }
#endif // defined(SUPPORT_VULKAN)
};

} // end namespace webrtc
} // end namespace unity
