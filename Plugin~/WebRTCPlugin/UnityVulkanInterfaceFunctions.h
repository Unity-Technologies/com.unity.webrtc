//#include <vulkan/vulkan.h>
#include <IUnityGraphicsVulkan.h>
//#include <IUnityProfiler.h>
//#include <IUnityRenderingExtensions.h>

template<typename T>
void InterceptInitialization(T& vulkan, UnityVulkanInitCallback func, void* userdata)
{
    vulkan.InterceptInitialization(func, nullptr);
}

