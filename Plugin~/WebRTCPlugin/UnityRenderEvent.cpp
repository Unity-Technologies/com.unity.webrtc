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

    static const size_t kLimitBufferCount = 20;
    static constexpr TimeDelta kStaleFrameLimit = TimeDelta::Seconds(10);
    static const UnityProfilerMarkerDesc* s_MarkerEncode = nullptr;
    static const UnityProfilerMarkerDesc* s_MarkerDecode = nullptr;
    static std::unique_ptr<IGraphicsDevice> s_gfxDevice;
    static std::unique_ptr<GpuMemoryBufferPool> s_bufferPool;
    static int s_batchUpdateEventID = 0;

    IGraphicsDevice* Plugin::GraphicsDevice() { return s_gfxDevice.get(); }

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
        UnityGfxRenderer renderer = s_UnityInterfaces->Get<IUnityGraphics>()->GetRenderer();
        if (renderer == kUnityGfxRendererNull)
            break;

        // Reserve eventID range to use for custom plugin events.
        s_batchUpdateEventID = s_UnityInterfaces->Get<IUnityGraphics>()->ReserveEventIDRange(1);

#if defined(SUPPORT_VULKAN)
        if (renderer == kUnityGfxRendererVulkan)
        {
            std::unique_ptr<UnityGraphicsVulkan> vulkan = UnityGraphicsVulkan::Get(s_UnityInterfaces);
            UnityVulkanInstance instance = vulkan->Instance();

            // Load vulkan functions dynamically.
            if (!LoadVulkanFunctions(instance))
            {
                RTC_LOG(LS_INFO) << "LoadVulkanFunctions failed";
                return;
            }

            /// note::
            /// Configure the event on the rendering thread called from CommandBuffer::IssuePluginEventAndData method in
            /// managed code.
            UnityVulkanPluginEventConfig batchUpdateEventConfig;
            batchUpdateEventConfig.graphicsQueueAccess = kUnityVulkanGraphicsQueueAccess_DontCare;
            batchUpdateEventConfig.renderPassPrecondition = kUnityVulkanRenderPass_EnsureOutside;
            batchUpdateEventConfig.flags = kUnityVulkanEventConfigFlag_EnsurePreviousFrameSubmission |
                kUnityVulkanEventConfigFlag_ModifiesCommandBuffersState;

            vulkan->ConfigureEvent(s_batchUpdateEventID, &batchUpdateEventConfig);
        }
#endif
        s_gfxDevice.reset(GraphicsDevice::GetInstance().Init(s_UnityInterfaces, s_ProfilerMarkerFactory.get()));
        if (s_gfxDevice)
        {
            s_gfxDevice->InitV();
        }
        s_bufferPool = std::make_unique<GpuMemoryBufferPool>(s_gfxDevice.get(), s_clock.get());
        break;
    }
    case kUnityGfxDeviceEventShutdown:
    {
        // Release buffers before graphics device because buffers depends on the device.
        s_bufferPool = nullptr;

        s_mapVideoRenderer.clear();

        if (s_gfxDevice)
        {
            s_gfxDevice->ShutdownV();
            s_gfxDevice = nullptr;
        }

        // UnityPluginUnload not called normally
        s_Graphics->UnregisterDeviceEventCallback(OnGraphicsDeviceEvent);
        s_clock = nullptr;
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
#if _WIN32 && _DEBUG
    _CrtSetDbgFlag(_CRTDBG_ALLOC_MEM_DF | _CRTDBG_LEAK_CHECK_DF);
#endif

    s_UnityInterfaces = unityInterfaces;
    s_Graphics = unityInterfaces->Get<IUnityGraphics>();
    s_Graphics->RegisterDeviceEventCallback(OnGraphicsDeviceEvent);
    s_clock.reset(Clock::GetRealTimeClock());

#if defined(SUPPORT_VULKAN)
    /// note::
    /// Intercept Vulkan initialization process to hook vulkan functions because vulkan extensions need adding when
    /// initializing vulkan device. This process have to be run before graphics device initialization.
    std::unique_ptr<UnityGraphicsVulkan> vulkan;
    vulkan = UnityGraphicsVulkan::Get(s_UnityInterfaces);
    if (!vulkan)
    {
        RTC_LOG(LS_INFO) << "Failed to get Vulkan.";
    }
    else if (!vulkan->AddInterceptInitialization(InterceptVulkanInitialization, nullptr, 0))
    {
        RTC_LOG(LS_INFO) << "AddInterceptInitialization failed.";
    }
#endif
    s_UnityProfiler = UnityProfiler::Get(unityInterfaces);
    if (s_UnityProfiler)
    {
        s_ProfilerMarkerFactory = ProfilerMarkerFactory::Create(s_UnityProfiler.get());
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

void PluginUnload() { OnGraphicsDeviceEvent(kUnityGfxDeviceEventShutdown); }

enum class VideoStreamTrackAction
{
    Ignore = 0,
    Decode = 1,
    Encode = 2,
};

// Keep in sync with VideoStreamTrack.cs
struct VideoStreamTrackData
{
    VideoStreamTrackAction action;
    void* texture;
    void* source;
    int width;
    int height;
    UnityRenderingExtTextureFormat format;
};

// Data format used by the managed code.
// CommandBuffer.IssuePluginEventAndData method pass data packed by this format.
struct BatchData
{
    int32_t tracksCount;
    VideoStreamTrackData** tracks;
};

// Notice: When DebugLog is used in a method called from RenderingThread,
// it hangs when attempting to leave PlayMode and re-enter PlayMode.
// So, we comment out `DebugLog`.
static void UNITY_INTERFACE_API OnBatchUpdateEvent(int eventID, void* data)
{
    if (eventID != s_batchUpdateEventID)
        return;
    if (!s_context)
        return;
    if (!ContextManager::GetInstance()->Exists(s_context))
        return;
    std::unique_lock<std::mutex> lock(s_context->mutex, std::try_to_lock);
    if (!lock.owns_lock())
        return;

    BatchData* batchData = static_cast<BatchData*>(data);

    if (!batchData || !batchData->tracks)
    {
        // Release all buffers.
        if (s_bufferPool)
            s_bufferPool->ReleaseStaleBuffers(Timestamp::PlusInfinity(), kStaleFrameLimit);
        return;
    }

    IGraphicsDevice* device = Plugin::GraphicsDevice();
    UnityGfxRenderer gfxRenderer = device->GetGfxRenderer();
    Timestamp timestamp = s_clock->CurrentTime();

    if (!device->UpdateState())
        return;

    for (int i = 0; i < batchData->tracksCount; i++)
    {
        VideoStreamTrackData* trackData = batchData->tracks[i];

        if (!trackData || !trackData->texture || !trackData->source)
            continue;

        if (trackData->action == VideoStreamTrackAction::Encode)
        {
            RTC_DCHECK(trackData->texture);
            RTC_DCHECK(trackData->source);
            RTC_DCHECK_GT(trackData->width, 0);
            RTC_DCHECK_GT(trackData->height, 0);

            UnityVideoTrackSource* source = static_cast<UnityVideoTrackSource*>(trackData->source);
            if (!s_context->ExistsRefPtr(source))
            {
                trackData->source = nullptr;
                continue;
            }

            timestamp = s_clock->CurrentTime();
            void* ptr = GraphicsUtility::TextureHandleToNativeGraphicsPtr(trackData->texture, device, gfxRenderer);
            if (!ptr)
            {
                RTC_LOG(LS_ERROR) << "GraphicsUtility::TextureHandleToNativeGraphicsPtr returns nullptr.";
                return;
            }
            unity::webrtc::Size size(trackData->width, trackData->height);

            if (s_bufferPool->bufferCount() < kLimitBufferCount)
            {
                std::unique_ptr<const ScopedProfiler> profiler;
                if (s_ProfilerMarkerFactory)
                    profiler = s_ProfilerMarkerFactory->CreateScopedProfiler(*s_MarkerEncode);

                auto frame = s_bufferPool->CreateFrame(ptr, size, trackData->format, timestamp);
                source->OnFrameCaptured(std::move(frame));
            }
        }
#if 0
        else if (trackData->action == VideoStreamTrackAction::Decode)
        {
            std::unique_ptr<const ScopedProfiler> profiler;
            if (s_ProfilerMarkerFactory)
                profiler = s_ProfilerMarkerFactory->CreateScopedProfiler(*s_MarkerDecodeCopy);
            UnityVideoRenderer* renderer = static_cast<UnityVideoRenderer*>(trackData->source);
            renderer->CopyBuffer(trackData->texture, trackData->width, trackData->height, trackData->format);
        }
#endif
    }

    s_bufferPool->ReleaseStaleBuffers(timestamp, kStaleFrameLimit);
}

extern "C" UnityRenderingEventAndData UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API
GetBatchUpdateEventFunc(Context* context)
{
    s_context = context;
    return OnBatchUpdateEvent;
}

extern "C" int UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API GetBatchUpdateEventID() { return s_batchUpdateEventID; }

static void UNITY_INTERFACE_API TextureUpdateCallback(int eventID, void* data)
{
    if (!s_context)
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
            if (s_UnityProfiler && s_UnityProfiler->IsAvailable())
                s_UnityProfiler->BeginSample(s_MarkerDecode);

            params->texData = renderer->ConvertVideoFrameToTextureAndWriteToBuffer(
                width, height, ConvertTextureFormat(params->format));
        }
    }
    if (event == kUnityRenderingExtEventUpdateTextureEndV2)
    {
        auto params = reinterpret_cast<UnityRenderingExtTextureUpdateParamsV2*>(data);
        s_mapVideoRenderer.erase(params->userData);

        if (s_UnityProfiler && s_UnityProfiler->IsAvailable())
            s_UnityProfiler->EndSample(s_MarkerDecode);
    }
}

extern "C" UnityRenderingEventAndData UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API GetUpdateTextureFunc(Context* context)
{
    s_context = context;
    return TextureUpdateCallback;
}
