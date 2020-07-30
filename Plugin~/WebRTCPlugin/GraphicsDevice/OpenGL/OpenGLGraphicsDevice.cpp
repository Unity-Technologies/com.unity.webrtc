#include "pch.h"
#include "OpenGLGraphicsDevice.h"
#include "OpenGLTexture2D.h"

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
bool OpenGLGraphicsDevice::InitV() {
#if _DEBUG
    GLuint unusedIds = 0;
    glEnable(GL_DEBUG_OUTPUT);
    glEnable(GL_DEBUG_OUTPUT_SYNCHRONOUS);
    glDebugMessageCallback(OnOpenGLDebugMessage, nullptr);
    glDebugMessageControl(GL_DONT_CARE, GL_DONT_CARE, GL_DONT_CARE, 0, &unusedIds, true);
#endif
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
    return nullptr;
}


//---------------------------------------------------------------------------------------------------------------------
bool OpenGLGraphicsDevice::CopyResourceV(ITexture2D* dest, ITexture2D* src) {
    auto width = dest->GetWidth();
    auto height  = dest->GetHeight();
    GLuint dstName = reinterpret_cast<intptr_t>(dest->GetNativeTexturePtrV());
    GLuint srcName = reinterpret_cast<intptr_t>(src->GetNativeTexturePtrV());
    return CopyResource(dstName, srcName, width, height);
}

//---------------------------------------------------------------------------------------------------------------------
bool OpenGLGraphicsDevice::CopyResourceFromNativeV(ITexture2D* dest, void* nativeTexturePtr) {
    auto width = dest->GetWidth();
    auto height  = dest->GetHeight();
    GLuint dstName = reinterpret_cast<intptr_t>(dest->GetNativeTexturePtrV());
    GLuint srcName = reinterpret_cast<intptr_t>(nativeTexturePtr);
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

rtc::scoped_refptr<webrtc::I420Buffer> OpenGLGraphicsDevice::ConvertRGBToI420(ITexture2D* tex)
{
    assert(false && "ConvertRGBToI420 need to implement on OpenGL");
    return nullptr;
}

} // end namespace webrtc
} // end namespace unity