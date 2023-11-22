#include "pch.h"

#if CUDA_PLATFORM
#include <cuda.h>
#include <cudaGL.h>
#endif

#include "third_party/libyuv/include/libyuv.h"

#include "GraphicsDevice/GraphicsUtility.h"
#include "OpenGLGraphicsDevice.h"
#include "OpenGLTexture2D.h"

#include "OpenGLContext.h"

#if CUDA_PLATFORM
#include "GraphicsDevice/Cuda/GpuMemoryBufferCudaHandle.h"
#else
#include "GpuMemoryBuffer.h"
#endif

namespace unity
{
namespace webrtc
{

#if SUPPORT_OPENGL_ES
    GLuint fbo[2];
#endif

    Size glTexSize(GLenum target, GLuint texture, GLint mipLevel)
    {
        int width = 0, height = 0;
        glBindTexture(target, texture);
        glGetTexLevelParameteriv(target, mipLevel, GL_TEXTURE_WIDTH, &width);
        glGetTexLevelParameteriv(target, mipLevel, GL_TEXTURE_HEIGHT, &height);
        glBindTexture(target, 0);
        return Size(width, height);
    }

    OpenGLGraphicsDevice::OpenGLGraphicsDevice(UnityGfxRenderer renderer, ProfilerMarkerFactory* profiler)
        : IGraphicsDevice(renderer, profiler)
        , mainContext_(nullptr)
    {
        OpenGLContext::Init();

        mainContext_ = OpenGLContext::CurrentContext();
        RTC_DCHECK(mainContext_);
    }

    OpenGLGraphicsDevice::~OpenGLGraphicsDevice() { }

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

    void OpenGLGraphicsDevice::ShutdownV()
    {

#if SUPPORT_OPENGL_ES
        glDeleteFramebuffers(2, fbo);
#endif

#if CUDA_PLATFORM
        m_cudaContext.Shutdown();
#endif
    }

    ITexture2D*
    OpenGLGraphicsDevice::CreateDefaultTextureV(uint32_t w, uint32_t h, UnityRenderingExtTextureFormat textureFormat)
    {
        GLuint tex;
        glGenTextures(1, &tex);
        glBindTexture(GL_TEXTURE_2D, tex);
        glTexStorage2D(GL_TEXTURE_2D, 1, GL_RGBA8, w, h);
        glBindTexture(GL_TEXTURE_2D, 0);

        OpenGLTexture2D::ReleaseOpenGLTextureCallback callback =
            std::bind(&OpenGLGraphicsDevice::ReleaseTexture, this, std::placeholders::_1);
        return new OpenGLTexture2D(w, h, tex, callback);
    }

    ITexture2D*
    OpenGLGraphicsDevice::CreateCPUReadTextureV(uint32_t w, uint32_t h, UnityRenderingExtTextureFormat textureFormat)
    {
        OpenGLTexture2D* tex = static_cast<OpenGLTexture2D*>(CreateDefaultTextureV(w, h, textureFormat));
        tex->CreatePBO();
        return tex;
    }

    bool OpenGLGraphicsDevice::CopyResourceV(ITexture2D* dst, ITexture2D* src)
    {
        OpenGLTexture2D* srcTexture = static_cast<OpenGLTexture2D*>(src);
        OpenGLTexture2D* dstTexture = static_cast<OpenGLTexture2D*>(dst);
        const GLuint srcName = srcTexture->GetTexture();
        return CopyResource(dstTexture, srcName);
    }

    bool OpenGLGraphicsDevice::CopyResourceFromNativeV(ITexture2D* dst, void* nativeTexturePtr)
    {
        OpenGLTexture2D* texture2D = static_cast<OpenGLTexture2D*>(dst);
        const GLuint srcName = reinterpret_cast<uintptr_t>(nativeTexturePtr);
        return CopyResource(texture2D, srcName);
    }

    bool OpenGLGraphicsDevice::CopyResource(OpenGLTexture2D* texture, GLuint srcName)
    {
        const GLuint dstName = texture->GetTexture();
        if (srcName == dstName)
        {
            RTC_LOG(LS_INFO) << "Same texture";
            return false;
        }
        if (glIsTexture(srcName) == GL_FALSE)
        {
            RTC_LOG(LS_INFO) << "srcName is not texture";
            return false;
        }
        if (glIsTexture(dstName) == GL_FALSE)
        {
            RTC_LOG(LS_INFO) << "dstName is not texture";
            return false;
        }

        Size srcSize = glTexSize(GL_TEXTURE_2D, srcName, 0);
        Size dstSize = glTexSize(GL_TEXTURE_2D, dstName, 0);

        if (srcSize.width() == 0 || srcSize.height() == 0)
        {
            RTC_LOG(LS_INFO) << "texture size is not valid";
            return false;
        }
        if (srcSize != dstSize)
        {
            RTC_LOG(LS_INFO) << "texture size is not same";
            return false;
        }

        // todo(kazuki): "glCopyImageSubData" is available since OpenGL ES 3.2 on Android platform.
        // OpenGL ES 3.2 is needed to use API level 24.
        glCopyImageSubData(
            srcName,
            GL_TEXTURE_2D,
            0,
            0,
            0,
            0,
            dstName,
            GL_TEXTURE_2D,
            0,
            0,
            0,
            0,
            dstSize.width(),
            dstSize.height(),
            1);

        // Create sync object.
        GLsync sync = glFenceSync(GL_SYNC_GPU_COMMANDS_COMPLETE, 0);
        GLenum error = glGetError();
        if (error != GL_NO_ERROR)
        {
            RTC_LOG(LS_INFO) << "glFenceSync returns error " << error;
            return false;
        }
        texture->SetSync(sync);
        return true;
    }

    void OpenGLGraphicsDevice::ReleaseTexture(OpenGLTexture2D* texture)
    {
        if (!OpenGLContext::CurrentContext())
            contexts_.push_back(OpenGLContext::CreateGLContext(mainContext_.get()));
        texture->Release();
    }

    void GetTexImage(GLenum target, GLint level, GLenum format, GLenum type, void* pixels)
    {
#if SUPPORT_OPENGL_CORE
        glGetTexImage(target, level, format, type, pixels);
#elif SUPPORT_OPENGL_ES
        glBindFramebuffer(GL_FRAMEBUFFER, fbo[0]);

        int width = 0;
        int height = 0;
        glGetTexLevelParameteriv(GL_TEXTURE_2D, 0, GL_TEXTURE_WIDTH, &width);
        glGetTexLevelParameteriv(GL_TEXTURE_2D, 0, GL_TEXTURE_HEIGHT, &height);

        GLint tex;
        glGetIntegerv(GL_TEXTURE_BINDING_2D, &tex);

        glFramebufferTexture2D(GL_FRAMEBUFFER, GL_COLOR_ATTACHMENT0, GL_TEXTURE_2D, tex, 0);

        // read pixels from framebuffer to PBO
        glReadBuffer(GL_COLOR_ATTACHMENT0);
        glReadPixels(0, 0, width, height, format, type, pixels);
        glBindFramebuffer(GL_FRAMEBUFFER, 0);
#endif
    }

    rtc::scoped_refptr<webrtc::I420Buffer> OpenGLGraphicsDevice::ConvertRGBToI420(ITexture2D* tex)
    {
        if (!OpenGLContext::CurrentContext())
            contexts_.push_back(OpenGLContext::CreateGLContext(mainContext_.get()));

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
        GLubyte* pboPtr = static_cast<GLubyte*>(glMapBufferRange(GL_PIXEL_PACK_BUFFER, 0, bufferSize, GL_MAP_READ_BIT));
        if (pboPtr)
        {
            std::memcpy(data, pboPtr, bufferSize);
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
            (width + 1) / 2,
            i420_buffer->MutableDataV(),
            (width + 1) / 2,
            width,
            height);
        return i420_buffer;
    }

    std::unique_ptr<GpuMemoryBufferHandle> OpenGLGraphicsDevice::Map(ITexture2D* texture)
    {
#if CUDA_PLATFORM
        if (!IsCudaSupport())
            return nullptr;

        if (!OpenGLContext::CurrentContext())
            contexts_.push_back(OpenGLContext::CreateGLContext(mainContext_.get()));

        OpenGLTexture2D* glTexture2D = static_cast<OpenGLTexture2D*>(texture);
        return GpuMemoryBufferCudaHandle::CreateHandle(GetCUcontext(), glTexture2D->GetTexture());
#else
        return nullptr;
#endif
    }

    bool OpenGLGraphicsDevice::WaitSync(const ITexture2D* texture)
    {
        if (!OpenGLContext::CurrentContext())
            contexts_.push_back(OpenGLContext::CreateGLContext(mainContext_.get()));

        const OpenGLTexture2D* glTexture2D = static_cast<const OpenGLTexture2D*>(texture);
        GLsync sync = glTexture2D->GetSync();
        if (sync == 0)
        {
            RTC_LOG(LS_INFO) << "The sync object is already reset.";
            return true;
        }
        GLenum ret = glClientWaitSync(sync, GL_SYNC_FLUSH_COMMANDS_BIT, m_syncTimeout.count());
        GLenum error = glGetError();
        if (error != GL_NO_ERROR)
        {
            RTC_LOG(LS_INFO) << "glClientWaitSync returns error " << error;
            return false;
        }

        switch (ret)
        {
        case GL_CONDITION_SATISFIED:
        case GL_ALREADY_SIGNALED:
            return true;
        }
        RTC_LOG(LS_INFO) << "glClientWaitSync returns " << ret;
        return false;
    }

    bool OpenGLGraphicsDevice::ResetSync(const ITexture2D* texture)
    {
        const OpenGLTexture2D* glTexture2D = static_cast<const OpenGLTexture2D*>(texture);
        GLsync sync = glTexture2D->GetSync();
        if (sync == 0)
        {
            RTC_LOG(LS_INFO) << "The sync object is already reset.";
            return true;
        }
        glDeleteSync(sync);
        GLenum error = glGetError();
        if (error != GL_NO_ERROR)
        {
            RTC_LOG(LS_INFO) << "glDeleteSync returns error " << error;
            return false;
        }
        return true;
    }

} // end namespace webrtc
} // end namespace unity
