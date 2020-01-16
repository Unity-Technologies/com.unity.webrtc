#include "pch.h"
#include "Context.h"
#include "IUnityGraphics.h"
#include "GraphicsDevice/GraphicsDevice.h"

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
    Context* s_context = nullptr;
    IGraphicsDevice* s_device;
}
using namespace WebRTC;

static void UNITY_INTERFACE_API OnGraphicsDeviceEvent(UnityGfxDeviceEventType eventType)
{
    switch (eventType)
    {
    case kUnityGfxDeviceEventInitialize:
    {
        break;
    }
    case kUnityGfxDeviceEventShutdown:
    {
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
    switch(static_cast<VideoStreamRenderEventID>(eventID))
    {
        case VideoStreamRenderEventID::Initialize:
            if(!GraphicsDevice::GetInstance().IsInitialized()) {
                GraphicsDevice::GetInstance().Init(s_UnityInterfaces);
            }
            s_device = GraphicsDevice::GetInstance().GetDevice();
            s_context->InitializeEncoder(s_device);
            return;
        case VideoStreamRenderEventID::Encode:
            s_context->EncodeFrame();
            return;
        case VideoStreamRenderEventID::Finalize:
            s_context->FinalizeEncoder();
            GraphicsDevice::GetInstance().Shutdown();
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
