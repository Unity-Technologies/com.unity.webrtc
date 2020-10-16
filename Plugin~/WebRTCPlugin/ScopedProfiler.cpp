#include "pch.h"
#include "ScopedProfiler.h"

namespace unity
{
namespace webrtc
{
    IUnityProfiler* ScopedProfiler::UnityProfiler = nullptr;

    ScopedProfiler::ScopedProfiler(const UnityProfilerMarkerDesc& desc)
    : m_desc(&desc)
    {
        if (UnityProfiler == nullptr || UnityProfiler->IsAvailable() == 0)
            return;
        UnityProfiler->BeginSample(m_desc);
    }

    ScopedProfiler::~ScopedProfiler()
    {
        if (UnityProfiler == nullptr || UnityProfiler->IsAvailable() == 0)
            return;
        UnityProfiler->EndSample(m_desc);
    }
} // end namespace webrtc
} // end namespace unity
