#include "pch.h"
#include <d3d11.h>
#include <wrl/client.h>
#include "D3D11GraphicsDeviceTestBase.h"
#include "../unity/include/IUnityGraphics.h"
#include "../WebRTCPlugin/GraphicsDevice/GraphicsDevice.h"
#include "../WebRTCPlugin/GraphicsDevice/ITexture2D.h"

void D3D11GraphicsDeviceTestBase::SetUp()
{
    auto hr = CreateDXGIFactory1(__uuidof(IDXGIFactory1), (void **)(pFactory.GetAddressOf()));
    EXPECT_TRUE(SUCCEEDED(hr));
    EXPECT_NE(nullptr, pFactory.Get());

    hr = pFactory->EnumAdapters(0, pAdapter.GetAddressOf());
    EXPECT_TRUE(SUCCEEDED(hr));
    EXPECT_NE(nullptr, pAdapter.Get());

    hr = D3D11CreateDevice(
        pAdapter.Get(), D3D_DRIVER_TYPE_UNKNOWN, NULL, 0,
        NULL, 0, D3D11_SDK_VERSION, pD3DDevice.GetAddressOf(), NULL, pD3DDeviceContext.GetAddressOf());

    EXPECT_TRUE(SUCCEEDED(hr));
    EXPECT_NE(nullptr, pD3DDevice.Get());
}
void D3D11GraphicsDeviceTestBase::TearDown()
{
}
