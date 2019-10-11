#pragma once

#include "ITexture2D.h"
#include "d3d11.h"

#include "WebRTCMacros.h"

namespace WebRTC {

struct D3D11Texture2D : ITexture2D {
public:
    ID3D11Texture2D* m_texture;

    D3D11Texture2D(uint32_t w, uint32_t h,ID3D11Texture2D* tex);

    virtual ~D3D11Texture2D() {
        SAFE_RELEASE(m_texture);
    }

    inline virtual void* GetNativeTexturePtrV();
    inline virtual const void* GetNativeTexturePtrV() const;

};

//---------------------------------------------------------------------------------------------------------------------

void* D3D11Texture2D::GetNativeTexturePtrV() { return m_texture; }
const void* D3D11Texture2D::GetNativeTexturePtrV() const { return m_texture; }

} //end namespace


