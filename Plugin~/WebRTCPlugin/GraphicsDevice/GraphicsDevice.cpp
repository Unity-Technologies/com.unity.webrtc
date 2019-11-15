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

void GraphicsDevice::Init(IUnityInterfaces* unityInterface) {

    m_rendererType = unityInterface->Get<IUnityGraphics>()->GetRenderer();
    switch (m_rendererType) {
        case kUnityGfxRendererD3D11: {
#if defined(SUPPORT_D3D11)
            m_device = new D3D11GraphicsDevice(unityInterface->Get<IUnityGraphicsD3D11>()->GetDevice());
#endif
            break;
        }
        case kUnityGfxRendererD3D12: {
#if defined(SUPPORT_D3D12)
            m_device = new D3D12GraphicsDevice(unityInterface->Get<IUnityGraphicsD3D12v2>()->GetDevice());
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
            return;
        }
    }
    m_device->InitV();
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

