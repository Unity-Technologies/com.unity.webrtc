#include "pch.h"

#include "D3D11Texture2D.h"
#include "NvCodecUtils.h"

namespace unity
{
namespace webrtc
{

    D3D11Texture2D::D3D11Texture2D(uint32_t w, uint32_t h, ID3D11Texture2D* tex, ID3D11Fence* fence)
        : ITexture2D(w, h)
        , m_texture(tex)
        , m_fence(fence)
    {
    }
} // end namespace webrtc
} // end namespace unity
