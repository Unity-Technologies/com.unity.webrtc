#include "pch.h"

#if defined(__linux)
#include <dlfcn.h>
#endif

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

bool LoadVulkanLibrary(LIBRARY_TYPE& library) {
// Keep the logic similar to Unity internals at VKApiFunctions.cpp
#if defined(UNITY_WIN)
    library = LoadLibrary("vulkan-1.dll");
#elif defined(UNITY_ANDROID)
    library = dlopen("libvulkan.so", RTLD_NOW | RTLD_LOCAL);
#elif defined(UNITY_LINUX)
    library = dlopen("libvulkan.so.1", RTLD_NOW | RTLD_LOCAL);
#else
#error Unsupported Platform
#endif
    if (library == nullptr)
        return false;
    return true;
}

bool LoadExportedVulkanFunction(LIBRARY_TYPE const& library) {
#if defined(UNITY_WIN)
#define LoadFunction GetProcAddress
#elif defined(__linux)
#define LoadFunction dlsym
#endif

#define EXPORTED_VULKAN_FUNCTION( name ) \
    name = (PFN_##name)LoadFunction( library, #name ); \
    if (name == nullptr) { \
        return false; \
    }
#include "ListOfVulkanFunctions.inl"
    return true;
}

bool LoadGlobalVulkanFunction() {
#define GLOBAL_VULKAN_FUNCTION( name ) \
    name = (PFN_##name)vkGetInstanceProcAddr( nullptr, #name ); \
    if (name == nullptr) { \
        return false; \
    }
#include "ListOfVulkanFunctions.inl"
    return true;
}

bool LoadInstanceVulkanFunction(VkInstance instance) {
#define INSTANCE_VULKAN_FUNCTION( name ) \
    name = (PFN_##name)vkGetInstanceProcAddr( instance, #name ); \
    if (name == nullptr) { \
        return false; \
    }
#include "ListOfVulkanFunctions.inl"
    return true;
}

bool LoadDeviceVulkanFunction(VkDevice device) {
#define DEVICE_VULKAN_FUNCTION( name ) \
    name = (PFN_##name)vkGetDeviceProcAddr( device, #name ); \
    if (name == nullptr) { \
        return false; \
    }
#include "ListOfVulkanFunctions.inl"
    return true;
}
#pragma clang diagnostic pop

} // namespace webrtc
} // namespace unity
