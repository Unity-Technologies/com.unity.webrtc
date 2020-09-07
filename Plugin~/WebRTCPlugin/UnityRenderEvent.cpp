#include "pch.h"

#include <IUnityGraphics.h>
#include <IUnityProfiler.h>
#include <IUnityRenderingExtensions.h>

#include "Codec/EncoderFactory.h"
#include "Context.h"
#include "GraphicsDevice/GraphicsDevice.h"

#if defined(SUPPORT_VULKAN)
#include <IUnityGraphicsVulkan.h>
#include "GraphicsDevice/Vulkan/UnityVulkanInitCallback.h"
#endif

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
    std::map<const ::webrtc::MediaStreamTrackInterface*, std::unique_ptr<IEncoder>> s_mapEncoder;

    const UnityProfilerMarkerDesc* s_MarkerEncode = nullptr;
    bool s_IsDevelopmentBuild = false;

    static webrtc::VideoType ConvertTextureFormat(UnityRenderingExtTextureFormat type)
    {
        switch (type)
        {
        case UnityRenderingExtTextureFormat::kUnityRenderingExtFormatB8G8R8A8_SRGB:
        case UnityRenderingExtTextureFormat::kUnityRenderingExtFormatB8G8R8A8_UNorm:
        case UnityRenderingExtTextureFormat::kUnityRenderingExtFormatB8G8R8A8_SNorm:
        case UnityRenderingExtTextureFormat::kUnityRenderingExtFormatB8G8R8A8_UInt:
        case UnityRenderingExtTextureFormat::kUnityRenderingExtFormatB8G8R8A8_SInt:
            return webrtc::VideoType::kARGB;
        case UnityRenderingExtTextureFormat::kUnityRenderingExtFormatR8G8B8A8_SRGB:
        case UnityRenderingExtTextureFormat::kUnityRenderingExtFormatR8G8B8A8_UNorm:
        case UnityRenderingExtTextureFormat::kUnityRenderingExtFormatR8G8B8A8_SNorm:
        case UnityRenderingExtTextureFormat::kUnityRenderingExtFormatR8G8B8A8_UInt:
        case UnityRenderingExtTextureFormat::kUnityRenderingExtFormatR8G8B8A8_SInt:
        case UnityRenderingExtTextureFormat::kUnityRenderingExtFormatA8R8G8B8_SRGB:
        case UnityRenderingExtTextureFormat::kUnityRenderingExtFormatA8R8G8B8_UNorm:
            return webrtc::VideoType::kABGR;
        }

        // DebugLog("Unknown texture format:%d", type);
        return webrtc::VideoType::kUnknown;
    }

    IGraphicsDevice* GetGraphicsDevice()
    {
        if (!GraphicsDevice::GetInstance().IsInitialized())
        {
            GraphicsDevice::GetInstance().Init(s_UnityInterfaces);
        }
        return GraphicsDevice::GetInstance().GetDevice();
    }

    UnityGfxRenderer GetGraphicsRenderer()
    {
        return s_Graphics->GetRenderer();
    }

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

        IGraphicsDevice* device = GetGraphicsDevice();
        if (device != nullptr)
            device->InitV();
        break;
    }
    case kUnityGfxDeviceEventShutdown:
    {
        IGraphicsDevice* device = GetGraphicsDevice();
        if (device != nullptr)
            device->ShutdownV();
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

#if defined(SUPPORT_VULKAN)
    IUnityGraphicsVulkan* vulkan = unityInterfaces->Get<IUnityGraphicsVulkan>();
    if(vulkan != nullptr)
    {
        vulkan->InterceptInitialization(InterceptVulkanInitialization, nullptr);
    }
#endif

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

// Notice: When DebugLog is used in a method called from RenderingThread, 
// it hangs when attempting to leave PlayMode and re-enter PlayMode.
// So, we comment out `DebugLog`.

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
            IGraphicsDevice* device = GetGraphicsDevice();
            const VideoEncoderParameter* param = s_context->GetEncoderParameter(track);
            const UnityEncoderType encoderType = s_context->GetEncoderType();
            s_mapEncoder[track] = EncoderFactory::GetInstance().Init(
                param->width, param->height, device, encoderType);
            if (!s_context->InitializeEncoder(s_mapEncoder[track].get(), track))
            {
                // DebugLog("Encoder initialization faild.");
            }
            return;
        }
        case VideoStreamRenderEventID::Encode:
        {
            if (s_IsDevelopmentBuild)
                s_UnityProfiler->BeginSample(s_MarkerEncode);
            if(!s_context->EncodeFrame(track))
            {
                // DebugLog("Encode frame failed");
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
            // DebugLog("Unknown VideoStreamRenderEventID:%d", eventID);
            return;
        }
    }
}

extern "C" UnityRenderingEventAndData UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API GetRenderEventFunc(Context* context)
{
    s_context = context;
    return OnRenderEvent;
}

static void UNITY_INTERFACE_API TextureUpdateCallback(int eventID, void* data)
{
    if (s_context == nullptr)
        return;
    if (!ContextManager::GetInstance()->Exists(s_context))
        return;
    std::unique_lock<std::mutex> lock(s_context->mutex, std::try_to_lock);
    if (!lock.owns_lock())
        return;

    auto event = static_cast<UnityRenderingExtEventType>(eventID);

    if (event == kUnityRenderingExtEventUpdateTextureBeginV2)
    {
        auto params = reinterpret_cast<UnityRenderingExtTextureUpdateParamsV2 *>(data);
        
        auto renderer = s_context->GetVideoRenderer(params->userData);
        if (renderer == nullptr)
        {
            // DebugLog("VideoRenderer not found, rendererId:%d", params->userData);
            return;
        }

        renderer->ConvertVideoFrameToTextureAndWriteToBuffer(params->width, params->height, ConvertTextureFormat(params->format));
        params->texData = renderer->tempBuffer.data();
    }
}

extern "C" UnityRenderingEventAndData UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API GetUpdateTextureFunc(Context* context)
{
    s_context = context;
    return TextureUpdateCallback;
}
