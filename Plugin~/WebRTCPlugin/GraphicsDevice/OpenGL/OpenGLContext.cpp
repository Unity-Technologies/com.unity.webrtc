#include "pch.h"

#include "OpenGLContext.h"

#if SUPPORT_OPENGL_ES
#include <EGL/egl.h>
#endif
#if SUPPORT_OPENGL_CORE && UNITY_LINUX
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
        EGLContextImpl(EGLDisplay display, EGLContext sharedCtx = 0)
        : display_(display)
        , created_(false)
        {
            RTC_DCHECK(display_);
            int count = 0;
            if(!eglGetConfigs(display, 0, 0, &count))
            {
                RTC_LOG(LS_ERROR) << "eglGetConfigs failed:" << eglGetError();
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
                RTC_LOG(LS_ERROR) << "eglMakeCurrent failed:" << eglGetError();
                throw;
            }
            created_ = true;
        }
        ~EGLContextImpl()
        {
            if(created_)
            {
                eglDestroySurface(display_, surface_);
                eglDestroyContext(display_, context_);
            }
        }
    private:
        EGLContext  context_;
        EGLDisplay display_;
        EGLSurface surface_;
        // In Unity, this flag is false because the context already created which rely on the render thread.
        bool created_;
    };

    // The context is created by Unity, and it related on the render thread.
    //EGLContext g_context;
    //EGLDisplay g_display;
#endif

#if SUPPORT_OPENGL_CORE && UNITY_LINUX
    class GLXContextImpl : public OpenGLContext
    {
    public:
        GLXContextImpl(GLXContext sharedCtx = 0)
        : created_(false)
        {
            RTC_DCHECK(display);

            // Return if the context is already created.
            context_ = glXGetCurrentContext();
            if(context_ != nullptr)
                return;

            static int dblBuf[]  =
            {
                GLX_RGBA,
                GLX_DEPTH_SIZE, 16,
                GLX_DOUBLEBUFFER,
                None
            };
            XVisualInfo *vi = glXChooseVisual(display, DefaultScreen(display), dblBuf);
            context_ = glXCreateContext(display, vi, sharedCtx, GL_TRUE);
            RTC_DCHECK(context_);

            const int visualAttrs[] = { None };
            const int attrs[] = {
                    GLX_PBUFFER_WIDTH, 1,
                    GLX_PBUFFER_HEIGHT, 1,
                    None
            };

            int returnedElements;
            GLXFBConfig* configs = glXChooseFBConfig(display, 0, visualAttrs, &returnedElements);
            pbuffer_ = glXCreatePbuffer(display, configs[0], attrs);
            RTC_DCHECK(pbuffer_);
            if(!glXMakeContextCurrent(display, pbuffer_, pbuffer_, context_))
            {
                RTC_LOG(LS_ERROR) << "glXMakeContextCurrent failed";
                throw;
            }
            created_ = true;
        }
        ~GLXContextImpl()
        {
            glXDestroyPbuffer(display, pbuffer_);
            glXDestroyContext(display, context_);
        }

        GLXContext context() const { return context_; }

        static Display* display;
    private:
        GLXPbuffer pbuffer_;
        GLXContext context_;
        // In Unity, this flag is false because the context already created which rely on the render thread.
        bool created_;
    };
    Display* GLXContextImpl::display;

#endif

    void OpenGLContext::Init()
    {
#if SUPPORT_OPENGL_ES
        g_display = eglGetCurrentDisplay();
        RTC_DCHECK(g_display);
#elif SUPPORT_OPENGL_CORE && UNITY_LINUX
        GLXContextImpl::display = glXGetCurrentDisplay();
        RTC_DCHECK(GLXContextImpl::display);
#endif
    }

    std::unique_ptr<OpenGLContext> OpenGLContext::CurrentContext()
    {
#if SUPPORT_OPENGL_ES
        if(eglGetCurrentContext())
        return CreateGLContext();
    return nullptr;
#endif
#if SUPPORT_OPENGL_CORE && UNITY_LINUX
        if(glXGetCurrentContext())
            return CreateGLContext();
        return nullptr;
#endif
    }

    std::unique_ptr<OpenGLContext> OpenGLContext::CreateGLContext(const OpenGLContext* shared)
    {
#if SUPPORT_OPENGL_ES
        return std::make_unique<EGLContextImpl>(shared);
#endif
#if SUPPORT_OPENGL_CORE && UNITY_LINUX
        const GLXContextImpl* context = static_cast<const GLXContextImpl*>(shared);
        GLXContext glxContext = context ? context->context() : nullptr;
        return std::make_unique<GLXContextImpl>(glxContext);
#endif
    }

}
}