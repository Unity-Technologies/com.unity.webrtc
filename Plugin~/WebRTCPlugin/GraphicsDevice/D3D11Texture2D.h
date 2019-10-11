#pragma once

#include "ITexture2D.h"
#include "d3d11.h"

namespace WebRTC {

struct D3D11Texture2D : ITexture2D {
public:
    ID3D11Texture2D* m_texture;

    D3D11Texture2D(int w, int h);

    virtual ~D3D11Texture2D() {
        m_texture->Release();
        m_texture = nullptr;
    }

    inline virtual void* GetNativeTexturePtrV();
    inline virtual const void* GetNativeTexturePtrV() const;

};

//---------------------------------------------------------------------------------------------------------------------

void* D3D11Texture2D::GetNativeTexturePtrV() { return m_texture; }
const void* D3D11Texture2D::GetNativeTexturePtrV() const { return m_texture; }

} //end namespace


