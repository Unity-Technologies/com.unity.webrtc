#include "pch.h"
#include "OpenGLTexture2D.h"

namespace WebRTC {

//---------------------------------------------------------------------------------------------------------------------

OpenGLTexture2D::OpenGLTexture2D(uint32_t w, uint32_t h, GLuint* tex) : ITexture2D(w,h)
    , m_texture(tex)
{

}

} //end namespace
