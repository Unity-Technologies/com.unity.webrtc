#include "pch.h"

#include "D3D11Texture2D.h"
#include "NvCodecUtils.h"

namespace unity
{
namespace webrtc
{

    D3D11Texture2D::D3D11Texture2D(
        uint32_t width,
        uint32_t height,
        UnityRenderingExtTextureFormat format,
        ID3D11Texture2D* tex,
        bool externalTexture)
        : ITexture2D(width, height, format)
        , m_texture(tex)
        , m_externalTexture(externalTexture)
    {
    }
} // end namespace webrtc
} // end namespace unity
