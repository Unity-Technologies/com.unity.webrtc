#include

template<typename T> void InterceptInitialization(T& vulkan, UnityVulkanInitCallback func, void* userdata)
{
    vulkan.InterceptInitialization(InterceptVulkanInitialization, nullptr);
}

