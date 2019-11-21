#include "pch.h"
#include "MetalTexture2D.h"

namespace WebRTC {

//---------------------------------------------------------------------------------------------------------------------

    MetalTexture2D::MetalTexture2D(uint32_t w, uint32_t h, void* tex) : ITexture2D(w,h)
            , m_texture(static_cast<MTLTexture*>(tex))
    {

    }

} //end namespace
