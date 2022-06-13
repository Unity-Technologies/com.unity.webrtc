#include <IUnityProfiler.h>
#include <memory>

namespace unity
{
namespace webrtc
{
    template<typename T>
    inline int CreateCategory(T* instance, UnityProfilerCategoryId* category, const char* name, uint32_t unused)
    {
        return instance->CreateCategory(category, name, unused);
    }

    template<>
    inline int
    CreateCategory(IUnityProfiler* instance, UnityProfilerCategoryId* category, const char* name, uint32_t unused)
    {
        // IUnityProfiler(V1) is not supported CreateCategory.
        *category = kUnityProfilerCategoryRender;
        return 0;
    }

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
        virtual int CreateCategory(UnityProfilerCategoryId* category, const char* name, uint32_t unused) = 0;
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

        void BeginSample(const UnityProfilerMarkerDesc* markerDesc) override { profiler_->BeginSample(markerDesc); }

        void BeginSample(
            const UnityProfilerMarkerDesc* markerDesc,
            uint16_t eventDataCount,
            const UnityProfilerMarkerData* eventData) override
        {
            profiler_->BeginSample(markerDesc, eventDataCount, eventData);
        }

        void EndSample(const UnityProfilerMarkerDesc* markerDesc) override { profiler_->EndSample(markerDesc); }

        int IsAvailable() override { return profiler_->IsAvailable(); }

        int CreateMarker(
            const UnityProfilerMarkerDesc** desc,
            const char* name,
            UnityProfilerCategoryId category,
            UnityProfilerMarkerFlags flags,
            int eventDataCount) override
        {
            return profiler_->CreateMarker(desc, name, category, flags, eventDataCount);
        }

        int SetMarkerMetadataName(
            const UnityProfilerMarkerDesc* desc,
            int index,
            const char* metadataName,
            UnityProfilerMarkerDataType metadataType,
            UnityProfilerMarkerDataUnit metadataUnit) override
        {
            return profiler_->SetMarkerMetadataName(desc, index, metadataName, metadataType, metadataUnit);
        }

        int CreateCategory(UnityProfilerCategoryId* category, const char* name, uint32_t unused) override
        {
            return unity::webrtc::CreateCategory(profiler_, category, name, unused);
        }

        int RegisterThread(UnityProfilerThreadId* threadId, const char* groupName, const char* name) override
        {
            return profiler_->RegisterThread(threadId, groupName, name);
        }

        int UnregisterThread(UnityProfilerThreadId threadId) override { return profiler_->UnregisterThread(threadId); }

    private:
        T* profiler_;
    };

    inline std::unique_ptr<UnityProfiler> UnityProfiler::Get(IUnityInterfaces* unityInterfaces)
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