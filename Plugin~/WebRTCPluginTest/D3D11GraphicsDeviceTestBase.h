#pragma once
#include "gtest/gtest.h"

class D3D11GraphicsDeviceTestBase : public testing::TestWithParam< ::std::tuple<UnityGfxRenderer, void*> > {
protected:
    virtual void SetUp() override;
    virtual void TearDown() override;
};
