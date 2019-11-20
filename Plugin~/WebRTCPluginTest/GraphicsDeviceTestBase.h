#pragma once
#include "gtest/gtest.h"
#include "../WebRTCPlugin/GraphicsDevice/IGraphicsDevice.h"

class GraphicsDeviceTestBase : public testing::TestWithParam< ::std::tuple<UnityGfxRenderer, void*> > {

public:
    static std::tuple<UnityGfxRenderer, void*> CreateParameter();

protected:
    virtual void SetUp() override;
    virtual void TearDown() override;
    WebRTC::IGraphicsDevice* m_device;
};

