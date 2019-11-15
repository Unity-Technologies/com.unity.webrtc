#pragma once
#include "gtest/gtest.h"
#include <wrl/client.h>

class D3D11GraphicsDeviceTestBase : public ::testing::Test {
protected:
    virtual void SetUp() override;
    virtual void TearDown() override;
    Microsoft::WRL::ComPtr<IDXGIFactory1> pFactory;
    Microsoft::WRL::ComPtr<IDXGIAdapter> pAdapter;
    Microsoft::WRL::ComPtr<ID3D11Device> pD3DDevice;
    Microsoft::WRL::ComPtr<ID3D11DeviceContext> pD3DDeviceContext;
};
