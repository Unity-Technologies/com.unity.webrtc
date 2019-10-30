#pragma once

#include "GraphicsDevice/IGraphicsDevice.h"
#include "WebRTCConstants.h"

namespace WebRTC {


class D3D12GraphicsDevice : public IGraphicsDevice{
public:
    D3D12GraphicsDevice(ID3D12Device* nativeDevice);
    virtual ~D3D12GraphicsDevice();
    virtual void InitV();
    virtual void ShutdownV();
    inline virtual void* GetEncodeDevicePtrV();

    virtual ITexture2D* CreateDefaultTextureV(uint32_t w, uint32_t h);
    virtual ITexture2D* CreateDefaultTextureFromNativeV(uint32_t w, uint32_t h, void* nativeTexturePtr);
    virtual void CopyNativeResourceV(void* dest, void* src);


private:

    ITexture2D* CreateD3D12Texture(uint32_t w, uint32_t h);

    ID3D12Device* m_d3d12Device;
    //ID3D12DeviceContext* m_d3d12Context;

    ID3D11Device* m_d3d11Device;
    ID3D11DeviceContext* m_d3d11Context;

};

//---------------------------------------------------------------------------------------------------------------------

//use D3D11. See notes below
void* D3D12GraphicsDevice::GetEncodeDevicePtrV() { return reinterpret_cast<void*>(m_d3d11Device); }

//[Note-sin: 2019-10-30]
//Since NVEncoder does not support DX12, we copy the texture from DX12 resource to DX11 resource first (GPU), and then
//pass it to NVidia


}
