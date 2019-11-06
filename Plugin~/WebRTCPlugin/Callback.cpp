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
// TODO::
// #include "gl3w.h"
#elif UNITY_LINUX
#	define GL_GLEXT_PROTOTYPES
#include <GL/glew.h>
#else
#	error Unknown platform
#endif

enum class VideoStreamRenderEventID
{
    Initialize = 0,
    Encode = 1,
    Finalize = 2
};


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

// get d3d11 device.
// this function is not called on Linux.
static void UNITY_INTERFACE_API OnGraphicsDeviceEvent(UnityGfxDeviceEventType eventType)
{
    switch (eventType)
    {
    case kUnityGfxDeviceEventInitialize:
    {
#if SUPPORT_D3D11
        if (s_RenderType == kUnityGfxRendererD3D11)
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
#if _WIN32
                rt->Release();
#else
                delete rt;
#endif
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

static bool isInitializedGL = false;

static void UNITY_INTERFACE_API OnRenderEvent(int eventID) {
#if defined(SUPPORT_OPENGL_CORE) && !defined(_WIN32)
    if(!isInitializedGL)
    {
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
        isInitializedGL = true;
    }
#endif

    if (s_context == nullptr)
    {
        LogPrint("context is not initialized", eventID);
        return;
    }
    switch(static_cast<VideoStreamRenderEventID>(eventID))
    {
        case VideoStreamRenderEventID::Initialize:
            s_context->InitializeEncoder();
            return;
        case VideoStreamRenderEventID::Encode:
            s_context->EncodeFrame();
            return;
        case VideoStreamRenderEventID::Finalize:
            s_context->FinalizerEncoder();
            return;
        default:
            LogPrint("Unknown event id %d", eventID);
            return;
    }
}

extern "C" UnityRenderingEvent UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API GetRenderEventFunc(Context* context)
{
    s_context = context;
    return OnRenderEvent;
}
