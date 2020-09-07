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
#if defined(UNITY_WIN)
    { kUnityGfxRendererD3D11, UnityEncoderType::UnityEncoderHardware },
    { kUnityGfxRendererD3D11, UnityEncoderType::UnityEncoderSoftware },
    { kUnityGfxRendererD3D12, UnityEncoderType::UnityEncoderHardware },
    { kUnityGfxRendererD3D12, UnityEncoderType::UnityEncoderSoftware },
    { kUnityGfxRendererVulkan, UnityEncoderType::UnityEncoderHardware },
    { kUnityGfxRendererVulkan, UnityEncoderType::UnityEncoderSoftware }
#elif defined(UNITY_OSX)
    { kUnityGfxRendererMetal, UnityEncoderType::UnityEncoderSoftware }
#elif defined(UNITY_LINUX)
    { kUnityGfxRendererOpenGLCore, UnityEncoderType::UnityEncoderHardware }
#endif
};

} // end namespace webrtc
} // end namespace unity
