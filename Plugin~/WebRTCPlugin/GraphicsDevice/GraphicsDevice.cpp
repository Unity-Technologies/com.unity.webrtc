#include "pch.h"
#include "GraphicsDevice.h"
#include "WebRTCConstants.h" //NUM_TEXTURES_FOR_BUFFERING
#include "IGraphicsDevice.h"

//D3D11
#include "D3D11/D3D11GraphicsDevice.h" 

namespace WebRTC {

GraphicsDevice& GraphicsDevice::GetInstance() {
    static GraphicsDevice device;
    return device;
}

//---------------------------------------------------------------------------------------------------------------------

void GraphicsDevice::Init(IUnityInterfaces* unityInterface) {

    m_rendererType = unityInterface->Get<IUnityGraphics>()->GetRenderer();
    if (m_rendererType == kUnityGfxRendererD3D11)
    {
        m_device = new D3D11GraphicsDevice(unityInterface->Get<IUnityGraphicsD3D11>()->GetDevice());  
    }
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

