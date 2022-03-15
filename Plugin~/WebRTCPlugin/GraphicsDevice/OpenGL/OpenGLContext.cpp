#include "pch.h"

#include "OpenGLContext.h"

#if SUPPORT_OPENGL_ES
#include <EGL/egl.h>
#elif SUPPORT_OPENGL_CORE && UNITY_LINUX
#include <GL/glx.h>
#endif

namespace unity
{
namespace webrtc
{
#if SUPPORT_OPENGL_ES
    class EGLContextImpl : public OpenGLContext
    {
    public:
        EGLContextImpl(EGLDisplay display, EGLContext sharedCtx)
        {
            int count = 0;
            if(!eglGetConfigs(display, 0, 0, &count))
            {
                RTC_LOG(LS_ERROR) << "eglGetConfigs failed";
                throw;
            }
            std::vector<EGLConfig> configs(count);
            eglGetConfigs(display, configs.data(), count, &count);
            EGLint contextAttr[] =
            {
                EGL_NONE
            };
            context_ = eglCreateContext(display_, configs[0], sharedCtx, contextAttr);
            RTC_DCHECK(context_);

            EGLint surfaceAttr[] =
            {
                EGL_WIDTH, 1,
                EGL_HEIGHT, 1,
                EGL_NONE
            };
            surface_ = eglCreatePbufferSurface(display_, configs[0], surfaceAttr);
            RTC_DCHECK(surface_);

            if(!eglMakeCurrent(display_, surface_, surface_, context_))
            {
                RTC_LOG(LS_ERROR) << "eglMakeCurrent failed";
                throw;
            }
        }
        ~EGLContextImpl()
        {
            eglDestroySurface(display_, surface_);
            eglDestroyContext(display_, context_);
        }
    private:
        EGLContext  context_;
        EGLDisplay display_;
        EGLSurface surface_;
    };

    // The context is created by Unity, and it related on the render thread.
    EGLContext g_context;
    EGLDisplay g_display;

#elif SUPPORT_OPENGL_CORE && UNITY_LINUX
    class GLXContextImpl : public OpenGLContext
    {
    public:
        GLXContextImpl(Display* display, GLXContext sharedCtx)
        : display_(display)
        {
            static int dblBuf[]  =
            {
                GLX_RGBA,
                GLX_DEPTH_SIZE, 16,
                GLX_DOUBLEBUFFER,
                None
            };
            XVisualInfo *vi = glXChooseVisual(display_, DefaultScreen(display_), dblBuf);
            context_ = glXCreateContext(display_, vi, sharedCtx, GL_TRUE);
            RTC_DCHECK(context_);

            const int visualAttrs[] = { None };
            const int attrs[] = {
                    GLX_PBUFFER_WIDTH, 1,
                    GLX_PBUFFER_HEIGHT, 1,
                    None
            };

            int returnedElements;
            GLXFBConfig* configs = glXChooseFBConfig(display_, 0, visualAttrs, &returnedElements);
            pbuffer_ = glXCreatePbuffer(display_, configs[0], attrs);
            RTC_DCHECK(pbuffer_);
            if(!glXMakeContextCurrent(display_, pbuffer_, pbuffer_, context_))
            {
                RTC_LOG(LS_ERROR) << "glXMakeContextCurrent failed";
                throw;
            }
        }
        ~GLXContextImpl()
        {
            glXDestroyPbuffer(display_, pbuffer_);
            glXDestroyContext(display_, context_);
        }
    private:
        GLXPbuffer pbuffer_;
        GLXContext context_;
        Display* display_;
    };

    // The context is created by Unity, and it related on the render thread.
    GLXContext g_context;
    Display* g_display = nullptr;
#endif

    void OpenGLContext::InitGLContext()
    {
#if SUPPORT_OPENGL_ES
        g_context = eglGetCurrentContext();
        RTC_DCHECK(g_context);
        g_display = eglGetCurrentDisplay();
        RTC_DCHECK(g_display);
#elif SUPPORT_OPENGL_CORE && UNITY_LINUX
        g_context = glXGetCurrentContext();
        RTC_DCHECK(g_context);
        g_display = glXGetCurrentDisplay();
        RTC_DCHECK(g_display);
#endif
    }

    bool OpenGLContext::CurrentContext()
    {
#if SUPPORT_OPENGL_ES
        return eglGetCurrentContext() != EGL_NO_CONTEXT;
#elif SUPPORT_OPENGL_CORE && UNITY_LINUX
        return glXGetCurrentContext() != NULL;
#endif
    }

    std::unique_ptr<OpenGLContext> OpenGLContext::CreateGLContext()
    {
#if SUPPORT_OPENGL_ES
        EGLContext context = eglGetCurrentContext();
        if(context)
            throw;
        return std::make_unique<EGLContextImpl>(g_display, g_context);
#elif SUPPORT_OPENGL_CORE && UNITY_LINUX
        GLXContext context = glXGetCurrentContext();
        if(context)
            throw;
        return std::make_unique<GLXContextImpl>(g_display, g_context);
#endif
    }
}
}