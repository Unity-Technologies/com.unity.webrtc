#include "pch.h"

#include "ScopedProfiler.h"
#include "UnityProfilerInterfaceFunctions.h"

namespace unity
{
namespace webrtc
{
    ScopedProfiler::ScopedProfiler(UnityProfiler* profiler, const UnityProfilerMarkerDesc& desc)
        : profiler_(profiler)
        , m_desc(&desc)
    {
        RTC_DCHECK(profiler);

        if (profiler_->IsAvailable())
            profiler_->BeginSample(m_desc);
    }

    ScopedProfiler::~ScopedProfiler()
    {
        if (profiler_->IsAvailable())
            profiler_->EndSample(m_desc);
    }

    ScopedProfilerThread::ScopedProfilerThread(UnityProfiler* profiler, const char* groupName, const char* name)
        : profiler_(profiler)
    {
        RTC_DCHECK(profiler);
        RTC_DCHECK(groupName);
        RTC_DCHECK(name);

        if (profiler_->IsAvailable())
        {
            int result = profiler_->RegisterThread(&threadId_, groupName, name);
            if (result)
            {
                RTC_LOG(LS_INFO) << "IUnityProfiler::RegisterThread error:" << result;
                throw;
            }
        }
    }
    ScopedProfilerThread ::~ScopedProfilerThread()
    {
        if (profiler_->IsAvailable())
            profiler_->UnregisterThread(threadId_);
    }

} // end namespace webrtc
} // end namespace unity
