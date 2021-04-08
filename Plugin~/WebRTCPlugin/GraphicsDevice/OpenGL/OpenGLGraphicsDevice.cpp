#include "pch.h"
#include "third_party/libyuv/include/libyuv.h"

#include "OpenGLGraphicsDevice.h"
#include "OpenGLTexture2D.h"
#include "GraphicsDevice/GraphicsUtility.h"

namespace unity
{
namespace webrtc
{

#if SUPPORT_OPENGL_ES
GLuint fbo[2];
#endif

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

#if SUPPORT_OPENGL_ES
    glGenFramebuffers(2, fbo);
#endif

#if CUDA_PLATFORM
    m_isCudaSupport = CUDA_SUCCESS == m_cudaContext.InitGL();
#endif
    return true;
}

//---------------------------------------------------------------------------------------------------------------------

void OpenGLGraphicsDevice::ShutdownV() {

#if SUPPORT_OPENGL_ES
    glDeleteFramebuffers(2, fbo);
#endif

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

    // todo(kazuki): "glCopyImageSubData" is available since OpenGL ES 3.2 on Android platform.
    // OpenGL ES 3.2 is needed to use API level 24.
//#if SUPPORT_OPENGL_CORE
    glCopyImageSubData(
        srcName, GL_TEXTURE_2D, 0, 0, 0, 0,
        dstName, GL_TEXTURE_2D, 0, 0, 0, 0,
        width, height, 1);
// #elif SUPPORT_OPENGL_ES
//     glBindTexture(GL_TEXTURE_2D, srcName);
//     glBindFramebuffer(GL_DRAW_FRAMEBUFFER, fbo[1]);
//     glFramebufferTexture2D(GL_DRAW_FRAMEBUFFER,
//         GL_COLOR_ATTACHMENT0, GL_TEXTURE_2D, srcName, 0);

//     glBindTexture(GL_TEXTURE_2D, dstName);
//     glCopyTexSubImage2D(GL_TEXTURE_2D, 0, 0, 0, 0, 0, width, height);
//     glBindFramebuffer(GL_DRAW_FRAMEBUFFER, 0);
//     glBindTexture(GL_TEXTURE_2D, 0);
// #endif
    return true;
}

void GetTexImage(GLenum target, GLint level, GLenum format, GLenum type, void *pixels)
{
#if SUPPORT_OPENGL_CORE
    glGetTexImage(target, level, format, type, pixels);
#elif SUPPORT_OPENGL_ES
    glBindFramebuffer( GL_FRAMEBUFFER, fbo[0] );

    int width = 0;
    int height = 0;
    glGetTexLevelParameteriv(GL_TEXTURE_2D, 0, GL_TEXTURE_WIDTH, &width);
    glGetTexLevelParameteriv(GL_TEXTURE_2D, 0, GL_TEXTURE_HEIGHT, &height);

    GLint tex;
    glGetIntegerv(GL_TEXTURE_BINDING_2D, &tex);
    
    glFramebufferTexture2D(GL_FRAMEBUFFER,
        GL_COLOR_ATTACHMENT0, GL_TEXTURE_2D, tex, 0);

    // read pixels from framebuffer to PBO
    glReadBuffer(GL_COLOR_ATTACHMENT0);
    glReadPixels(0, 0, width, height, format, type, pixels);
    glBindFramebuffer(GL_FRAMEBUFFER, 0);
#endif
}

rtc::scoped_refptr<webrtc::I420Buffer> OpenGLGraphicsDevice::ConvertRGBToI420(ITexture2D* tex)
{
    OpenGLTexture2D* sourceTex = static_cast<OpenGLTexture2D*>(tex);
    const GLuint sourceId = reinterpret_cast<uintptr_t>(sourceTex->GetNativeTexturePtrV());
    const GLuint pbo = sourceTex->GetPBO();
    const GLenum format = GL_RGBA;
    const uint32_t width = sourceTex->GetWidth();
    const uint32_t height = sourceTex->GetHeight();
    const uint32_t bufferSize = sourceTex->GetBufferSize();
    byte* data = sourceTex->GetBuffer();

    glBindBuffer(GL_PIXEL_PACK_BUFFER, pbo);
    glBindTexture(GL_TEXTURE_2D, sourceId);

    GetTexImage(GL_TEXTURE_2D, 0, format, GL_UNSIGNED_BYTE, nullptr);

    // Send PBO to main memory
    GLubyte* pboPtr = static_cast<GLubyte*>(glMapBufferRange(
        GL_PIXEL_PACK_BUFFER, 0, bufferSize, GL_MAP_READ_BIT));
    if (pboPtr != nullptr)
    {
        memcpy(data, pboPtr, bufferSize);
        glUnmapBuffer(GL_PIXEL_PACK_BUFFER);
    }
    glBindTexture(GL_TEXTURE_2D, 0);
    glBindBuffer(GL_PIXEL_PACK_BUFFER, 0);

    // RGBA -> I420
    rtc::scoped_refptr<webrtc::I420Buffer> i420_buffer = webrtc::I420Buffer::Create(width, height);
    libyuv::ABGRToI420(
        static_cast<uint8_t*>(data),
        width * 4,
        i420_buffer->MutableDataY(),
        width,
        i420_buffer->MutableDataU(),
        (width+1)/2,
        i420_buffer->MutableDataV(),
        (width+1)/2,
        width,
        height
    );
    return i420_buffer;
}

} // end namespace webrtc
} // end namespace unity