#include "pch.h"
#include "OpenGLGraphicsDevice.h"
#include "OpenGLTexture2D.h"

namespace WebRTC {

OpenGLGraphicsDevice::OpenGLGraphicsDevice()
{
}

//---------------------------------------------------------------------------------------------------------------------
OpenGLGraphicsDevice::~OpenGLGraphicsDevice() {

}

//---------------------------------------------------------------------------------------------------------------------
bool OpenGLGraphicsDevice::InitV() {
    GLenum err = glewInit();
    if (GLEW_OK != err)
    {
        return false;
    }
#if _DEBUG
    GLuint unusedIds = 0;
    glEnable(GL_DEBUG_OUTPUT);
    glEnable(GL_DEBUG_OUTPUT_SYNCHRONOUS);
    glDebugMessageCallback(OnOpenGLDebugMessage, nullptr);
    glDebugMessageControl(GL_DONT_CARE, GL_DONT_CARE, GL_DONT_CARE, 0, &unusedIds, true);
#endif
    if (err != GLEW_OK)
    {
        LogPrint("OpenGL initialize failed");
        return false;
    }
    return true;
}

//---------------------------------------------------------------------------------------------------------------------

void OpenGLGraphicsDevice::ShutdownV() {

}

//---------------------------------------------------------------------------------------------------------------------
ITexture2D* OpenGLGraphicsDevice::CreateDefaultTextureV(uint32_t w, uint32_t h) {

        GLuint tex;
        glGenTextures(1, &tex);
        glBindTexture(GL_TEXTURE_2D, tex);
        glTexStorage2D(GL_TEXTURE_2D, 1, GL_RGBA8, w, h);
        glBindTexture(GL_TEXTURE_2D, 0);
        return new OpenGLTexture2D(w, h, &tex);
}

//---------------------------------------------------------------------------------------------------------------------
ITexture2D* OpenGLGraphicsDevice::CreateCPUReadTextureV(uint32_t w, uint32_t h) {
    assert(false && "CreateCPUReadTextureV need to implement on OpenGL");
    return NULL;
}

//---------------------------------------------------------------------------------------------------------------------
ITexture2D* OpenGLGraphicsDevice::CreateDefaultTextureFromNativeV(uint32_t w, uint32_t h, void* nativeTexturePtr) {
    assert(nullptr!=nativeTexturePtr);
    auto texPtr = reinterpret_cast<GLuint*>(nativeTexturePtr);
    return new OpenGLTexture2D(w, h, texPtr);
}


//---------------------------------------------------------------------------------------------------------------------
bool OpenGLGraphicsDevice::CopyResourceV(ITexture2D* dest, ITexture2D* src) {
    auto nativeDest = reinterpret_cast<GLuint*>(dest->GetNativeTexturePtrV());
    auto nativeSrc = reinterpret_cast<GLuint*>(src->GetNativeTexturePtrV());
    auto width = dest->GetWidth();
    auto height  = dest->GetHeight();

    GLuint srcName = *nativeSrc;
    GLuint dstName = *nativeDest;
    return CopyResource(dstName, srcName, width, height);

}

//---------------------------------------------------------------------------------------------------------------------
bool OpenGLGraphicsDevice::CopyResourceFromNativeV(ITexture2D* dest, void* nativeTexturePtr) {
    auto nativeDest = reinterpret_cast<GLuint*>(dest->GetNativeTexturePtrV());
    auto width = dest->GetWidth();
    auto height  = dest->GetHeight();

    GLuint srcName = (GLuint)(size_t)(nativeTexturePtr);
    GLuint dstName = *nativeDest;
    return CopyResource(dstName, srcName, width, height);
}

bool OpenGLGraphicsDevice::CopyResource(GLuint dstName, GLuint srcName, uint32 width, uint32 height) {
    if(srcName == dstName)
    {
        LogPrint("Same texture");
        return false;
    }
    if(glIsTexture(srcName) == GL_FALSE)
    {
        LogPrint("srcName is not texture");
        return false;
    }
    if(glIsTexture(dstName) == GL_FALSE)
    {
        LogPrint("dstName is not texture");
        return false;
    }
    glCopyImageSubData(
            srcName, GL_TEXTURE_2D, 0, 0, 0, 0,
            dstName, GL_TEXTURE_2D, 0, 0, 0, 0,
            width, height, 1);
    return true;
}

rtc::scoped_refptr<webrtc::I420Buffer> D3D12GraphicsDevice::ConvertRGBToI420(ITexture2D* tex)
{
    assert(false && "ConvertRGBToI420 need to implement on OpenGL");
    return NULL;
}

} //end namespace
