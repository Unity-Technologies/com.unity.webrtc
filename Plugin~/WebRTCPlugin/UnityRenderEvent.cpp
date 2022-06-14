#include "pch.h"

#include "Context.h"
#include "GpuMemoryBufferPool.h"
#include "GraphicsDevice/GraphicsDevice.h"
#include "GraphicsDevice/GraphicsUtility.h"
#include "ProfilerMarkerFactory.h"
#include "ScopedProfiler.h"
#include "UnityProfilerInterfaceFunctions.h"
#include "UnityVideoTrackSource.h"
#include "VideoFrame.h"

#if defined(SUPPORT_VULKAN)
#include "GraphicsDevice/Vulkan/UnityVulkanInitCallback.h"
#include "UnityVulkanInterfaceFunctions.h"
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
    static IUnityInterfaces* s_UnityInterfaces = nullptr;
    static IUnityGraphics* s_Graphics = nullptr;
    static Context* s_context = nullptr;
    static std::unique_ptr<UnityProfiler> s_UnityProfiler = nullptr;
    static std::unique_ptr<ProfilerMarkerFactory> s_ProfilerMarkerFactory = nullptr;
    static std::map<const uint32_t, std::shared_ptr<UnityVideoRenderer>> s_mapVideoRenderer;
    static std::unique_ptr<Clock> s_clock;

    static const UnityProfilerMarkerDesc* s_MarkerEncode = nullptr;
    static const UnityProfilerMarkerDesc* s_MarkerDecode = nullptr;
    static std::unique_ptr<IGraphicsDevice> s_gfxDevice;
    static std::unique_ptr<GpuMemoryBufferPool> s_bufferPool;

    IGraphicsDevice* Plugin::GraphicsDevice()
    {
        RTC_DCHECK(s_gfxDevice.get());
        return s_gfxDevice.get();
    }

    ProfilerMarkerFactory* Plugin::ProfilerMarkerFactory() { return s_ProfilerMarkerFactory.get(); }

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
static LIBRARY_TYPE s_vulkanLibrary = nullptr;

static bool LoadVulkanFunctions(UnityVulkanInstance& instance)
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

        UnityGfxRenderer renderer = s_UnityInterfaces->Get<IUnityGraphics>()->GetRenderer();
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

            UnityVulkanPluginEventConfig config;
            config.graphicsQueueAccess = kUnityVulkanGraphicsQueueAccess_DontCare;
            config.renderPassPrecondition = kUnityVulkanRenderPass_EnsureInside;
            config.flags = kUnityVulkanEventConfigFlag_EnsurePreviousFrameSubmission |
                kUnityVulkanEventConfigFlag_ModifiesCommandBuffersState;

            vulkan->ConfigureEvent(static_cast<int>(VideoStreamRenderEventID::Encode), &config);
        }
#endif
        s_gfxDevice.reset(GraphicsDevice::GetInstance().Init(s_UnityInterfaces, s_ProfilerMarkerFactory.get()));
        if (s_gfxDevice != nullptr)
        {
            s_gfxDevice->InitV();
        }
        s_bufferPool = std::make_unique<GpuMemoryBufferPool>(s_gfxDevice.get(), s_clock.get());
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

        // UnityPluginUnload not called normally
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
    }
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

extern "C" void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API UnityWebRTCPluginUnload() { PluginUnload(); }
#else
extern "C" void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API UnityPluginLoad(IUnityInterfaces* unityInterfaces)
{
    PluginLoad(unityInterfaces);
}

extern "C" void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API UnityPluginUnload() { PluginUnload(); }
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
    if (!vulkan->AddInterceptInitialization(InterceptVulkanInitialization, nullptr, 0))
    {
        RTC_LOG(LS_INFO) << "AddInterceptInitialization failed.";
    }
#endif
    s_UnityProfiler = UnityProfiler::Get(unityInterfaces);
    s_ProfilerMarkerFactory = ProfilerMarkerFactory::Create(s_UnityProfiler.get());

    if (s_ProfilerMarkerFactory)
    {
        s_MarkerEncode = s_ProfilerMarkerFactory->CreateMarker(
            "UnityVideoTrackSource.OnFrameCaptured", kUnityProfilerCategoryRender, kUnityProfilerMarkerFlagDefault, 0);
        s_MarkerDecode = s_ProfilerMarkerFactory->CreateMarker(
            "UnityVideoRenderer.ConvertVideoFrameToTextureAndWriteToBuffer",
            kUnityProfilerCategoryRender,
            kUnityProfilerMarkerFlagDefault,
            0);
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
    if (!lock.owns_lock())
        return;

    EncodeData* encodeData = static_cast<EncodeData*>(data);

    RTC_DCHECK(encodeData->texture);
    RTC_DCHECK(encodeData->source);
    RTC_DCHECK_GT(encodeData->width, 0);
    RTC_DCHECK_GT(encodeData->height, 0);

    const VideoStreamRenderEventID event = static_cast<VideoStreamRenderEventID>(eventID);

    switch (event)
    {
    case VideoStreamRenderEventID::Encode:
    {
        UnityVideoTrackSource* source = encodeData->source;
        if (!s_context->ExistsRefPtr(source))
            return;
        Timestamp timestamp = s_clock->CurrentTime();
        IGraphicsDevice* device = Plugin::GraphicsDevice();
        UnityGfxRenderer gfxRenderer = device->GetGfxRenderer();
        void* ptr = GraphicsUtility::TextureHandleToNativeGraphicsPtr(encodeData->texture, device, gfxRenderer);
        unity::webrtc::Size size(encodeData->width, encodeData->height);
        {
            std::unique_ptr<const ScopedProfiler> profiler;
            if (s_ProfilerMarkerFactory)
                profiler = s_ProfilerMarkerFactory->CreateScopedProfiler(*s_MarkerEncode);

            auto frame = s_bufferPool->CreateFrame(ptr, size, encodeData->format, timestamp);
            source->OnFrameCaptured(std::move(frame));
        }

        s_bufferPool->ReleaseStaleBuffers(timestamp);

        return;
    }
    default:
    {
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
        auto params = reinterpret_cast<UnityRenderingExtTextureUpdateParamsV2*>(data);

        auto renderer = s_context->GetVideoRenderer(params->userData);
        if (renderer == nullptr)
            return;
        s_mapVideoRenderer[params->userData] = renderer;
        int width = static_cast<int>(params->width);
        int height = static_cast<int>(params->height);

        {
            std::unique_ptr<const ScopedProfiler> profiler;
            if (s_ProfilerMarkerFactory)
                profiler = s_ProfilerMarkerFactory->CreateScopedProfiler(*s_MarkerDecode);

            params->texData = renderer->ConvertVideoFrameToTextureAndWriteToBuffer(
                width, height, ConvertTextureFormat(params->format));
        }
    }
    if (event == kUnityRenderingExtEventUpdateTextureEndV2)
    {
        auto params = reinterpret_cast<UnityRenderingExtTextureUpdateParamsV2*>(data);
        s_mapVideoRenderer.erase(params->userData);
    }
}

extern "C" UnityRenderingEventAndData UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API GetUpdateTextureFunc(Context* context)
{
    s_context = context;
    return TextureUpdateCallback;
}
