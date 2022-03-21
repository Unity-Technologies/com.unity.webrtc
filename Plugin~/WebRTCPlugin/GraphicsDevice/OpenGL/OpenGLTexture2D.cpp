#include "pch.h"

#include "OpenGLTexture2D.h"
#include "OpenGLContext.h"

#if CUDA_PLATFORM
#include <cudaGL.h>
#endif

namespace unity
{
namespace webrtc
{

//---------------------------------------------------------------------------------------------------------------------

OpenGLTexture2D::OpenGLTexture2D(uint32_t w, uint32_t h, GLuint tex, ReleaseOpenGLTextureCallback callback)
    : ITexture2D(w,h)
    , m_texture(tex)
    , m_buffer(nullptr)
    , m_callback(callback)
{
      RTC_DCHECK(m_texture);
}

OpenGLTexture2D::~OpenGLTexture2D()
{
    m_callback(this);
}

void OpenGLTexture2D::Release()
{
    glDeleteTextures(1, &m_texture);
    m_texture = 0;

    glBindBuffer(GL_ARRAY_BUFFER, m_pbo);
    glDeleteBuffers(1, &m_pbo);
    m_pbo = 0;

    free(m_buffer);
    m_buffer = nullptr;
}

void OpenGLTexture2D::CreatePBO()
{
    glGenBuffers(1, &m_pbo);
    glBindBuffer(GL_PIXEL_UNPACK_BUFFER, m_pbo);

    const size_t bufferSize = GetBufferSize();
    glBufferData(GL_PIXEL_UNPACK_BUFFER, bufferSize, nullptr, GL_DYNAMIC_DRAW);
    glBindBuffer(GL_PIXEL_UNPACK_BUFFER, 0);

    if(m_buffer == nullptr)
    {
        m_buffer = static_cast<byte*>(malloc(bufferSize));
    }
}
} // end namespace webrtc
} // end namespace unity