#include "pch.h"
#include "GraphicsDeviceTestBase.h"
#include "../WebRTCPlugin/PlatformBase.h"
#include "../WebRTCPlugin/GraphicsDevice/GraphicsDevice.h"

using namespace WebRTC;

#if defined(SUPPORT_D3D11)
#include <d3d11.h>
#include <wrl/client.h>

Microsoft::WRL::ComPtr<IDXGIFactory1> pFactory;
Microsoft::WRL::ComPtr<IDXGIAdapter> pAdapter;
Microsoft::WRL::ComPtr<ID3D11Device> pD3DDevice;
Microsoft::WRL::ComPtr<ID3D11DeviceContext> pD3DDeviceContext;

void* CreateDevice()
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
    return pD3DDevice.Get();
}
#elif defined(SUPPORT_METAL)

#import <Metal/Metal.h>

void* CreateDevice()
{
    return MTLCreateSystemDefaultDevice();
}

#else
#include <GL/glut.h>

static bool s_glutInitialized;

void* CreateDevice()
{
    if (!s_glutInitialized)
    {
        int argc = 0;
        glutInit(&argc, nullptr);
        s_glutInitialized = true;
        glutCreateWindow("test");
    }
    return nullptr;
}
#endif

std::tuple<UnityGfxRenderer, void*> GraphicsDeviceTestBase::CreateParameter()
{
#if defined(SUPPORT_D3D11)
    auto unityGfxRenderer = kUnityGfxRendererD3D11;
#elif defined(SUPPORT_OPENGL_CORE)
    auto unityGfxRenderer = kUnityGfxRendererOpenGLCore;
#elif defined(SUPPORT_METAL)
    auto unityGfxRenderer = kUnityGfxRendererMetal;
#endif
    return std::make_tuple(unityGfxRenderer, CreateDevice());
}


void GraphicsDeviceTestBase::SetUp()
{
    UnityGfxRenderer unityGfxRenderer;
    void* pGraphicsDevice;
    std::tie(unityGfxRenderer, pGraphicsDevice) = GetParam();

    ASSERT_TRUE(GraphicsDevice::GetInstance().Init(unityGfxRenderer, pGraphicsDevice, nullptr));
    m_device = GraphicsDevice::GetInstance().GetDevice();
    ASSERT_NE(nullptr, m_device);
}
void GraphicsDeviceTestBase::TearDown()
{
    GraphicsDevice::GetInstance().Shutdown();
}
