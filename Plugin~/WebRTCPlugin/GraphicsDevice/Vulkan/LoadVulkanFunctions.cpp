#include "pch.h"

#if defined(__linux)
#include <dlfcn.h>
#endif

namespace unity
{
namespace webrtc
{

#define EXPORTED_VULKAN_FUNCTION(func) PFN_##func func;
#define GLOBAL_VULKAN_FUNCTION(func) PFN_##func func;
#define INSTANCE_VULKAN_FUNCTION(func) PFN_##func func;
#define DEVICE_VULKAN_FUNCTION(func) PFN_##func func;

#include "ListOfVulkanFunctions.inl"

bool LoadVulkanLibrary(LIBRARY_TYPE& library) {
#if defined(_WIN32)
    library = LoadLibrary("vulkan-1.dll");
#elif defined(__linux)
    library = dlopen("libvulkan.so", RTLD_NOW | RTLD_LOCAL);
#endif
    if (library == nullptr)
        return false;
    return true;
}

bool LoadExportedVulkanFunction(LIBRARY_TYPE const& library) {
#if defined(_WIN32)
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

} // namespace webrtc
} // namespace unity
