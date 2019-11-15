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
void OpenGLGraphicsDevice::InitV() {
    GLenum err = glewInit();
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
    }
}

//---------------------------------------------------------------------------------------------------------------------

void OpenGLGraphicsDevice::ShutdownV() {

}

//---------------------------------------------------------------------------------------------------------------------
ITexture2D* OpenGLGraphicsDevice::CreateDefaultTextureV(uint32_t w, uint32_t h) {

        GLuint texture;
        glGenTextures(1, &texture);
        glBindTexture(GL_TEXTURE_2D, tex);
        glTexStorage2D(GL_TEXTURE_2D, 1, GL_RGBA8, width, height);
        glBindTexture(GL_TEXTURE_2D, 0);
        pitch = GetWidthInBytes(format, width);
        glBindTexture(GL_TEXTURE_2D, 0);
        return new OpenGLTexture2D(w,h,texture);
}

//---------------------------------------------------------------------------------------------------------------------
ITexture2D* OpenGLGraphicsDevice::CreateDefaultTextureFromNativeV(uint32_t w, uint32_t h, void* nativeTexturePtr) {
    assert(nullptr!=nativeTexturePtr);
    auto texPtr = reinterpret_cast<GLuint*>(nativeTexturePtr);
    return new OpenGLTexture2D(w, h, texPtr);
}


//---------------------------------------------------------------------------------------------------------------------
void OpenGLGraphicsDevice::CopyResourceV(ITexture2D* dest, ITexture2D* src) {
    auto nativeDest = reinterpret_cast<GLuint*>(dest->GetNativeTexturePtrV());
    auto nativeSrc = reinterpret_cast<GLuint*>(src->GetNativeTexturePtrV());

    GLuint srcName = *nativeSrc;
    GLuint dstName = *nativeDest;

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
}

//---------------------------------------------------------------------------------------------------------------------
void OpenGLGraphicsDevice::CopyResourceFromNativeV(ITexture2D* dest, void* nativeTexturePtr) {
    if (dest->GetNativeTexturePtrV() == nativeTexturePtr)
        return;

    //[Note-sin: 2019-10-30] Do we need to implement this for RenderStreaming ?
    DebugWarning("OpenGLGraphicsDevice: CopyResourceFromNativeV() is not supported");

}

} //end namespace
