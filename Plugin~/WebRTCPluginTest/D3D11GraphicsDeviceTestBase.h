#pragma once
#include "gtest/gtest.h"
#include "../WebRTCPlugin/GraphicsDevice/IGraphicsDevice.h"

class D3D11GraphicsDeviceTestBase : public testing::TestWithParam< ::std::tuple<UnityGfxRenderer, void*> > {
protected:
    virtual void SetUp() override;
    virtual void TearDown() override;
    WebRTC::IGraphicsDevice* m_device;
};
