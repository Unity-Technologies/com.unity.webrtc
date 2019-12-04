#pragma once

#include "GraphicsDevice/IGraphicsDevice.h"
#include "WebRTCConstants.h"

namespace WebRTC {


class D3D12GraphicsDevice : public IGraphicsDevice{
public:
    D3D12GraphicsDevice(ID3D12Device* nativeDevice);
    virtual ~D3D12GraphicsDevice();
    virtual bool InitV() override;
    virtual void ShutdownV() override;
    inline virtual void* GetEncodeDevicePtrV() override;

    virtual ITexture2D* CreateDefaultTextureV(uint32_t w, uint32_t h) override;
    virtual ITexture2D* CreateCPUReadTextureV(uint32_t width, uint32_t height) override;
    virtual bool CopyResourceV(ITexture2D* dest, ITexture2D* src) override;
    virtual bool CopyResourceFromNativeV(ITexture2D* dest, void* nativeTexturePtr) override;
    inline virtual GraphicsDeviceType GetDeviceType() const override;
    virtual rtc::scoped_refptr<webrtc::I420Buffer> ConvertRGBToI420(ITexture2D* tex) override;

private:

    ITexture2D* CreateSharedD3D12Texture(uint32_t w, uint32_t h);

    ID3D12Device* m_d3d12Device;
    //ID3D12DeviceContext* m_d3d12Context;

    //[Note-sin: 2019-10-30] sharing res from d3d12 to d3d11 require d3d11.1. Fence is supported in d3d11.4 or newer.
    ID3D11Device5* m_d3d11Device;
    ID3D11DeviceContext4* m_d3d11Context;

};

//---------------------------------------------------------------------------------------------------------------------

//use D3D11. See notes below
void* D3D12GraphicsDevice::GetEncodeDevicePtrV() { return reinterpret_cast<void*>(m_d3d11Device); }
GraphicsDeviceType D3D12GraphicsDevice::GetDeviceType() const { return GRAPHICS_DEVICE_D3D12; }

}

//---------------------------------------------------------------------------------------------------------------------
//[Note-sin: 2019-10-30]
//Since NVEncoder does not support DX12, we copy the texture from DX12 resource to DX11 resource first (GPU), and then
//pass it to NVidia

