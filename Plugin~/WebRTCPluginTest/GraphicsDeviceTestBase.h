#pragma once
#include "gtest/gtest.h"
#include "../WebRTCPlugin/GraphicsDevice/IGraphicsDevice.h"
#include "unity/include/IUnityInterface.h"

class GraphicsDeviceTestBase : public testing::TestWithParam< ::std::tuple<UnityGfxRenderer, void*, IUnityInterface*> > {

public:
    static std::tuple<UnityGfxRenderer, void*, IUnityInterface*> CreateParameter();

protected:
    virtual void SetUp() override;
    virtual void TearDown() override;
    WebRTC::IGraphicsDevice* m_device;
};

