#pragma once

#include <array>

#include <IUnityGraphicsVulkan.h>
#include <IUnityRenderingExtensions.h>
#include <vulkan/vulkan.h>

namespace unity
{
namespace webrtc
{

    class VulkanUtility
    {

    public:
        static bool FindMemoryTypeIndex(
            const VkPhysicalDevice physicalDevice,
            uint32_t typeFilter,
            VkMemoryPropertyFlags properties,
            uint32_t* memoryTypeIndex);

        static VkResult CreateImage(
            const UnityVulkanInstance& instance,
            const VkAllocationCallbacks* allocator,
            const uint32_t width,
            const uint32_t height,
            const VkImageTiling tiling,
            const VkImageUsageFlags usage,
            const VkMemoryPropertyFlags properties,
            const VkFormat format,
            UnityVulkanImage* image,
            bool exportHandle);

        static VkImageView CreateImageView(
            const UnityVulkanInstance& instance,
            const VkAllocationCallbacks* allocator,
            const VkImage image,
            const VkFormat format);

        static bool GetPhysicalDeviceUUID(
            VkInstance instance, VkPhysicalDevice phyDevice, std::array<uint8_t, VK_UUID_SIZE>* deviceUUID);

        static bool LoadDeviceFunctions(const VkDevice device);
        static bool LoadInstanceFunctions(const VkInstance instance);
        static void* GetExportHandle(const VkDevice device, const VkDeviceMemory memory);

        static VkResult DoImageLayoutTransition(
            const VkCommandBuffer commandBuffer,
            const VkImage image,
            const VkFormat format,
            const VkImageLayout oldLayout,
            const VkPipelineStageFlags oldStage,
            const VkImageLayout newLayout,
            const VkPipelineStageFlags newStage);

        static VkResult CopyImage(
            const VkCommandBuffer commandBuffer,
            const VkImage srcImage,
            const VkImage dstImage,
            const uint32_t width,
            const uint32_t height);
    };

} // end namespace webrtc
} // end namespace unity
