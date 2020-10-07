#pragma once

namespace unity
{
namespace webrtc
{

class VulkanUtility {

public:

    static bool FindMemoryTypeInto(const VkPhysicalDevice physicalDevice, uint32_t typeFilter,
        VkMemoryPropertyFlags properties, uint32_t* memoryTypeIndex);


    static VkDeviceSize CreateImage(const VkPhysicalDevice physicalDevice, const VkDevice device, 
        const VkAllocationCallbacks* allocator,
        const uint32_t width, const uint32_t height,
        const VkImageTiling tiling, const VkImageUsageFlags usage, const VkMemoryPropertyFlags properties,
        const VkFormat format,
        VkImage* image, VkDeviceMemory* imageMemory, bool exportHandle);

    static VkResult CreateImage(const VkPhysicalDevice physicalDevice, const VkDevice device,
        const VkAllocationCallbacks* allocator,
        const uint32_t width, const uint32_t height,
        const VkImageTiling tiling, const VkImageUsageFlags usage, const VkMemoryPropertyFlags properties,
        const VkFormat format,
        UnityVulkanImage* image, bool exportHandle);

    static VkImageView  CreateImageView(const VkDevice device, const VkAllocationCallbacks* allocator, 
                                        const VkImage image, const VkFormat format);


    static bool GetPhysicalDeviceUUIDInto( VkInstance instance, VkPhysicalDevice phyDevice,
        std::array<uint8_t, VK_UUID_SIZE>* deviceUUID
    );

    static void* GetExportHandle(const VkDevice device, const VkDeviceMemory memory);

    static VkResult BeginOneTimeCommandBufferInto(const VkDevice device, const VkCommandPool commandPool,
        VkCommandBuffer* commandBuffer);

    //Uses vkQueueWaitIdle to synchronize
    static VkResult EndAndSubmitOneTimeCommandBuffer(const VkDevice device, const VkCommandPool commandPool, 
                                                  const VkQueue queue, VkCommandBuffer commandBuffer);

    static VkResult DoImageLayoutTransition(const VkDevice device, const VkCommandPool commandPool, const VkQueue queue, 
                                        const VkImage image, VkFormat format, 
                                        const VkImageLayout oldLayout, const VkPipelineStageFlags oldStage,
                                        const VkImageLayout newLayout, const VkPipelineStageFlags newStage);

    static VkResult CopyImage(const VkDevice device, const VkCommandPool commandPool, const VkQueue queue,
               const VkImage srcImage, const VkImage dstImage,
               const uint32_t width, const uint32_t height);

};

} // end namespace webrtc
} // end namespace unity


