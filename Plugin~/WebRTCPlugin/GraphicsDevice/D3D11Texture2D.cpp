#include "pch.h"
#include "D3D11Texture2D.h"

namespace WebRTC {

extern ID3D11Device* g_D3D11Device;

//---------------------------------------------------------------------------------------------------------------------

D3D11Texture2D::D3D11Texture2D(uint32_t w, uint32_t h, ID3D11Texture2D* tex) : ITexture2D(w,h)
    , m_texture(tex)
{

}

} //end namespace
