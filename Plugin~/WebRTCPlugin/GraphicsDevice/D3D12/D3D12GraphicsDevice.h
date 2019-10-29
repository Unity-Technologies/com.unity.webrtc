#pragma once

#include "GraphicsDevice/IGraphicsDevice.h"
#include "WebRTCConstants.h"

namespace WebRTC {


class D3D12GraphicsDevice : public IGraphicsDevice{
public:
    D3D12GraphicsDevice(ID3D12Device* nativeDevice);
    virtual ~D3D12GraphicsDevice();
    virtual void ShutdownV();
    inline virtual void* GetNativeDevicePtrV();

    virtual ITexture2D* CreateEncoderInputTextureV(uint32_t w, uint32_t h);
    virtual ITexture2D* CreateEncoderInputTextureV(uint32_t w, uint32_t h, void* nativeTexturePtr);
    virtual void CopyNativeResourceV(void* dest, void* src);


private:
    ID3D12Device* m_d3d12Device;
    //ID3D12DeviceContext* m_d3d12Context; 
};

//---------------------------------------------------------------------------------------------------------------------

void* D3D12GraphicsDevice::GetNativeDevicePtrV() { return reinterpret_cast<void*>(m_d3d12Device); }



}
