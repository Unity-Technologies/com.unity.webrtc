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

#define UNITY_USED_VULKAN_API_FUNCTIONS(apply) \
    apply(vkCreateInstance); \
    apply(vkCreateDevice); \
    apply(vkCreateImage); \
    apply(vkCmdBeginRenderPass); \
    apply(vkCreateBuffer); \
    apply(vkGetPhysicalDeviceMemoryProperties); \
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

} // namespace webrtc
} // namespace unity
