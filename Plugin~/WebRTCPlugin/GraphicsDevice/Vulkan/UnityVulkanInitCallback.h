#pragma once
#include <vulkan/vulkan.h>

#ifdef _WIN32
#define LIBRARY_TYPE HMODULE
#elif defined __linux
#define LIBRARY_TYPE void*
#endif

namespace unity {
namespace webrtc {

#define EXPORTED_VULKAN_FUNCTION(func) extern PFN_##func func;
#define GLOBAL_VULKAN_FUNCTION(func) extern PFN_##func func;
#define INSTANCE_VULKAN_FUNCTION(func) extern PFN_##func func;
#define DEVICE_VULKAN_FUNCTION(func) extern PFN_##func func;

#include "ListOfVulkanFunctions.inl"

/// <summary>
///
/// </summary>
/// <param name="getInstanceProcAddr"></param>
/// <param name="userdata"></param>
/// <returns></returns>
PFN_vkGetInstanceProcAddr InterceptVulkanInitialization(
    PFN_vkGetInstanceProcAddr getInstanceProcAddr, void* userdata);

bool LoadVulkanLibrary(LIBRARY_TYPE& library);
bool LoadExportedVulkanFunction(LIBRARY_TYPE const& library);
bool LoadGlobalVulkanFunction();
bool LoadInstanceVulkanFunction(VkInstance instance);
bool LoadDeviceVulkanFunction(VkDevice device);


} // namespace webrtc
} // namespace unity
