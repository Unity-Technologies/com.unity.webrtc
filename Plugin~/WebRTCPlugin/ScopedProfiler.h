#pragma once

#include <IUnityProfiler.h>

namespace unity
{
namespace webrtc
{
    class UnityProfiler;

    class ScopedProfiler
    {
    public:
        ScopedProfiler(UnityProfiler* profiler, const UnityProfilerMarkerDesc& desc);
        ~ScopedProfiler();

    private:
        void operator=(const ScopedProfiler& src) const { }
        ScopedProfiler(const ScopedProfiler& src) { }

        UnityProfiler* profiler_;
        const UnityProfilerMarkerDesc* m_desc;
    };

    class ScopedProfilerThread
    {
    public:
        ScopedProfilerThread(UnityProfiler* profiler, const char* groupName, const char* name);
        ~ScopedProfilerThread();

    private:
        void operator=(const ScopedProfilerThread& src) const { }
        ScopedProfilerThread(const ScopedProfilerThread& src) { }

        UnityProfiler* profiler_;
        UnityProfilerThreadId threadId_;
    };

} // end namespace webrtc
} // end namespace unity
