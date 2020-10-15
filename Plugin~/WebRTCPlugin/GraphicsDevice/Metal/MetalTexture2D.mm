#include "pch.h"
#include "MetalTexture2D.h"

namespace unity
{
namespace webrtc
{

//---------------------------------------------------------------------------------------------------------------------

    MetalTexture2D::MetalTexture2D(uint32_t w, uint32_t h, id<MTLTexture> tex)
    : ITexture2D(w,h) , m_texture(tex)
    {
    }

    MetalTexture2D::~MetalTexture2D()
    {
        m_texture = nullptr;
    }
    
} // end namespace webrtc
} // end namespace unity
