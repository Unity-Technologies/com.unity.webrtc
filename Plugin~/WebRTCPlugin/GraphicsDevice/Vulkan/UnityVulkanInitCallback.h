#pragma once

namespace unity {
namespace webrtc {

/// <summary>
///
/// </summary>
/// <param name="getInstanceProcAddr"></param>
/// <param name="userdata"></param>
/// <returns></returns>
PFN_vkGetInstanceProcAddr InterceptVulkanInitialization(
    PFN_vkGetInstanceProcAddr getInstanceProcAddr, void* userdata);

} // namespace webrtc
} // namespace unity
