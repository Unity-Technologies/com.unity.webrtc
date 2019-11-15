#pragma once

#include "GraphicsDevice/ITexture2D.h"
#include "d3d11.h"

#include "WebRTCMacros.h"

namespace WebRTC {

struct D3D12Texture2D : ITexture2D {
public:

    D3D12Texture2D(uint32_t w, uint32_t h,ID3D12Resource* nativeTex, HANDLE handle, ID3D11Texture2D* sharedTex);

    virtual ~D3D12Texture2D() {
        SAFE_RELEASE(m_sharedTexture);
        CloseHandle(m_sharedHandle);
        SAFE_RELEASE(m_nativeTexture);
    }

    inline virtual void* GetNativeTexturePtrV();
    inline virtual const void* GetNativeTexturePtrV() const;
    inline virtual void* GetEncodeTexturePtrV();
    inline virtual const void* GetEncodeTexturePtrV() const;

private:
    ID3D12Resource* m_nativeTexture;
    HANDLE m_sharedHandle;
    ID3D11Texture2D* m_sharedTexture;
};

//---------------------------------------------------------------------------------------------------------------------

void* D3D12Texture2D::GetNativeTexturePtrV() { return m_nativeTexture; }
const void* D3D12Texture2D::GetNativeTexturePtrV() const { return m_nativeTexture; };

void* D3D12Texture2D::GetEncodeTexturePtrV() { return m_sharedTexture; }
const void* D3D12Texture2D::GetEncodeTexturePtrV() const { return m_sharedTexture; }

} //end namespace


