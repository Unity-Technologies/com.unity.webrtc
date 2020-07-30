#include "pch.h"
#include "GraphicsDeviceTestBase.h"
#include "../WebRTCPlugin/PlatformBase.h"
#include "../WebRTCPlugin/GraphicsDevice/GraphicsDevice.h"

#if defined(SUPPORT_D3D11) // D3D11

#include <d3d11.h>
#include <wrl/client.h>
#include "../WebRTCPlugin/GraphicsDevice/D3D12/D3D12GraphicsDevice.h"

#endif

#if defined(SUPPORT_METAL)  // Metal
#import <Metal/Metal.h>
#include <DummyUnityInterface/DummyUnityGraphicsMetal.h>
#endif

#if defined(SUPPORT_OPENGL_CORE) // OpenGL

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
    auto hr = CreateDXGIFactory1(IID_PPV_ARGS(&pFactory));
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
        if (SUCCEEDED(D3D12CreateDevice(adapter.Get(), D3D_FEATURE_LEVEL_12_0, _uuidof(ID3D12Device), nullptr)))
        {
            break;
        }
    }
    *ppAdapter = adapter.Detach();
}

void* CreateDeviceD3D12()
{
    auto hr = CreateDXGIFactory2(0, IID_PPV_ARGS(&pFactory4));
    EXPECT_TRUE(SUCCEEDED(hr));
    EXPECT_NE(nullptr, pFactory4.Get());

    GetHardwareAdapter(pFactory4.Get(), &pAdapter1);
    EXPECT_NE(nullptr, pAdapter1.Get());

    hr = D3D12CreateDevice(
        pAdapter1.Get(), D3D_FEATURE_LEVEL_12_0, IID_PPV_ARGS(&pD3D12Device));
    EXPECT_TRUE(SUCCEEDED(hr));
    EXPECT_NE(nullptr, pD3D12Device.Get());

    D3D12_FEATURE_DATA_D3D12_OPTIONS3 options = {};
    hr = pD3D12Device->CheckFeatureSupport(D3D12_FEATURE_D3D12_OPTIONS3, &options, sizeof(options));
    EXPECT_TRUE(SUCCEEDED(hr));
    EXPECT_TRUE(options.WriteBufferImmediateSupportFlags & (1 << D3D12_COMMAND_LIST_TYPE_DIRECT));
    D3D12_COMMAND_QUEUE_DESC queueDesc = {};
    queueDesc.Flags = D3D12_COMMAND_QUEUE_FLAG_DISABLE_GPU_TIMEOUT;
    queueDesc.Type = D3D12_COMMAND_LIST_TYPE_DIRECT;
    queueDesc.NodeMask = kD3D12NodeMask;

    hr = pD3D12Device->CreateCommandQueue(&queueDesc, IID_PPV_ARGS(&pCommandQueue));
    EXPECT_TRUE(SUCCEEDED(hr));
    EXPECT_NE(nullptr, pCommandQueue.Get());

    return pD3D12Device.Get();
}

#endif
#if defined(SUPPORT_METAL)  // Metal

void* CreateDeviceMetal()
{
    return MTLCreateSystemDefaultDevice();
}

#endif

#if defined(SUPPORT_OPENGL_CORE) // OpenGL

static bool s_glutInitialized;

void* CreateDeviceOpenGL()
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

IUnityInterface* CreateUnityInterface(UnityGfxRenderer renderer) {

    switch(renderer)
    {
#if defined(SUPPORT_D3D11)
    case kUnityGfxRendererD3D11:
        return nullptr;
#endif
#if defined(SUPPORT_D3D12)
    case kUnityGfxRendererD3D12:
        return nullptr;
#endif
#if defined(SUPPORT_OPENGL_CORE)
    case kUnityGfxRendererOpenGLCore:
        return nullptr;
#endif
#if defined(SUPPORT_METAL)  // Metal
    case kUnityGfxRendererMetal:
        return new DummyUnityGraphicsMetal();
#endif
    }
    return nullptr;
}

//---------------------------------------------------------------------------------------------------------------------

void* CreateDevice(UnityGfxRenderer renderer)
{
    switch (renderer)
    {
#if defined(SUPPORT_D3D11)
    case kUnityGfxRendererD3D11:
        return CreateDeviceD3D11();
#endif
#if defined(SUPPORT_D3D12)
    case kUnityGfxRendererD3D12:
        return CreateDeviceD3D12();
#endif
#if defined(SUPPORT_OPENGL_CORE)
    case kUnityGfxRendererOpenGLCore:
        return CreateDeviceOpenGL();
#endif
#if defined(SUPPORT_METAL)
    case kUnityGfxRendererMetal:
        return CreateDeviceMetal();
#endif
    }
}

//---------------------------------------------------------------------------------------------------------------------

GraphicsDeviceTestBase::GraphicsDeviceTestBase()
{
    std::tie(m_unityGfxRenderer, m_encoderType) = GetParam();
    const auto pGraphicsDevice = CreateDevice(m_unityGfxRenderer);
    const auto unityInterface = CreateUnityInterface(m_unityGfxRenderer);

    if (m_unityGfxRenderer == kUnityGfxRendererD3D12)
    {
#if defined(SUPPORT_D3D12)
        m_device = new D3D12GraphicsDevice(static_cast<ID3D12Device*>(pGraphicsDevice), pCommandQueue.Get());
        m_device->InitV();
#endif
    }
    else
    {
        GraphicsDevice::GetInstance().Init(m_unityGfxRenderer, pGraphicsDevice, unityInterface);
        m_device = GraphicsDevice::GetInstance().GetDevice();
    }

    if (m_device == nullptr)
        throw;
}


GraphicsDeviceTestBase::~GraphicsDeviceTestBase()
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
