#include "pch.h"
#include "GraphicsDeviceTestBase.h"
#include "../WebRTCPlugin/PlatformBase.h"
#include "../WebRTCPlugin/GraphicsDevice/GraphicsDevice.h"

#if defined(SUPPORT_D3D11)
#include <d3d11.h>
#include <wrl/client.h>

namespace unity
{
namespace webrtc
{

Microsoft::WRL::ComPtr<IDXGIFactory1> pFactory;
Microsoft::WRL::ComPtr<IDXGIAdapter> pAdapter;
Microsoft::WRL::ComPtr<ID3D11Device> pD3D11Device;
Microsoft::WRL::ComPtr<ID3D11DeviceContext> pD3D11DeviceContext;

Microsoft::WRL::ComPtr<IDXGIAdapter1> pAdapter1;
Microsoft::WRL::ComPtr<IDXGIFactory4> pFactory4;
Microsoft::WRL::ComPtr<ID3D12Device5> pD3D12Device;
Microsoft::WRL::ComPtr<ID3D12CommandQueue> pCommandQueue;

//---------------------------------------------------------------------------------------------------------------------

void* CreateDeviceD3D11()
{
    auto hr = CreateDXGIFactory1(__uuidof(IDXGIFactory1), reinterpret_cast<void **>(pFactory.GetAddressOf()));
    EXPECT_TRUE(SUCCEEDED(hr));
    EXPECT_NE(nullptr, pFactory.Get());

    hr = pFactory->EnumAdapters(0, pAdapter.GetAddressOf());
    EXPECT_TRUE(SUCCEEDED(hr));
    EXPECT_NE(nullptr, pAdapter.Get());

    hr = D3D11CreateDevice(
        pAdapter.Get(), D3D_DRIVER_TYPE_UNKNOWN, nullptr, 0,
        nullptr, 0, D3D11_SDK_VERSION, pD3D11Device.GetAddressOf(), nullptr, pD3D11DeviceContext.GetAddressOf());

    EXPECT_TRUE(SUCCEEDED(hr));
    EXPECT_NE(nullptr, pD3D11Device.Get());
    return pD3D11Device.Get();
}

void* CreateDeviceD3D12()
{
    auto hr = CreateDXGIFactory1(IID_PPV_ARGS(&pFactory4));
    EXPECT_TRUE(SUCCEEDED(hr));
    EXPECT_NE(nullptr, pFactory4.Get());

    hr = pFactory4->EnumAdapters1(0, &pAdapter1);
    EXPECT_TRUE(SUCCEEDED(hr));
    EXPECT_NE(nullptr, pAdapter1.Get());

    hr = D3D12CreateDevice(
        pAdapter1.Get(), D3D_FEATURE_LEVEL_11_1, IID_PPV_ARGS(&pD3D12Device));
    EXPECT_TRUE(SUCCEEDED(hr));
    EXPECT_NE(nullptr, pD3D12Device.Get());

    D3D12_COMMAND_QUEUE_DESC queueDesc = {};
    queueDesc.Flags = D3D12_COMMAND_QUEUE_FLAG_NONE;
    queueDesc.Type = D3D12_COMMAND_LIST_TYPE_DIRECT;

    hr = pD3D12Device->CreateCommandQueue(&queueDesc, IID_PPV_ARGS(&pCommandQueue));
    EXPECT_TRUE(SUCCEEDED(hr));
    EXPECT_NE(nullptr, pCommandQueue.Get());

    return pD3D12Device.Get();
}

void* CreateDevice(UnityGfxRenderer renderer)
{
    switch (renderer)
    {
    case kUnityGfxRendererD3D11:
        return CreateDeviceD3D11();
    case kUnityGfxRendererD3D12:
        return CreateDeviceD3D12();
    }
}

IUnityInterface* CreateUnityInterface() {
    return nullptr;
}

#elif defined(SUPPORT_METAL)

#import <Metal/Metal.h>
#include <DummyUnityInterface/DummyUnityGraphicsMetal.h>

void* CreateDevice(UnityGfxRenderer renderer)
{
    return MTLCreateSystemDefaultDevice();
}

IUnityInterface* CreateUnityInterface() {
    return new DummyUnityGraphicsMetal();
}

#else
#include <GL/glut.h>

static bool s_glutInitialized;

void* CreateDevice(UnityGfxRenderer renderer)
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

IUnityInterface* CreateUnityInterface() {
    return nullptr;
}

#endif

//---------------------------------------------------------------------------------------------------------------------

void GraphicsDeviceTestBase::SetUp()
{
    UnityGfxRenderer unityGfxRenderer;
    std::tie(unityGfxRenderer, encoderType) = GetParam();
    const auto pGraphicsDevice = CreateDevice(unityGfxRenderer);
    const auto unityInterface = CreateUnityInterface();

    ASSERT_TRUE(GraphicsDevice::GetInstance().Init(unityGfxRenderer, pGraphicsDevice, unityInterface));
    m_device = GraphicsDevice::GetInstance().GetDevice();
    ASSERT_NE(nullptr, m_device);
}
void GraphicsDeviceTestBase::TearDown()
{
    GraphicsDevice::GetInstance().Shutdown();
}

} // end namespace webrtc
} // end namespace unity
