#include "pch.h"

#include <IUnityGraphics.h>
#include <IUnityProfiler.h>
#include <IUnityRenderingExtensions.h>

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
        case UnityRenderingExtTextureFormat::kUnityRenderingExtFormatA8R8G8B8_SRGB:
        case UnityRenderingExtTextureFormat::kUnityRenderingExtFormatA8R8G8B8_UNorm:
            return webrtc::VideoType::kABGR;
        }
        throw std::invalid_argument("not support texture format");
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

static void UNITY_INTERFACE_API TextureUpdateCallback(int eventID, void* data)
{
    auto event = static_cast<UnityRenderingExtEventType>(eventID);

    if (event == kUnityRenderingExtEventUpdateTextureBeginV2)
    {
        auto params = reinterpret_cast<UnityRenderingExtTextureUpdateParamsV2 *>(data);
        
        auto renderer = s_context->GetVideoRenderer(params->userData);
        if (renderer == nullptr)
        {
            DebugLog("VideoRenderer not found, rendererId:%d", params->userData);
            return;
        }

        auto frame = renderer->GetFrameBuffer();
        if(frame == nullptr)
        {
            DebugLog("VideoFrame is not received yet, rendererId:%d", params->userData);
            return;
        }

        rtc::scoped_refptr<webrtc::I420Buffer> i420_buffer = webrtc::I420Buffer::Create(params->width, params->height);
        i420_buffer->ScaleFrom(*frame->ToI420());
        delete[] renderer->tempBuffer;
        renderer->tempBuffer = new uint8_t[params->width * params->height * 4];

        libyuv::ConvertFromI420(
            i420_buffer->DataY(), i420_buffer->StrideY(), i420_buffer->DataU(),
            i420_buffer->StrideU(), i420_buffer->DataV(), i420_buffer->StrideV(),
            renderer->tempBuffer, 0, params->width, params->height,
            ConvertVideoType(ConvertTextureFormat(params->format)));
        params->texData = renderer->tempBuffer;
    }
    else if (event == kUnityRenderingExtEventUpdateTextureEndV2)
    {
        auto params = reinterpret_cast<UnityRenderingExtTextureUpdateParamsV2 *>(data);
        auto renderer = s_context->GetVideoRenderer(params->userData);
        if (renderer == nullptr)
        {
            DebugLog("VideoRenderer not found, rendererId:%d", params->userData);
            return;
        }

        delete[] renderer->tempBuffer;
        renderer->tempBuffer = nullptr;
    }
}

extern "C" UnityRenderingEventAndData UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API GetUpdateTextureFunc(Context* context)
{
    s_context = context;
    return TextureUpdateCallback;
}
