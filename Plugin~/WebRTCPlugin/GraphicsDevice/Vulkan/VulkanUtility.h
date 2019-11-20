#pragma once

namespace WebRTC {

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

    static VkImageView  CreateImageView(const VkDevice device, const VkAllocationCallbacks* allocator, 
                                        const VkImage image, const VkFormat format);


    static bool GetPhysicalDeviceUUIDInto( VkInstance instance, VkPhysicalDevice phyDevice,
        std::array<uint8_t, VK_UUID_SIZE>* deviceUUID
    );

    static void* GetExportHandle(const VkDevice device, const VkDeviceMemory memory);


};
} //end namespace


