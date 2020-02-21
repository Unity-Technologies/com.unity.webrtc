#pragma once

#include "GraphicsDevice/IGraphicsDevice.h"
#include "WebRTCConstants.h"
#include "D3D12Texture2D.h"

namespace WebRTC {

#define DefPtr(_a) _COM_SMARTPTR_TYPEDEF(_a, __uuidof(_a))
DefPtr(ID3D12CommandAllocator);
DefPtr(ID3D12GraphicsCommandList4);

class D3D12GraphicsDevice : public IGraphicsDevice{
public:
    explicit D3D12GraphicsDevice(ID3D12Device* nativeDevice, IUnityGraphicsD3D12v5* unityInterface );
    virtual ~D3D12GraphicsDevice();
    virtual bool InitV() override;
    virtual void ShutdownV() override;
    inline virtual void* GetEncodeDevicePtrV() override;

    virtual ITexture2D* CreateDefaultTextureV(uint32_t w, uint32_t h) override;
    virtual bool CopyResourceV(ITexture2D* dest, ITexture2D* src) override;
    virtual bool CopyResourceFromNativeV(ITexture2D* dest, void* nativeTexturePtr) override;
    inline virtual GraphicsDeviceType GetDeviceType() const override;

    virtual ITexture2D* CreateCPUReadTextureV(uint32_t w, uint32_t h) override;
    virtual rtc::scoped_refptr<webrtc::I420Buffer> ConvertRGBToI420(ITexture2D* tex) override;

private:

    WebRTC::D3D12Texture2D* CreateSharedD3D12Texture(uint32_t w, uint32_t h);
    void WaitForFence(ID3D12Fence* fence, HANDLE handle, uint64_t* fenceValue);
    void Barrier(ID3D12Resource* res,
        const D3D12_RESOURCE_STATES stateBefore, const D3D12_RESOURCE_STATES stateAfter,
        const UINT subresource = D3D12_RESOURCE_BARRIER_ALL_SUBRESOURCES);

    ID3D12Device* m_d3d12Device;

    //[Note-sin: 2019-10-30] sharing res from d3d12 to d3d11 require d3d11.1. Fence is supported in d3d11.4 or newer.
    ID3D11Device5* m_d3d11Device;
    ID3D11DeviceContext4* m_d3d11Context;


    //[TODO-sin: 2019-12-2] //This should be allocated for each frame.
    ID3D12CommandAllocatorPtr m_commandAllocator;
    ID3D12GraphicsCommandList4Ptr m_commandList;
    IUnityGraphicsD3D12v5* m_unityInterface;

    //Fence to copy resource on GPU (and CPU if the texture was created with CPU-access)
    ID3D12Fence* m_copyResourceFence;
	HANDLE m_copyResourceEventHandle;
    uint64_t m_copyResourceFenceValue = 1;
};

//---------------------------------------------------------------------------------------------------------------------

//use D3D11. See notes below
void* D3D12GraphicsDevice::GetEncodeDevicePtrV() { return reinterpret_cast<void*>(m_d3d11Device); }
GraphicsDeviceType D3D12GraphicsDevice::GetDeviceType() const { return GRAPHICS_DEVICE_D3D12; }

}

//---------------------------------------------------------------------------------------------------------------------
//[Note-sin: 2019-10-30]
//Since NVEncoder does not support DX12, we use a DX12 resource that can be shared with DX11, and then pass it
//the DX11 resource to NVidia Encoder

