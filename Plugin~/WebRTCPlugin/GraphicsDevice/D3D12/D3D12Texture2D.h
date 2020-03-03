#pragma once

#include "GraphicsDevice/ITexture2D.h"
#include "D3D12ResourceFootprint.h"
#include "d3d11.h"

#include "WebRTCMacros.h"

namespace WebRTC {

class D3D12Texture2D : public ITexture2D {
public:

    D3D12Texture2D(uint32_t w, uint32_t h,ID3D12Resource* nativeTex, HANDLE handle, ID3D11Texture2D* sharedTex);

    virtual ~D3D12Texture2D() {
        SAFE_DELETE(m_nativeTextureFootprint);
        SAFE_RELEASE(m_readbackResource);
        SAFE_RELEASE(m_sharedTexture);
        CloseHandle(m_sharedHandle);
        SAFE_RELEASE(m_nativeTexture);
    }

    inline virtual void* GetNativeTexturePtrV() override;
    inline virtual const void* GetNativeTexturePtrV() const override;
    inline virtual void* GetEncodeTexturePtrV() override;
    inline virtual const void* GetEncodeTexturePtrV() const override;
    HRESULT CreateReadbackResource(ID3D12Device* device);
    inline ID3D12Resource* GetReadbackResource() const;
    inline const D3D12ResourceFootprint* GetNativeTextureFootprint() const;

private:
    ID3D12Resource* m_nativeTexture;
    HANDLE m_sharedHandle;
    ID3D11Texture2D* m_sharedTexture;   //Shared between DX11 and DX12

    //For CPU Read
    ID3D12Resource* m_readbackResource;
    D3D12ResourceFootprint* m_nativeTextureFootprint;

};

//---------------------------------------------------------------------------------------------------------------------

void* D3D12Texture2D::GetNativeTexturePtrV() { return m_nativeTexture; }
const void* D3D12Texture2D::GetNativeTexturePtrV() const { return m_nativeTexture; };

void* D3D12Texture2D::GetEncodeTexturePtrV() { return m_sharedTexture; }
const void* D3D12Texture2D::GetEncodeTexturePtrV() const { return m_sharedTexture; }
ID3D12Resource* D3D12Texture2D::GetReadbackResource() const { return m_readbackResource; }
const D3D12ResourceFootprint* D3D12Texture2D::GetNativeTextureFootprint() const { return m_nativeTextureFootprint; }

} //end namespace


