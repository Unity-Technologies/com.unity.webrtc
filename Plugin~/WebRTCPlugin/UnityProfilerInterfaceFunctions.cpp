#include "pch.h"

#include "UnityProfilerInterfaceFunctions.h"

namespace unity
{
namespace webrtc
{
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
