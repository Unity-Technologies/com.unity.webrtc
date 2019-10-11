#include "pch.h"
#include "GraphicsDevice.h"
#include "WebRTCConstants.h" //NUM_TEXTURES_FOR_BUFFERING
#include "IGraphicsDevice.h"

//D3D11
#include "D3D11/D3D11GraphicsDevice.h" 

namespace WebRTC {

ID3D11DeviceContext* context; ////d3d11 context
ID3D11Device* g_D3D11Device = nullptr; ////d3d11 device
ID3D11Texture2D* g_renderTextures[NUM_TEXTURES_FOR_BUFFERING]; ////natively created ID3D11Texture2D ptrs

GraphicsDevice& GraphicsDevice::GetInstance() {
    static GraphicsDevice device;
    return device;
}

//---------------------------------------------------------------------------------------------------------------------

void GraphicsDevice::Init(IUnityInterfaces* unityInterface) {

    m_rendererType = unityInterface->Get<IUnityGraphics>()->GetRenderer();
    if (m_rendererType == kUnityGfxRendererD3D11)
    {
        g_D3D11Device = unityInterface->Get<IUnityGraphicsD3D11>()->GetDevice();
        m_device = new D3D11GraphicsDevice(g_D3D11Device);  
        g_D3D11Device->GetImmediateContext(&context);
    }
}

//---------------------------------------------------------------------------------------------------------------------

void GraphicsDevice::Shutdown() {
    m_device->ShutdownV();
    m_device = nullptr;

    for (auto rt : g_renderTextures)  {
        if (rt) {
            rt->Release();
            rt = nullptr;
        }
    }
}

//---------------------------------------------------------------------------------------------------------------------
ITexture2D* GraphicsDevice::CreateEncoderInputTexture(uint32_t w , uint32_t h ) {
    return m_device->CreateEncoderInputTextureV(w,h);
}

//---------------------------------------------------------------------------------------------------------------------

GraphicsDevice::GraphicsDevice() : m_rendererType(static_cast<UnityGfxRenderer>(0)) {

}

}

