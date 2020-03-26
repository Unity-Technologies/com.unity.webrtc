#pragma once
#include "gtest/gtest.h"
#include "../WebRTCPlugin/GraphicsDevice/IGraphicsDevice.h"

namespace unity
{
namespace webrtc
{

using unity::webrtc::UnityEncoderType;
using std::tuple;
using testing::Values;

class GraphicsDeviceTestBase : public testing::TestWithParam<tuple<UnityGfxRenderer, UnityEncoderType> > {

protected:
    void SetUp() override;
    void TearDown() override;
    IGraphicsDevice* m_device;
    UnityEncoderType encoderType;
};

static tuple<UnityGfxRenderer, UnityEncoderType> VALUES_TEST_ENV[] = {
#if defined(UNITY_WIN)
    { kUnityGfxRendererD3D11, UnityEncoderType::UnityEncoderHardware },
    { kUnityGfxRendererD3D11, UnityEncoderType::UnityEncoderSoftware }
//    { kUnityGfxRendererD3D12, UnityEncoderType::UnityEncoderHardware }
//    { kUnityGfxRendererD3D12, UnityEncoderType::UnityEncoderSoftware }
#elif defined(UNITY_OSX)
    { kUnityGfxRendererMetal, UnityEncoderType::UnityEncoderSoftware }
#elif defined(UNITY_LINUX)
    { kUnityGfxRendererOpenGLCore, UnityEncoderType::UnityEncoderHardware }
#endif
};

} // end namespace webrtc
} // end namespace unity
