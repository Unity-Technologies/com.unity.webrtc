#include "pch.h"

#include <IUnityGraphics.h>
#include <IUnityProfiler.h>
#include <IUnityRenderingExtensions.h>

#include "Context.h"
#include "ScopedProfiler.h"
#include "UnityVideoTrackSource.h"
#include "GraphicsDevice/GraphicsDevice.h"
#include "GraphicsDevice/GraphicsUtility.h"
#include "VideoFrame.h"
#include "GpuMemoryBufferPool.h"

#if defined(SUPPORT_VULKAN)
#include "UnityVulkanInterfaceFunctions.h"
#include "GraphicsDevice/Vulkan/UnityVulkanInitCallback.h"
#endif

enum class VideoStreamRenderEventID
{
    Initialize = 0,
    Encode = 1,
    Finalize = 2
};

using namespace unity::webrtc;
using namespace ::webrtc;

namespace unity
{
namespace webrtc
{
    IUnityInterfaces* s_UnityInterfaces = nullptr;
    IUnityGraphics* s_Graphics = nullptr;
    Context* s_context = nullptr;
    std::map<const uint32_t, std::shared_ptr<UnityVideoRenderer>> s_mapVideoRenderer;
    std::unique_ptr <Clock> s_clock;

    const UnityProfilerMarkerDesc* s_MarkerEncode = nullptr;
    const UnityProfilerMarkerDesc* s_MarkerDecode = nullptr;
    std::unique_ptr<IGraphicsDevice> s_gfxDevice;
    std::unique_ptr<GpuMemoryBufferPool> s_bufferPool;

    IGraphicsDevice* GraphicsUtility::GetGraphicsDevice()
    {
        RTC_DCHECK(s_gfxDevice.get());
        return s_gfxDevice.get();
    }

    UnityGfxRenderer GraphicsUtility::GetGfxRenderer()
    {
        RTC_DCHECK(s_Graphics);
        return s_Graphics->GetRenderer();
    }

    static libyuv::FourCC ConvertTextureFormat(UnityRenderingExtTextureFormat type)
    {
        switch (type)
        {
        case UnityRenderingExtTextureFormat::kUnityRenderingExtFormatB8G8R8A8_SRGB:
        case UnityRenderingExtTextureFormat::kUnityRenderingExtFormatB8G8R8A8_UNorm:
        case UnityRenderingExtTextureFormat::kUnityRenderingExtFormatB8G8R8A8_SNorm:
        case UnityRenderingExtTextureFormat::kUnityRenderingExtFormatB8G8R8A8_UInt:
        case UnityRenderingExtTextureFormat::kUnityRenderingExtFormatB8G8R8A8_SInt:
            return libyuv::FOURCC_ARGB;
        case UnityRenderingExtTextureFormat::kUnityRenderingExtFormatR8G8B8A8_SRGB:
        case UnityRenderingExtTextureFormat::kUnityRenderingExtFormatR8G8B8A8_UNorm:
        case UnityRenderingExtTextureFormat::kUnityRenderingExtFormatR8G8B8A8_SNorm:
        case UnityRenderingExtTextureFormat::kUnityRenderingExtFormatR8G8B8A8_UInt:
        case UnityRenderingExtTextureFormat::kUnityRenderingExtFormatR8G8B8A8_SInt:
            return libyuv::FOURCC_ABGR;
        case UnityRenderingExtTextureFormat::kUnityRenderingExtFormatA8R8G8B8_SRGB:
        case UnityRenderingExtTextureFormat::kUnityRenderingExtFormatA8R8G8B8_UNorm:
            return libyuv::FOURCC_BGRA;
        default:
            return libyuv::FOURCC_ANY;
        }
    }
} // end namespace webrtc
} // end namespace unity

using namespace unity::webrtc;

#if defined(SUPPORT_VULKAN)
LIBRARY_TYPE s_vulkanLibrary = nullptr;

bool LoadVulkanFunctions(UnityVulkanInstance& instance)
{
    if (!LoadVulkanLibrary(s_vulkanLibrary))
    {
        RTC_LOG(LS_ERROR) << "Failed loading vulkan library";
        return false;
    }
    if (!LoadExportedVulkanFunction(s_vulkanLibrary))
    {
        RTC_LOG(LS_ERROR) << "Failed loading vulkan exported function";
        return false;
    }

    if (!LoadInstanceVulkanFunction(instance.instance))
    {
        RTC_LOG(LS_ERROR) << "Failed loading vulkan instance function";
        return false;
    }
    if (!LoadDeviceVulkanFunction(instance.device))
    {
        RTC_LOG(LS_ERROR) << "Failed loading vulkan device function";
        return false;
    }
    return true;
}
#endif

static void UNITY_INTERFACE_API OnGraphicsDeviceEvent(UnityGfxDeviceEventType eventType)
{
    switch (eventType)
    {
    case kUnityGfxDeviceEventInitialize:
    {
        /// note::
        /// kUnityGfxDeviceEventInitialize event is occurred twice on Unity Editor.
        /// First time, s_UnityInterfaces return UnityGfxRenderer as kUnityGfxRendererNull.
        /// The actual value of UnityGfxRenderer is returned on second time.

        s_mapVideoRenderer.clear();

        UnityGfxRenderer renderer =
            s_UnityInterfaces->Get<IUnityGraphics>()->GetRenderer();
        if (renderer == kUnityGfxRendererNull)
            break;

#if defined(SUPPORT_VULKAN)
        if (renderer == kUnityGfxRendererVulkan)
        {
            std::unique_ptr<UnityGraphicsVulkan> vulkan = UnityGraphicsVulkan::Get(s_UnityInterfaces);
            UnityVulkanInstance instance = vulkan->Instance();
            if (!LoadVulkanFunctions(instance))
            {
                RTC_LOG(LS_INFO) << "LoadVulkanFunctions failed";
                return;
            }
        }
#endif
        s_gfxDevice.reset(GraphicsDevice::GetInstance().Init(s_UnityInterfaces));
        if(s_gfxDevice != nullptr)
        {
            s_gfxDevice->InitV();
        }
        s_bufferPool = std::make_unique<GpuMemoryBufferPool>(s_gfxDevice.get());
        break;
    }
    case kUnityGfxDeviceEventShutdown:
    {
        s_mapVideoRenderer.clear();

        if (s_gfxDevice != nullptr)
        {
            s_gfxDevice->ShutdownV();
            s_gfxDevice.reset();
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

// forward declaration for plugin load event
void PluginLoad(IUnityInterfaces* unityInterfaces);
void PluginUnload();

// Unity plugin load event
//
// "That is simply registering our UnityPluginLoad and UnityPluginUnload,
// as on iOS we cannot use dynamic libraries (hence we cannot load functions
// from them by name as we usually do on other platforms)."
// https://github.com/Unity-Technologies/iOSNativeCodeSamples/blob/2019-dev/Graphics/MetalNativeRenderingPlugin/README.md
//
#if defined(UNITY_IOS) || defined(UNITY_IOS_SIMULATOR)
extern "C" void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API UnityWebRTCPluginLoad(IUnityInterfaces* unityInterfaces)
{
    PluginLoad(unityInterfaces);
}

extern "C" void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API UnityWebRTCPluginUnload()
{
    PluginUnload();
}
#else
extern "C" void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API UnityPluginLoad(IUnityInterfaces* unityInterfaces)
{
    PluginLoad(unityInterfaces);
}

extern "C" void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API UnityPluginUnload()
{
    PluginUnload();
}
#endif


// Unity plugin load event
void PluginLoad(IUnityInterfaces* unityInterfaces)
{
#if WIN32 && _DEBUG
    _CrtSetDbgFlag(_CRTDBG_ALLOC_MEM_DF | _CRTDBG_LEAK_CHECK_DF);
#endif

    s_UnityInterfaces = unityInterfaces;
    s_Graphics = unityInterfaces->Get<IUnityGraphics>();
    s_Graphics->RegisterDeviceEventCallback(OnGraphicsDeviceEvent);
    s_clock.reset(Clock::GetRealTimeClock());

#if defined(SUPPORT_VULKAN)
    auto vulkan = UnityGraphicsVulkan::Get(s_UnityInterfaces);
    vulkan->InterceptInitialization(InterceptVulkanInitialization, nullptr);
#endif

    IUnityProfiler* unityProfiler = unityInterfaces->Get<IUnityProfiler>();
    if (unityProfiler != nullptr)
    {
        unityProfiler->CreateMarker(
            &s_MarkerEncode,
            "UnityVideoTrackSource.OnFrameCaptured",
            kUnityProfilerCategoryRender,
            kUnityProfilerMarkerFlagDefault, 0);
        unityProfiler->CreateMarker(
            &s_MarkerDecode,
            "UnityVideoRenderer.ConvertVideoFrameToTextureAndWriteToBuffer",
            kUnityProfilerCategoryRender,
            kUnityProfilerMarkerFlagDefault, 0);
        ScopedProfiler::UnityProfiler = unityProfiler;
    }

    OnGraphicsDeviceEvent(kUnityGfxDeviceEventInitialize);
}

void PluginUnload()
{
    s_Graphics->UnregisterDeviceEventCallback(OnGraphicsDeviceEvent);
    s_clock.reset();
}

// Data format used by the managed code.
// CommandBuffer.IssuePluginEventAndData method pass data packed by this format. 
struct EncodeData
{
    void* texture;
    UnityVideoTrackSource* source;
    int width;
    int height;
    UnityRenderingExtTextureFormat format;
};

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

    EncodeData* encodeData =
        static_cast<EncodeData*>(data);

    RTC_DCHECK_GT(encodeData->width, 0);
    RTC_DCHECK_GT(encodeData->height, 0);

    const VideoStreamRenderEventID event =
        static_cast<VideoStreamRenderEventID>(eventID);


    switch(event)
    {
        case VideoStreamRenderEventID::Encode:
        {
            UnityVideoTrackSource* source = encodeData->source;
            if (source == nullptr)
                return;
            int64_t timestamp = s_clock->TimeInMicroseconds();
            IGraphicsDevice* device = GraphicsUtility::GetGraphicsDevice();
            UnityGfxRenderer gfxRenderer = GraphicsUtility::GetGfxRenderer();
            void* ptr = GraphicsUtility::TextureHandleToNativeGraphicsPtr(
                encodeData->texture, device, gfxRenderer);
            Size size(encodeData->width, encodeData->height);
            {
                ScopedProfiler profiler(*s_MarkerEncode);

                auto frame =
                    s_bufferPool->CreateFrame(ptr, size, encodeData->format, timestamp);
                source->OnFrameCaptured(std::move(frame));
            }
            return;
        }
        default: {
            RTC_DCHECK(0);
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
            return;
        s_mapVideoRenderer[params->userData] = renderer;
        {
            ScopedProfiler profiler(*s_MarkerDecode);
            params->texData =
                renderer->ConvertVideoFrameToTextureAndWriteToBuffer(
                params->width, params->height, ConvertTextureFormat(params->format));

        }
    }
    if (event == kUnityRenderingExtEventUpdateTextureEndV2)
    {
        auto params = reinterpret_cast<UnityRenderingExtTextureUpdateParamsV2 *>(data);
        s_mapVideoRenderer.erase(params->userData);
    }
}

extern "C" UnityRenderingEventAndData UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API GetUpdateTextureFunc(Context* context)
{
    s_context = context;
    return TextureUpdateCallback;
}
