#include "pch.h"

namespace unity
{
namespace webrtc
{

// todo(kazuki):: fix workaround
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Wmacro-redefined"

#define EXPORTED_VULKAN_FUNCTION(func) PFN_##func func;
#define GLOBAL_VULKAN_FUNCTION(func) PFN_##func func;
#define INSTANCE_VULKAN_FUNCTION(func) PFN_##func func;
#define DEVICE_VULKAN_FUNCTION(func) PFN_##func func;

#include "ListOfVulkanFunctions.inl"

    static LIBRARY_TYPE s_vulkanLibrary = nullptr;

    bool LoadVulkanFunctions(UnityVulkanInstance& instance)
    {
        if (!LoadVulkanLibrary(s_vulkanLibrary))
        {
            RTC_LOG(LS_ERROR) << "Failed loading vulkan library";
            return false;
        }
        if (!LoadExportedVulkanFunction(s_vulkanLibrary))
        {
            RTC_LOG(LS_ERROR) << "Failed loading vulkan exported function";
            return false;
        }

        if (!LoadInstanceVulkanFunction(instance.instance))
        {
            RTC_LOG(LS_ERROR) << "Failed loading vulkan instance function";
            return false;
        }
        if (!LoadDeviceVulkanFunction(instance.device))
        {
            RTC_LOG(LS_ERROR) << "Failed loading vulkan device function";
            return false;
        }
        return true;
    }

    bool LoadVulkanLibrary(LIBRARY_TYPE& library)
    {
// Keep the logic similar to Unity internals at VKApiFunctions.cpp
#if UNITY_WIN
        library = LoadLibrary("vulkan-1.dll");
#elif UNITY_ANDROID
        library = dlopen("libvulkan.so", RTLD_NOW | RTLD_LOCAL);
#elif UNITY_LINUX
        library = dlopen("libvulkan.so.1", RTLD_NOW | RTLD_LOCAL);
#else
#error Unsupported Platform
#endif
        if (library == nullptr)
            return false;
        return true;
    }

    bool LoadExportedVulkanFunction(LIBRARY_TYPE const& library)
    {
#if UNITY_WIN
#define LoadFunction GetProcAddress
#elif UNITY_ANDROID || UNITY_LINUX
#define LoadFunction dlsym
#endif

#define EXPORTED_VULKAN_FUNCTION(name)                                                                                 \
    name = (PFN_##name)LoadFunction(library, #name);                                                                   \
    if (name == nullptr)                                                                                               \
    {                                                                                                                  \
        return false;                                                                                                  \
    }
#include "ListOfVulkanFunctions.inl"
        return true;
    }

    bool LoadGlobalVulkanFunction()
    {
#define GLOBAL_VULKAN_FUNCTION(name)                                                                                   \
    name = (PFN_##name)vkGetInstanceProcAddr(nullptr, #name);                                                          \
    if (name == nullptr)                                                                                               \
    {                                                                                                                  \
        return false;                                                                                                  \
    }
#include "ListOfVulkanFunctions.inl"
        return true;
    }

    bool LoadInstanceVulkanFunction(VkInstance instance)
    {
#define INSTANCE_VULKAN_FUNCTION(name)                                                                                 \
    name = (PFN_##name)vkGetInstanceProcAddr(instance, #name);                                                         \
    if (name == nullptr)                                                                                               \
    {                                                                                                                  \
        return false;                                                                                                  \
    }
#include "ListOfVulkanFunctions.inl"
        return true;
    }

    bool LoadDeviceVulkanFunction(VkDevice device)
    {
#define DEVICE_VULKAN_FUNCTION(name)                                                                                   \
    name = (PFN_##name)vkGetDeviceProcAddr(device, #name);                                                             \
    if (name == nullptr)                                                                                               \
    {                                                                                                                  \
        return false;                                                                                                  \
    }
#include "ListOfVulkanFunctions.inl"
        return true;
    }
#pragma clang diagnostic pop

} // namespace webrtc
} // namespace unity
