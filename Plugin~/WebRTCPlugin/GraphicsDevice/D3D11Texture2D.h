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

    inline virtual void* GetResourcePtrV();
    inline virtual const void* GetResourcePtrV() const;

};

//---------------------------------------------------------------------------------------------------------------------

void* D3D11Texture2D::GetResourcePtrV() { return m_texture; }
const void* D3D11Texture2D::GetResourcePtrV() const { return m_texture; }

} //end namespace


