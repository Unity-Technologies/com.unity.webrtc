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
    : public testing::TestWithParam<tuple<UnityGfxRenderer, UnityEncoderType> >
{
public:
    GraphicsDeviceTestBase();
    virtual ~GraphicsDeviceTestBase();
protected:
    IGraphicsDevice* m_device;
    UnityEncoderType m_encoderType;
    UnityGfxRenderer m_unityGfxRenderer;
};

static tuple<UnityGfxRenderer, UnityEncoderType> VALUES_TEST_ENV[] = {
#if defined(SUPPORT_D3D11)
    { kUnityGfxRendererD3D11, UnityEncoderType::UnityEncoderHardware },
    { kUnityGfxRendererD3D11, UnityEncoderType::UnityEncoderSoftware },
#endif // defined(SUPPORT_D3D11)
#if defined(SUPPORT_D3D12)
    { kUnityGfxRendererD3D12, UnityEncoderType::UnityEncoderHardware },
    { kUnityGfxRendererD3D12, UnityEncoderType::UnityEncoderSoftware },
#endif // defined(SUPPORT_D3D12)
#if defined(SUPPORT_METAL)
    { kUnityGfxRendererMetal, UnityEncoderType::UnityEncoderSoftware }
#endif // defined(SUPPORT_METAL)
// todo::(kazuki) windows support
// todo::(kazuki) software encoder support
#if defined(SUPPORT_OPENGL_UNIFIED) & defined(UNITY_LINUX)
    { kUnityGfxRendererOpenGLCore, UnityEncoderType::UnityEncoderHardware },
#endif // defined(SUPPORT_OPENGL_UNIFIED)
#if defined(SUPPORT_VULKAN)
    { kUnityGfxRendererVulkan, UnityEncoderType::UnityEncoderHardware },
    { kUnityGfxRendererVulkan, UnityEncoderType::UnityEncoderSoftware }
#endif // defined(SUPPORT_VULKAN)
};

} // end namespace webrtc
} // end namespace unity
