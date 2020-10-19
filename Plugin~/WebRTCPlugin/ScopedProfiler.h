#pragma once
#include <IUnityProfiler.h>

namespace unity
{
namespace webrtc
{

class ScopedProfiler
{
public:
    static IUnityProfiler* UnityProfiler;

    ScopedProfiler(const UnityProfilerMarkerDesc &desc);
    ~ScopedProfiler();
private:
    void operator =(const ScopedProfiler& src) const {}
    ScopedProfiler(const ScopedProfiler& src) {}

    const UnityProfilerMarkerDesc* m_desc;
};

} // end namespace webrtc
} // end namespace unity
