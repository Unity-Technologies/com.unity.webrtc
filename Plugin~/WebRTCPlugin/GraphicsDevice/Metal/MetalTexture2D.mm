#include "pch.h"

#include "MetalTexture2D.h"

namespace unity
{
namespace webrtc
{
    MetalTexture2D::MetalTexture2D(
        uint32_t width, uint32_t height, UnityRenderingExtTextureFormat format, id<MTLTexture> tex)
        : ITexture2D(width, height, format)
        , m_texture(tex)
    {
    }

    MetalTexture2D::~MetalTexture2D() { m_texture = nullptr; }

} // end namespace webrtc
} // end namespace unity
