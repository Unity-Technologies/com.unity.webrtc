#include "pch.h"
#include "GraphicsDevice.h"
#include "WebRTCConstants.h" //NUM_TEXTURES_FOR_BUFFERING
#include "IGraphicsDevice.h"

//Graphics
#if defined(SUPPORT_D3D11) || defined(SUPPORT_D3D12)
#include "D3D11/D3D11GraphicsDevice.h" 
#include "D3D12/D3D12GraphicsDevice.h" 
#endif

#if defined(SUPPORT_OPENGL_CORE)
#include "OpenGL/OpenGLGraphicsDevice.h"
#endif

namespace WebRTC {

GraphicsDevice& GraphicsDevice::GetInstance() {
    static GraphicsDevice device;
    return device;
}

//---------------------------------------------------------------------------------------------------------------------

bool GraphicsDevice::Init(IUnityInterfaces* unityInterface) {

    m_rendererType = unityInterface->Get<IUnityGraphics>()->GetRenderer();
    void* device = nullptr;
    switch (m_rendererType) {
        case kUnityGfxRendererD3D11: {
#if defined(SUPPORT_D3D11)
            device = unityInterface->Get<IUnityGraphicsD3D11>()->GetDevice();
#endif
            break;
        }
        case kUnityGfxRendererD3D12: {
#if defined(SUPPORT_D3D12)
            device = unityInterface->Get<IUnityGraphicsD3D12>()->GetDevice();
#endif
            break;
        }
        case kUnityGfxRendererOpenGLCore: {
        }
        default: {
            DebugError("Unsupported Unity Renderer: %d", m_rendererType);
            return false;
        }
    }
    return Init(m_rendererType, device);
}

//---------------------------------------------------------------------------------------------------------------------

bool GraphicsDevice::Init(UnityGfxRenderer rendererType, void* device)
{
    switch (rendererType) {
    case kUnityGfxRendererD3D11: {
#if defined(SUPPORT_D3D11)
        m_device = new D3D11GraphicsDevice(static_cast<ID3D11Device*>(device));
#endif
        break;
    }
    case kUnityGfxRendererD3D12: {
#if defined(SUPPORT_D3D12)
        m_device = new D3D12GraphicsDevice(static_cast<ID3D12Device*>(device));
#endif
        break;
    }
    case kUnityGfxRendererOpenGLCore: {
#if defined(SUPPORT_OPENGL_CORE)
        m_device = new OpenGLGraphicsDevice();
#endif
    }
    default: {
        DebugError("Unsupported Unity Renderer: %d", m_rendererType);
        return false;
    }
    }
    m_device->InitV();
    return true;
}

//---------------------------------------------------------------------------------------------------------------------

void GraphicsDevice::Shutdown() {
    if (nullptr != m_device) {
        m_device->ShutdownV();
        delete m_device;
        m_device = nullptr;
    }
}

//---------------------------------------------------------------------------------------------------------------------

GraphicsDevice::GraphicsDevice() : m_rendererType(static_cast<UnityGfxRenderer>(0)) {

}

}

