#pragma once

namespace unity {
namespace webrtc {

#define UNITY_USED_VULKAN_API_FUNCTIONS(apply) \
    apply(vkCreateInstance); \
    apply(vkCreateDevice); \
    apply(vkDestroyDevice); \
    apply(vkCreateImage); \
    apply(vkCmdBeginRenderPass); \
    apply(vkCreateBuffer); \
    apply(vkGetPhysicalDeviceMemoryProperties); \
    apply(vkGetPhysicalDeviceQueueFamilyProperties); \
    apply(vkGetBufferMemoryRequirements); \
    apply(vkGetImageSubresourceLayout); \
    apply(vkMapMemory); \
    apply(vkBindBufferMemory); \
    apply(vkAllocateMemory); \
    apply(vkAllocateCommandBuffers); \
    apply(vkCreateCommandPool); \
    apply(vkDestroyCommandPool); \
    apply(vkDestroyBuffer); \
    apply(vkDestroyImage); \
    apply(vkEnumerateDeviceExtensionProperties); \
    apply(vkEnumeratePhysicalDevices); \
    apply(vkGetDeviceQueue); \
    apply(vkFreeMemory); \
    apply(vkUnmapMemory); \
    apply(vkQueueWaitIdle); \
    apply(vkDeviceWaitIdle); \
    apply(vkCmdCopyBufferToImage); \
    apply(vkFlushMappedMemoryRanges); \
    apply(vkCreatePipelineLayout); \
    apply(vkCreateShaderModule); \
    apply(vkDestroyShaderModule); \
    apply(vkCreateGraphicsPipelines); \
    apply(vkCmdBindPipeline); \
    apply(vkCmdDraw); \
    apply(vkCmdPushConstants); \
    apply(vkCmdBindVertexBuffers); \
    apply(vkDestroyPipeline); \
    apply(vkBeginCommandBuffer); \
    apply(vkBindImageMemory); \
    apply(vkCmdCopyImage); \
    apply(vkCmdPipelineBarrier); \
    apply(vkCreateImageView); \
    apply(vkEndCommandBuffer); \
    apply(vkFreeCommandBuffers); \
    apply(vkGetDeviceProcAddr); \
    apply(vkGetImageMemoryRequirements); \
    apply(vkQueueSubmit); \
    apply(vkDestroyPipelineLayout);

#define VULKAN_DEFINE_API_FUNCPTR(func) static PFN_##func func
    VULKAN_DEFINE_API_FUNCPTR(vkGetInstanceProcAddr);
    UNITY_USED_VULKAN_API_FUNCTIONS(VULKAN_DEFINE_API_FUNCPTR);
#undef VULKAN_DEFINE_API_FUNCPTR

/// <summary>
///
/// </summary>
/// <param name="getInstanceProcAddr"></param>
/// <param name="userdata"></param>
/// <returns></returns>
PFN_vkGetInstanceProcAddr InterceptVulkanInitialization(
    PFN_vkGetInstanceProcAddr getInstanceProcAddr, void* userdata);



static void LoadVulkanAPI(PFN_vkGetInstanceProcAddr getInstanceProcAddr, VkInstance instance);

} // namespace webrtc
} // namespace unity
