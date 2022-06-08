#pragma once

#include "IUnityProfiler.h"

namespace unity
{
namespace webrtc
{
    class ScopedProfiler;
    class ScopedProfilerThread;
    class ProfilerMarkerFactory
    {
    public:
        static std::unique_ptr<ProfilerMarkerFactory> Create(IUnityInterfaces* unityInterfaces);

        const UnityProfilerMarkerDesc* CreateMarker(
            const char* name, UnityProfilerCategoryId category, UnityProfilerMarkerFlags flags, int eventDataCount);

        // todo(kazuki):: support Unity2021.1 above
        UnityProfilerCategoryId CreateCategory(const char* name);

        std::unique_ptr<const ScopedProfiler> CreateScopedProfiler(const UnityProfilerMarkerDesc& desc);
        std::unique_ptr<const ScopedProfilerThread> CreateScopedProfilerThread(const char* groupName, const char* name);

        virtual ~ProfilerMarkerFactory();

    protected:
        ProfilerMarkerFactory(IUnityProfiler* profiler_);

    private:
        IUnityProfiler* profiler_;
    };
}
}
