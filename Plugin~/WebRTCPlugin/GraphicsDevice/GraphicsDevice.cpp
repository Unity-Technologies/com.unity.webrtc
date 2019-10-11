#include "pch.h"
#include "GraphicsDevice.h"
#include "WebRTCConstants.h" //NUM_TEXTURES_FOR_BUFFERING

//D3D11
#include "D3D11Texture2D.h" 

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
        g_D3D11Device->GetImmediateContext(&context);
    }
}

//---------------------------------------------------------------------------------------------------------------------

void GraphicsDevice::Shutdown() {
    for (auto rt : g_renderTextures)  {
        if (rt) {
            rt->Release();
            rt = nullptr;
        }
    }
}

//---------------------------------------------------------------------------------------------------------------------
ITexture2D* GraphicsDevice::CreateEncoderInputTexture(uint32_t w , uint32_t h ) {

    //[TODO-sin: 2019-10-11] Move this to GraphicsDeviceDX11
    ID3D11Texture2D* texture = nullptr;
    D3D11_TEXTURE2D_DESC desc = { 0 };
    desc.Width = w;
    desc.Height = h;
    desc.MipLevels = 1;
    desc.ArraySize = 1;
    desc.Format = DXGI_FORMAT_B8G8R8A8_UNORM;
    desc.SampleDesc.Count = 1;
    desc.Usage = D3D11_USAGE_DEFAULT;
    desc.BindFlags = D3D11_BIND_RENDER_TARGET;
    desc.CPUAccessFlags = 0;
    HRESULT r = g_D3D11Device->CreateTexture2D(&desc, NULL, &texture);

    return new D3D11Texture2D(w,h,texture);
}

//---------------------------------------------------------------------------------------------------------------------

GraphicsDevice::GraphicsDevice() : m_rendererType(static_cast<UnityGfxRenderer>(0)) {

}

}

