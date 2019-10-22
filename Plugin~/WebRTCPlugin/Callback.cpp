#include "pch.h"
#include "Context.h"
#include "IUnityGraphics.h"

#ifdef _WIN32
#include "IUnityGraphicsD3D11.h"
#endif

#include "PlatformBase.h"

#include <assert.h>
#if UNITY_IPHONE
#	include <OpenGLES/ES2/gl.h>
#elif UNITY_ANDROID || UNITY_WEBGL
#	include <GLES2/gl2.h>
#elif UNITY_OSX
#	include <OpenGL/gl3.h>
#elif UNITY_WIN
// On Windows, use gl3w to initialize and load OpenGL Core functions. In principle any other
// library (like GLEW, GLFW etc.) can be used; here we use gl3w since it's simple and
// straightforward.
#include "gl3w.h"
#elif UNITY_LINUX
#	define GL_GLEXT_PROTOTYPES
#	include <GL/gl.h>
#else
#	error Unknown platform
#endif

namespace WebRTC
{
    IUnityInterfaces* s_UnityInterfaces = nullptr;
    IUnityGraphics* s_Graphics = nullptr;
    UnityGfxRenderer s_RenderType;

#if SUPPORT_D3D11
    //d3d11 context
    ID3D11DeviceContext* context;
    //d3d11 device
    ID3D11Device* g_D3D11Device = nullptr;
#endif // if SUPPORT_D3D11

    //natively created ID3D11Texture2D ptrs
    UnityFrameBuffer* renderTextures[bufferedFrameNum];

    Context* s_context;
}
using namespace WebRTC;
//get d3d11 device
static void UNITY_INTERFACE_API OnGraphicsDeviceEvent(UnityGfxDeviceEventType eventType)
{
    switch (eventType)
    {
    case kUnityGfxDeviceEventInitialize:
    {
        s_RenderType = s_UnityInterfaces->Get<IUnityGraphics>()->GetRenderer();
        if (s_RenderType == kUnityGfxRendererOpenGLES20)
        {
//            m_VertexShader = CreateShader(GL_VERTEX_SHADER, kGlesVProgTextGLES2);
//            m_FragmentShader = CreateShader(GL_FRAGMENT_SHADER, kGlesFShaderTextGLES2);
        }
        else if (s_RenderType == kUnityGfxRendererOpenGLES30)
        {
//            m_VertexShader = CreateShader(GL_VERTEX_SHADER, kGlesVProgTextGLES3);
//            m_FragmentShader = CreateShader(GL_FRAGMENT_SHADER, kGlesFShaderTextGLES3);
        }
#if SUPPORT_OPENGL_CORE
        else if (s_RenderType == kUnityGfxRendererOpenGLCore)
        {
#if UNITY_WIN
            gl3wInit();
#endif
//            m_VertexShader = CreateShader(GL_VERTEX_SHADER, kGlesVProgTextGLCore);
//            m_FragmentShader = CreateShader(GL_FRAGMENT_SHADER, kGlesFShaderTextGLCore);
        }
#endif // if SUPPORT_OPENGL_CORE
#if SUPPORT_D3D11
        else if (s_RenderType == kUnityGfxRendererD3D11)
        {
            g_D3D11Device = s_UnityInterfaces->Get<IUnityGraphicsD3D11>()->GetDevice();
            g_D3D11Device->GetImmediateContext(&context);
        }
#endif // if SUPPORT_D3D11

        break;
    }
    case kUnityGfxDeviceEventShutdown:
    {
        for (auto rt : renderTextures)
        {
            if (rt)
            {
                rt->Release();
                rt = nullptr;
            }
        }
        //UnityPluginUnload not called normally
        s_Graphics->UnregisterDeviceEventCallback(OnGraphicsDeviceEvent);
        break;
    }
    case kUnityGfxDeviceEventBeforeReset:
    {
        break;
    }
    case kUnityGfxDeviceEventAfterReset:
    {
        break;
    }
    };
}
// Unity plugin load event
extern "C" void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API UnityPluginLoad(IUnityInterfaces* unityInterfaces)
{
    s_UnityInterfaces = unityInterfaces;
    s_Graphics = unityInterfaces->Get<IUnityGraphics>();
    s_Graphics->RegisterDeviceEventCallback(OnGraphicsDeviceEvent);
    OnGraphicsDeviceEvent(kUnityGfxDeviceEventInitialize);
}
extern "C" void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API UnityPluginUnload()
{
    s_Graphics->UnregisterDeviceEventCallback(OnGraphicsDeviceEvent);
}

static void UNITY_INTERFACE_API OnRenderEvent(int eventID)
{
    if (s_context != nullptr)
    {
        s_context->EncodeFrame();
    }
}

extern "C" UnityRenderingEvent UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API GetRenderEventFunc(Context* context)
{
    s_context = context;
    return OnRenderEvent;
}
