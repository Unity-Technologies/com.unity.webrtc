#pragma once
#include "gtest/gtest.h"
#include "../WebRTCPlugin/GraphicsDevice/IGraphicsDevice.h"

using WebRTC::UnityEncoderType;
using std::tuple;
using testing::Values;

class GraphicsDeviceTestBase : public testing::TestWithParam<tuple<UnityGfxRenderer, UnityEncoderType> > {

protected:
    virtual void SetUp() override;
    virtual void TearDown() override;
    WebRTC::IGraphicsDevice* m_device;
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
