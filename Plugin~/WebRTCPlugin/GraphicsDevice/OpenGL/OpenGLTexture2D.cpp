#include "pch.h"

#include "OpenGLContext.h"
#include "OpenGLTexture2D.h"

namespace unity
{
namespace webrtc
{
    OpenGLTexture2D::OpenGLTexture2D(uint32_t w, uint32_t h, GLuint tex, ReleaseOpenGLTextureCallback callback)
        : ITexture2D(w, h)
        , m_texture(tex)
        , m_pbo(0)
        , m_sync(0)
        , m_callback(callback)
    {
        RTC_DCHECK(m_texture);
    }

    OpenGLTexture2D::~OpenGLTexture2D() { m_callback(this); }

    void OpenGLTexture2D::Release()
    {
        if (glIsTexture(m_texture))
        {
            glDeleteTextures(1, &m_texture);
        }

        if (glIsBuffer(m_pbo))
        {
            glDeleteBuffers(1, &m_pbo);
        }
    }

    void OpenGLTexture2D::CreatePBO()
    {
        RTC_DCHECK_EQ(m_pbo, 0);

        glGenBuffers(1, &m_pbo);
        glBindBuffer(GL_PIXEL_UNPACK_BUFFER, m_pbo);

        const size_t bufferSize = GetBufferSize();
        glBufferData(GL_PIXEL_UNPACK_BUFFER, bufferSize, nullptr, GL_DYNAMIC_DRAW);
        glBindBuffer(GL_PIXEL_UNPACK_BUFFER, 0);

        if (m_buffer.empty())
            m_buffer.resize(bufferSize);
    }
} // end namespace webrtc
} // end namespace unity
