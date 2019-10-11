#pragma once

#include "GraphicsDevice/IGraphicsDevice.h"
#include "WebRTCConstants.h"

namespace WebRTC {


class D3D11GraphicsDevice : public IGraphicsDevice{
public:
    D3D11GraphicsDevice(ID3D11Device* nativeDevice);
    virtual ~D3D11GraphicsDevice();
    virtual void ShutdownV();
    virtual ITexture2D* CreateEncoderInputTextureV(uint32_t width, uint32_t height);

private:
    ID3D11Device* m_d3d11Device;
    ID3D11DeviceContext* m_d3d11Context; 
    ID3D11Texture2D* m_renderTextures[NUM_TEXTURES_FOR_BUFFERING]; 



};


}
