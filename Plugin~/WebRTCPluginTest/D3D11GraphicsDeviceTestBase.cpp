#include "pch.h"
#include "D3D11GraphicsDeviceTestBase.h"
#include "../WebRTCPlugin/GraphicsDevice/GraphicsDevice.h"

using namespace WebRTC;

#if defined(_WIN32)
#include <d3d11.h>
#include <wrl/client.h>

Microsoft::WRL::ComPtr<IDXGIFactory1> pFactory;
Microsoft::WRL::ComPtr<IDXGIAdapter> pAdapter;
Microsoft::WRL::ComPtr<ID3D11Device> pD3DDevice;
Microsoft::WRL::ComPtr<ID3D11DeviceContext> pD3DDeviceContext;

void D3D11GraphicsDeviceTestBase::SetUp()
{
    auto hr = CreateDXGIFactory1(__uuidof(IDXGIFactory1), reinterpret_cast<void **>(pFactory.GetAddressOf()));
    EXPECT_TRUE(SUCCEEDED(hr));
    EXPECT_NE(nullptr, pFactory.Get());

    hr = pFactory->EnumAdapters(0, pAdapter.GetAddressOf());
    EXPECT_TRUE(SUCCEEDED(hr));
    EXPECT_NE(nullptr, pAdapter.Get());

    hr = D3D11CreateDevice(
        pAdapter.Get(), D3D_DRIVER_TYPE_UNKNOWN, nullptr, 0,
        nullptr, 0, D3D11_SDK_VERSION, pD3DDevice.GetAddressOf(), nullptr, pD3DDeviceContext.GetAddressOf());

    EXPECT_TRUE(SUCCEEDED(hr));
    EXPECT_NE(nullptr, pD3DDevice.Get());
}
void D3D11GraphicsDeviceTestBase::TearDown()
{
}
#else

#include <GL/glut.h>

void D3D11GraphicsDeviceTestBase::SetUp()
{
    UnityGfxRenderer unityGfxRenderer;
    void* pGraphicsDevice;
    std::tie(unityGfxRenderer, pGraphicsDevice) = GetParam();

    int argc = 0;
    glutInit(&argc, nullptr);
    glutCreateWindow("test");

    ASSERT_TRUE(GraphicsDevice::GetInstance().Init(unityGfxRenderer, pGraphicsDevice));
    m_device = GraphicsDevice::GetInstance().GetDevice();
    ASSERT_NE(nullptr, m_device);
}

void D3D11GraphicsDeviceTestBase::TearDown()
{
    GraphicsDevice::GetInstance().Shutdown();
}

#endif