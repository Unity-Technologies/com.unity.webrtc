#pragma once

#include "GraphicsDevice/ITexture2D.h"
#include "WebRTCMacros.h"

namespace unity
{
namespace webrtc
{

struct OpenGLTexture2D : ITexture2D {
public:
    OpenGLTexture2D(uint32_t w, uint32_t h, GLuint tex);
    virtual ~OpenGLTexture2D();

    inline virtual void* GetNativeTexturePtrV();
    inline virtual const void* GetNativeTexturePtrV() const;
    inline virtual void* GetEncodeTexturePtrV();
    inline virtual const void* GetEncodeTexturePtrV() const;

    void CreatePBO();
    size_t GetBufferSize() const { return m_width * m_height * 4; }
    size_t GetPitch() const { return m_width * 4; }
    byte* GetBuffer() const { return m_buffer;  }
    GLuint GetPBO() const { return m_pbo; }
private:
    GLuint m_texture;
    GLuint m_pbo;
    byte* m_buffer = nullptr;
};

//---------------------------------------------------------------------------------------------------------------------

void* OpenGLTexture2D::GetNativeTexturePtrV() { return (void*)m_texture; }
const void* OpenGLTexture2D::GetNativeTexturePtrV() const { return (void*)m_texture; };
void* OpenGLTexture2D::GetEncodeTexturePtrV() { return (void*)m_texture; }
const void* OpenGLTexture2D::GetEncodeTexturePtrV() const { return (const void*)m_texture; }

} // end namespace webrtd
} // end namespace unity