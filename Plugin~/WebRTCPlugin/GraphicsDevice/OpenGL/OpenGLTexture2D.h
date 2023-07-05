#pragma once

#if SUPPORT_OPENGL_CORE
#include <glad/gl.h>
#endif

#include "GraphicsDevice/ITexture2D.h"
#include "WebRTCMacros.h"

#if CUDA_PLATFORM
#include "GraphicsDevice/Cuda/CudaContext.h"
#endif

namespace unity
{
namespace webrtc
{

    struct OpenGLTexture2D : ITexture2D
    {
    public:
        using ReleaseOpenGLTextureCallback = std::function<void(OpenGLTexture2D*)>;
        OpenGLTexture2D(uint32_t w, uint32_t h, GLuint tex, ReleaseOpenGLTextureCallback callback);
        virtual ~OpenGLTexture2D() override;

        inline void* GetNativeTexturePtrV() override;
        inline const void* GetNativeTexturePtrV() const override;
        inline void* GetEncodeTexturePtrV() override;
        inline const void* GetEncodeTexturePtrV() const override;

        void CreatePBO();
        size_t GetBufferSize() const { return m_width * m_height * 4; }
        size_t GetPitch() const { return m_width * 4; }
        byte* GetBuffer() { return m_buffer.data(); }
        GLuint GetPBO() const { return m_pbo; }
        GLuint GetTexture() const { return m_texture; }
        void Release();

        void SetSync(GLsync sync) { m_sync = sync; }
        GLsync GetSync() const { return m_sync; }

    private:
        GLuint m_texture;
        GLuint m_pbo;
        GLsync m_sync;
        std::vector<byte> m_buffer;
        ReleaseOpenGLTextureCallback m_callback;
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