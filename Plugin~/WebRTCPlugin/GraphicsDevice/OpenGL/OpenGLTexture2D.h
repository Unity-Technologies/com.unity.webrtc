#pragma once

#include "GraphicsDevice/ITexture2D.h"
#include "WebRTCMacros.h"

namespace WebRTC {

struct OpenGLTexture2D : ITexture2D {
public:
    GLuint* m_texture;

    OpenGLTexture2D(uint32_t w, uint32_t h, GLuint* tex);

    virtual ~OpenGLTexture2D() {
        glDeleteTextures(1 , m_texture);
        m_texture;
    }

    inline virtual void* GetNativeTexturePtrV();
    inline virtual const void* GetNativeTexturePtrV() const;
    inline virtual void* GetEncodeTexturePtrV();
    inline virtual const void* GetEncodeTexturePtrV() const;

};

//---------------------------------------------------------------------------------------------------------------------

void* OpenGLTexture2D::GetNativeTexturePtrV() { return m_texture; }
const void* OpenGLTexture2D::GetNativeTexturePtrV() const { return m_texture; };
void* OpenGLTexture2D::GetEncodeTexturePtrV() { return m_texture; }
const void* OpenGLTexture2D::GetEncodeTexturePtrV() const { return m_texture; }

} //end namespace


