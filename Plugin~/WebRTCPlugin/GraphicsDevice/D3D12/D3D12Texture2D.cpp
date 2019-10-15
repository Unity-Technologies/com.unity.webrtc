#include "pch.h"
#include "D3D12Texture2D.h"

namespace WebRTC {

//---------------------------------------------------------------------------------------------------------------------

D3D12Texture2D::D3D12Texture2D(uint32_t w, uint32_t h, ID3D12Resource* tex) : ITexture2D(w,h)
    , m_texture(tex)
{

}

} //end namespace
