#include "pch.h"
#include "GraphicsDeviceTestBase.h"
#include "../WebRTCPlugin/PlatformBase.h"
#include "../WebRTCPlugin/GraphicsDevice/GraphicsDevice.h"

#if defined(SUPPORT_D3D11) // D3D11

#include <d3d11.h>
#include <wrl/client.h>
#include "../WebRTCPlugin/GraphicsDevice/D3D12/D3D12GraphicsDevice.h"

#elif defined(SUPPORT_METAL)  // Metal
#import <Metal/Metal.h>
#include <DummyUnityInterface/DummyUnityGraphicsMetal.h>

#else // OpenGL

#include <GL/glut.h>

#endif

namespace unity
{
namespace webrtc
{

#if defined(SUPPORT_D3D11) // D3D11

Microsoft::WRL::ComPtr<IDXGIFactory1> pFactory;
Microsoft::WRL::ComPtr<IDXGIAdapter> pAdapter;
Microsoft::WRL::ComPtr<ID3D11Device> pD3D11Device;
Microsoft::WRL::ComPtr<ID3D11DeviceContext> pD3D11DeviceContext;

Microsoft::WRL::ComPtr<IDXGIAdapter1> pAdapter1;
Microsoft::WRL::ComPtr<IDXGIFactory4> pFactory4;
Microsoft::WRL::ComPtr<ID3D12Device5> pD3D12Device;
Microsoft::WRL::ComPtr<ID3D12CommandQueue> pCommandQueue;

const int kD3D12NodeMask = 0;

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

// Helper function for acquiring the first available hardware adapter that supports Direct3D 12.
// If no such adapter can be found, *ppAdapter will be set to nullptr.
void GetHardwareAdapter(IDXGIFactory2* pFactory, IDXGIAdapter1** ppAdapter)
{
    Microsoft::WRL::ComPtr<IDXGIAdapter1> adapter;
    *ppAdapter = nullptr;

    for (UINT adapterIndex = 0; DXGI_ERROR_NOT_FOUND != pFactory->EnumAdapters1(adapterIndex, &adapter); ++adapterIndex)
    {
        DXGI_ADAPTER_DESC1 desc;
        adapter->GetDesc1(&desc);

        if (desc.Flags & DXGI_ADAPTER_FLAG_SOFTWARE)
        {
            // Don't select the Basic Render Driver adapter.
            // If you want a software adapter, pass in "/warp" on the command line.
            continue;
        }

        // Check to see if the adapter supports Direct3D 12, but don't create the
        // actual device yet.
        if (SUCCEEDED(D3D12CreateDevice(adapter.Get(), D3D_FEATURE_LEVEL_11_0, _uuidof(ID3D12Device), nullptr)))
        {
            break;
        }
    }
    *ppAdapter = adapter.Detach();
}

void* CreateDeviceD3D12()
{
    auto hr = CreateDXGIFactory1(IID_PPV_ARGS(&pFactory4));
    EXPECT_TRUE(SUCCEEDED(hr));
    EXPECT_NE(nullptr, pFactory4.Get());

    GetHardwareAdapter(pFactory4.Get(), &pAdapter1);
    EXPECT_NE(nullptr, pAdapter1.Get());

    hr = D3D12CreateDevice(
        pAdapter1.Get(), D3D_FEATURE_LEVEL_11_1, IID_PPV_ARGS(&pD3D12Device));
    EXPECT_TRUE(SUCCEEDED(hr));
    EXPECT_NE(nullptr, pD3D12Device.Get());

    D3D12_COMMAND_QUEUE_DESC queueDesc = {};
    queueDesc.Flags = D3D12_COMMAND_QUEUE_FLAG_DISABLE_GPU_TIMEOUT;
    queueDesc.Type = D3D12_COMMAND_LIST_TYPE_DIRECT;
    queueDesc.NodeMask = kD3D12NodeMask;

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

#elif defined(SUPPORT_METAL)  // Metal

void* CreateDevice(UnityGfxRenderer renderer)
{
    return MTLCreateSystemDefaultDevice();
}

IUnityInterface* CreateUnityInterface() {
    return new DummyUnityGraphicsMetal();
}

#else // OpenGL

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
    std::tie(m_unityGfxRenderer, m_encoderType) = GetParam();
    const auto pGraphicsDevice = CreateDevice(m_unityGfxRenderer);
    const auto unityInterface = CreateUnityInterface();

    if (m_unityGfxRenderer == kUnityGfxRendererD3D12)
    {
#if defined(SUPPORT_D3D12)
        m_device = new D3D12GraphicsDevice(static_cast<ID3D12Device*>(pGraphicsDevice), pCommandQueue.Get());
        ASSERT_TRUE(m_device->InitV());
#endif
    }
    else
    {
        ASSERT_TRUE(GraphicsDevice::GetInstance().Init(m_unityGfxRenderer, pGraphicsDevice, unityInterface));
        m_device = GraphicsDevice::GetInstance().GetDevice();
    }

    ASSERT_NE(nullptr, m_device);
}
void GraphicsDeviceTestBase::TearDown()
{
    if (m_unityGfxRenderer == kUnityGfxRendererD3D12)
    {
#if defined(SUPPORT_D3D12)
        m_device->ShutdownV();
        m_device = nullptr;
#endif
    }
    else
    {
        GraphicsDevice::GetInstance().Shutdown();
    }
}

} // end namespace webrtc
} // end namespace unity
