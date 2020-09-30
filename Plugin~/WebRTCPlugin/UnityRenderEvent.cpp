#include "pch.h"

#include <IUnityGraphics.h>
#include <IUnityProfiler.h>

#include "Codec/EncoderFactory.h"
#include "Context.h"
#include "GraphicsDevice/GraphicsDevice.h"

enum class VideoStreamRenderEventID
{
    Initialize = 0,
    Encode = 1,
    Finalize = 2
};

namespace unity
{
namespace webrtc
{
    IUnityInterfaces* s_UnityInterfaces = nullptr;
    IUnityGraphics* s_Graphics = nullptr;
    IUnityProfiler* s_UnityProfiler = nullptr;
    Context* s_context = nullptr;
    IGraphicsDevice* s_device;
    std::map<const ::webrtc::MediaStreamTrackInterface*, std::unique_ptr<IEncoder>> s_mapEncoder;

    const UnityProfilerMarkerDesc* s_MarkerEncode = nullptr;
    bool s_IsDevelopmentBuild = false;
} // end namespace webrtc
} // end namespace unity

using namespace unity::webrtc;

static void UNITY_INTERFACE_API OnGraphicsDeviceEvent(UnityGfxDeviceEventType eventType)
{
    switch (eventType)
    {
    case kUnityGfxDeviceEventInitialize:
    {
        s_mapEncoder.clear();
        break;
    }
    case kUnityGfxDeviceEventShutdown:
    {
        //UnityPluginUnload not called normally
        s_Graphics->UnregisterDeviceEventCallback(OnGraphicsDeviceEvent);
        s_mapEncoder.clear();
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

    s_UnityProfiler = unityInterfaces->Get<IUnityProfiler>();
    if (s_UnityProfiler != nullptr)
    {
        s_IsDevelopmentBuild = s_UnityProfiler->IsAvailable() != 0;
        s_UnityProfiler->CreateMarker(
            &s_MarkerEncode, "Encode", kUnityProfilerCategoryRender,
            kUnityProfilerMarkerFlagDefault, 0);
    }

    OnGraphicsDeviceEvent(kUnityGfxDeviceEventInitialize);
}
extern "C" void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API UnityPluginUnload()
{
    s_Graphics->UnregisterDeviceEventCallback(OnGraphicsDeviceEvent);
}

static void UNITY_INTERFACE_API OnRenderEvent(int eventID, void* data)
{
    if (s_context == nullptr)
        return;
    if (!ContextManager::GetInstance()->Exists(s_context))
        return;
    std::unique_lock<std::mutex> lock(s_context->mutex, std::try_to_lock);
    if(!lock.owns_lock())
        return;

    MediaStreamTrackInterface* track =
        static_cast<MediaStreamTrackInterface*>(data);
    const VideoStreamRenderEventID event =
        static_cast<VideoStreamRenderEventID>(eventID);

    switch(event)
    {
        case VideoStreamRenderEventID::Initialize:
        {
            if (!GraphicsDevice::GetInstance().IsInitialized())
            {
                GraphicsDevice::GetInstance().Init(s_UnityInterfaces);
            }
            s_device = GraphicsDevice::GetInstance().GetDevice();
            const VideoEncoderParameter* param = s_context->GetEncoderParameter(track);
            const UnityEncoderType encoderType = s_context->GetEncoderType();
            s_mapEncoder[track] = EncoderFactory::GetInstance().Init(
                param->width, param->height, s_device, encoderType);
            if (!s_context->InitializeEncoder(s_mapEncoder[track].get(), track))
            {
                LogPrint("Encoder initialization faild.");
            }
            return;
        }
        case VideoStreamRenderEventID::Encode:
        {
            if (s_IsDevelopmentBuild)
                s_UnityProfiler->BeginSample(s_MarkerEncode);
            if(!s_context->EncodeFrame(track))
            {
                LogPrint("Encode frame failed");
            }
            if (s_IsDevelopmentBuild)
                s_UnityProfiler->EndSample(s_MarkerEncode);

            return;
        }
        case VideoStreamRenderEventID::Finalize:
        {
            s_context->FinalizeEncoder(s_mapEncoder[track].get());
            s_mapEncoder.erase(track);
            if(s_mapEncoder.empty())
            {
                GraphicsDevice::GetInstance().Shutdown();
            }
            return;
        }
        default: {
            LogPrint("Unknown event id %d", eventID);
            return;
        }
    }
}

extern "C" UnityRenderingEventAndData UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API GetRenderEventFunc(Context* context)
{
    s_context = context;
    return OnRenderEvent;
}
