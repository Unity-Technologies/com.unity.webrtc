#include "pch.h"

#include "IUnityInterface.h"
#include "ProfilerMarkerFactory.h"
#include "ScopedProfiler.h"
#include "UnityProfilerInterfaceFunctions.h"

namespace unity
{
namespace webrtc
{
    std::unique_ptr<ProfilerMarkerFactory> ProfilerMarkerFactory::Create(UnityProfiler* profiler)
    {
        RTC_DCHECK(profiler);
        return std::unique_ptr<ProfilerMarkerFactory>(new ProfilerMarkerFactory(profiler));
    }

    ProfilerMarkerFactory::ProfilerMarkerFactory(UnityProfiler* profiler)
        : profiler_(profiler)
    {
    }

    ProfilerMarkerFactory::~ProfilerMarkerFactory() { }

    const UnityProfilerMarkerDesc* ProfilerMarkerFactory::CreateMarker(
        const char* name, UnityProfilerCategoryId category, UnityProfilerMarkerFlags flags, int eventDataCount)
    {
        const UnityProfilerMarkerDesc* desc = nullptr;
        int result = profiler_->CreateMarker(&desc, name, category, flags, eventDataCount);
        if (result)
        {
            RTC_LOG(LS_ERROR) << "IUnityProfiler::CreateMarker error" << result;
            throw;
        }
        return desc;
    }

    UnityProfilerCategoryId ProfilerMarkerFactory::CreateCategory(const char* name)
    {
        // todo(kazuki):
        // return profiler_->CreateCategory(&desc, name, category, flags, eventDataCount);
        return kUnityProfilerCategoryRender;
    }

    std::unique_ptr<const ScopedProfiler>
    ProfilerMarkerFactory::CreateScopedProfiler(const UnityProfilerMarkerDesc& desc)
    {
        return std::make_unique<const ScopedProfiler>(profiler_, desc);
    }

    std::unique_ptr<const ScopedProfilerThread>
    ProfilerMarkerFactory::CreateScopedProfilerThread(const char* groupName, const char* name)
    {
        return std::make_unique<const ScopedProfilerThread>(profiler_, groupName, name);
    }
}
}
