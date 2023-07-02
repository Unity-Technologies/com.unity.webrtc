#include "pch.h"

#include "MetalTexture2D.h"

namespace unity
{
namespace webrtc
{

    MetalTexture2D::MetalTexture2D(uint32_t w, uint32_t h, id<MTLTexture> tex)
        : ITexture2D(w, h)
        , m_texture(tex)
        , m_semaphore(dispatch_semaphore_create(1))
    {
    }

    MetalTexture2D::~MetalTexture2D()
    {
        dispatch_release(m_semaphore);
        [m_texture release];
    }

} // end namespace webrtc
} // end namespace unity
