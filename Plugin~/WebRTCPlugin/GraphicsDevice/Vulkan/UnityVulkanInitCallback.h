#pragma once

#include <vector>
#include <vulkan/vulkan.h>

namespace unity
{
namespace webrtc
{

    // Preserve Unity's names and append requested names that are not already present by string value.
    std::vector<const char*>
    MergeExtensionNames(const char* const* enabled, uint32_t enabledCount, const std::vector<const char*>& requested);

    /// <summary>
    ///
    /// </summary>
    /// <param name="getInstanceProcAddr"></param>
    /// <param name="userdata"></param>
    /// <returns></returns>
    PFN_vkGetInstanceProcAddr
    InterceptVulkanInitialization(PFN_vkGetInstanceProcAddr getInstanceProcAddr, void* userdata);

} // namespace webrtc
} // namespace unity
