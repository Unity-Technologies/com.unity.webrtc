#include <IUnityProfiler.h>

namespace unity
{
namespace webrtc
{
    class UnityProfiler
    {
    public:
        virtual void BeginSample(const UnityProfilerMarkerDesc* markerDesc) = 0;
        virtual void BeginSample(
            const UnityProfilerMarkerDesc* markerDesc,
            uint16_t eventDataCount,
            const UnityProfilerMarkerData* eventData) = 0;
        virtual void EndSample(const UnityProfilerMarkerDesc* markerDesc) = 0;
        virtual int IsAvailable() = 0;
        virtual int CreateMarker(
            const UnityProfilerMarkerDesc** desc,
            const char* name,
            UnityProfilerCategoryId category,
            UnityProfilerMarkerFlags flags,
            int eventDataCount) = 0;
        virtual int SetMarkerMetadataName(
            const UnityProfilerMarkerDesc* desc,
            int index,
            const char* metadataName,
            UnityProfilerMarkerDataType metadataType,
            UnityProfilerMarkerDataUnit metadataUnit) = 0;
        virtual int RegisterThread(UnityProfilerThreadId* threadId, const char* groupName, const char* name) = 0;
        virtual int UnregisterThread(UnityProfilerThreadId threadId) = 0;
        virtual ~UnityProfiler() = default;

        static std::unique_ptr<UnityProfiler> Get(IUnityInterfaces* unityInterfaces);
    };

    template<typename T>
    class UnityProfilerImpl : public UnityProfiler
    {
    public:
        UnityProfilerImpl(T* profiler)
            : profiler_(profiler)
        {
        }
        ~UnityProfilerImpl() = default;

        void BeginSample(const UnityProfilerMarkerDesc* markerDesc)
        {
            profiler_->BeginSample(markerDesc);
        }
        virtual void BeginSample(
            const UnityProfilerMarkerDesc* markerDesc,
            uint16_t eventDataCount,
            const UnityProfilerMarkerData* eventData) = 0;
        virtual void EndSample(const UnityProfilerMarkerDesc* markerDesc) = 0;
        virtual int IsAvailable() = 0;
        virtual int CreateMarker(
            const UnityProfilerMarkerDesc** desc,
            const char* name,
            UnityProfilerCategoryId category,
            UnityProfilerMarkerFlags flags,
            int eventDataCount) = 0;
        virtual int SetMarkerMetadataName(
            const UnityProfilerMarkerDesc* desc,
            int index,
            const char* metadataName,
            UnityProfilerMarkerDataType metadataType,
            UnityProfilerMarkerDataUnit metadataUnit) = 0;
        virtual int RegisterThread(UnityProfilerThreadId* threadId, const char* groupName, const char* name) = 0;
        virtual int UnregisterThread(UnityProfilerThreadId threadId) = 0;

    private:
        T* profiler_;
    };

    std::unique_ptr<UnityProfiler> UnityProfiler::Get(IUnityInterfaces* unityInterfaces)
    {
        IUnityProfilerV2* profilerV2 = unityInterfaces->Get<IUnityProfilerV2>();
        if (profilerV2)
            return std::make_unique<UnityProfilerImpl<IUnityProfilerV2>>(profilerV2);
        IUnityProfiler* profiler = unityInterfaces->Get<IUnityProfiler>();
        if (profiler)
            return std::make_unique<UnityProfilerImpl<IUnityProfiler>>(profiler);
        return nullptr;
    }
}
}