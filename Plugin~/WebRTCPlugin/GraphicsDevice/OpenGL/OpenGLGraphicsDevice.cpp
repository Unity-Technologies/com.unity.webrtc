#include "pch.h"
#include "OpenGLGraphicsDevice.h"
#include "OpenGLTexture2D.h"
#include "GraphicsDevice/GraphicsUtility.h"

namespace unity
{
namespace webrtc
{

OpenGLGraphicsDevice::OpenGLGraphicsDevice()
{
}

//---------------------------------------------------------------------------------------------------------------------
OpenGLGraphicsDevice::~OpenGLGraphicsDevice() {

}

//---------------------------------------------------------------------------------------------------------------------
bool OpenGLGraphicsDevice::InitV()
{
#if _DEBUG
    GLuint unusedIds = 0;
    glEnable(GL_DEBUG_OUTPUT);
    glEnable(GL_DEBUG_OUTPUT_SYNCHRONOUS);
#if SUPPORT_OPENGL_CORE
    glDebugMessageCallback(OnOpenGLDebugMessage, nullptr);
    glDebugMessageControl(GL_DONT_CARE, GL_DONT_CARE, GL_DONT_CARE, 0, &unusedIds, true);
#endif    
#endif

#if CUDA_PLATFORM
    m_isCudaSupport = CUDA_SUCCESS == m_cudaContext.InitGL();
#endif
    return true;
}

//---------------------------------------------------------------------------------------------------------------------

void OpenGLGraphicsDevice::ShutdownV() {
#if CUDA_PLATFORM
    m_cudaContext.Shutdown();
#endif
}

//---------------------------------------------------------------------------------------------------------------------
ITexture2D* OpenGLGraphicsDevice::CreateDefaultTextureV(
    uint32_t w, uint32_t h, UnityRenderingExtTextureFormat textureFormat) 
{
    GLuint tex;
    glGenTextures(1, &tex);
    glBindTexture(GL_TEXTURE_2D, tex);
    glTexStorage2D(GL_TEXTURE_2D, 1, GL_RGBA8, w, h);
    glBindTexture(GL_TEXTURE_2D, 0);
    return new OpenGLTexture2D(w, h, tex);
}

//---------------------------------------------------------------------------------------------------------------------
ITexture2D* OpenGLGraphicsDevice::CreateCPUReadTextureV(
    uint32_t w, uint32_t h, UnityRenderingExtTextureFormat textureFormat)
{
    OpenGLTexture2D* tex = static_cast<OpenGLTexture2D*>(CreateDefaultTextureV(w, h, textureFormat));
    tex->CreatePBO();
    return tex;
}

//---------------------------------------------------------------------------------------------------------------------
bool OpenGLGraphicsDevice::CopyResourceV(ITexture2D* dest, ITexture2D* src) {
    const uint32_t width = dest->GetWidth();
    const uint32_t height  = dest->GetHeight();
    const GLuint dstName = reinterpret_cast<uintptr_t>(dest->GetNativeTexturePtrV());
    const GLuint srcName = reinterpret_cast<uintptr_t>(src->GetNativeTexturePtrV());
    return CopyResource(dstName, srcName, width, height);
}

//---------------------------------------------------------------------------------------------------------------------
bool OpenGLGraphicsDevice::CopyResourceFromNativeV(ITexture2D* dest, void* nativeTexturePtr) {
    const uint32_t width = dest->GetWidth();
    const uint32_t height  = dest->GetHeight();
    const GLuint dstName = reinterpret_cast<uintptr_t>(dest->GetNativeTexturePtrV());
    const GLuint srcName = reinterpret_cast<uintptr_t>(nativeTexturePtr);
    return CopyResource(dstName, srcName, width, height);
}

bool OpenGLGraphicsDevice::CopyResource(GLuint dstName, GLuint srcName, uint32 width, uint32 height) {
    if(srcName == dstName)
    {
        RTC_LOG(LS_INFO) << "Same texture";
        return false;
    }
    if(glIsTexture(srcName) == GL_FALSE)
    {
        RTC_LOG(LS_INFO) << "srcName is not texture";
        return false;
    }
    if(glIsTexture(dstName) == GL_FALSE)
    {
        RTC_LOG(LS_INFO) << "dstName is not texture";
        return false;
    }
#if SUPPORT_OPENGL_CORE
    glCopyImageSubData(
            srcName, GL_TEXTURE_2D, 0, 0, 0, 0,
            dstName, GL_TEXTURE_2D, 0, 0, 0, 0,
            width, height, 1);
#elif SUPPORT_OPENGL_ES
    GLuint fbo = 0;
    glGenFramebuffers(1, &fbo);
    glBindFramebuffer(GL_FRAMEBUFFER, fbo);
    
    glFramebufferTexture2D(
        GL_READ_FRAMEBUFFER, GL_COLOR_ATTACHMENT0, GL_TEXTURE_2D, srcName, 0);
    glBindTexture(GL_TEXTURE_2D, dstName);
    glCopyTexImage2D(GL_TEXTURE_2D, 0, GL_RGBA, 0, 0, width, height, 0);
    glDeleteFramebuffers(1, &fbo);
#endif
    return true;
}

rtc::scoped_refptr<webrtc::I420Buffer> OpenGLGraphicsDevice::ConvertRGBToI420(ITexture2D* tex)
{
    OpenGLTexture2D* sourceTex = static_cast<OpenGLTexture2D*>(tex);
    const GLuint sourceId = reinterpret_cast<uintptr_t>(sourceTex->GetNativeTexturePtrV());
    const GLuint pbo = sourceTex->GetPBO();
    const uint32_t width = sourceTex->GetWidth();
    const uint32_t height = sourceTex->GetHeight();
    const uint32_t pitch = sourceTex->GetPitch();
    const uint32_t bufferSize = sourceTex->GetBufferSize();
    byte* data = sourceTex->GetBuffer();

    // Send normal texture data to the PBO
    glBindBuffer(GL_PIXEL_PACK_BUFFER, pbo);
#if SUPPORT_OPENGL_ES
//    glReadPixels(0, 0, width, height, GL_RGBA, GL_UNSIGNED_BYTE, 0);
    glBindBuffer(GL_TEXTURE_2D, sourceId);

    // Send PBO to main memory
    GLubyte* pboPtr = static_cast<GLubyte*>(glMapBufferRange(
        GL_PIXEL_PACK_BUFFER, 0, bufferSize, GL_MAP_READ_BIT));
#elif SUPPORT_OPENGL_CORE
    glBindTexture(GL_TEXTURE_2D, sourceId);
    glGetTexImage(GL_TEXTURE_2D, 0, GL_BGRA, GL_UNSIGNED_BYTE, nullptr);

    // Send PBO to main memory
    GLubyte* pboPtr = static_cast<GLubyte*>(glMapBuffer(
        GL_PIXEL_PACK_BUFFER, GL_READ_ONLY));
#endif
    if (pboPtr != nullptr)
    {
        memcpy(data, pboPtr, bufferSize);
        glUnmapBuffer(GL_PIXEL_PACK_BUFFER);
    }
    glBindBuffer(GL_PIXEL_PACK_BUFFER, 0);

    rtc::scoped_refptr<webrtc::I420Buffer> i420_buffer =
            GraphicsUtility::ConvertRGBToI420Buffer(
                    width, height, pitch, static_cast<uint8_t*>(data));
    return i420_buffer;
}

} // end namespace webrtc
} // end namespace unity