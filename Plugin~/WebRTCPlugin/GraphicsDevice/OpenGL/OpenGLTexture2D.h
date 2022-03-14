#pragma once

#include "GraphicsDevice/ITexture2D.h"
#include "WebRTCMacros.h"

#if CUDA_PLATFORM
#include "GraphicsDevice/Cuda/CudaContext.h"
#endif


namespace unity
{
namespace webrtc
{

struct OpenGLTexture2D : ITexture2D {
public:
    OpenGLTexture2D(uint32_t w, uint32_t h, GLuint tex);
    virtual ~OpenGLTexture2D();

    inline void* GetNativeTexturePtrV() override;
    inline const void* GetNativeTexturePtrV() const override;
    inline void* GetEncodeTexturePtrV() override;
    inline const void* GetEncodeTexturePtrV() const override;

    std::unique_ptr<GpuMemoryBufferHandle> Map() override;

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

// todo(kazuki):: fix workaround
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Wint-to-void-pointer-cast"
void* OpenGLTexture2D::GetNativeTexturePtrV() { return (void*)m_texture; }
const void* OpenGLTexture2D::GetNativeTexturePtrV() const { return (void*)m_texture; };
void* OpenGLTexture2D::GetEncodeTexturePtrV() { return (void*)m_texture; }
const void* OpenGLTexture2D::GetEncodeTexturePtrV() const { return (const void*)m_texture; }
#pragma clang diagnostic pop

} // end namespace webrtd
} // end namespace unity