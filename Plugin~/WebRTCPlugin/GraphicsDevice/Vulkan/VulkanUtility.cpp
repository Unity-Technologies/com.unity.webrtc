#include "pch.h"
#include "VulkanUtility.h"

#ifdef _WIN32
#include <Windows.h>
#include <vulkan/vulkan_win32.h>
#endif

#ifndef _WIN32
#define EXTERNAL_MEMORY_HANDLE_SUPPORTED_TYPE    VK_EXTERNAL_MEMORY_HANDLE_TYPE_OPAQUE_FD_BIT_KHR
#else
#define EXTERNAL_MEMORY_HANDLE_SUPPORTED_TYPE    VK_EXTERNAL_MEMORY_HANDLE_TYPE_OPAQUE_WIN32_BIT_KHR
#endif

namespace WebRTC {

//
bool VulkanUtility::FindMemoryTypeInto(const VkPhysicalDevice physicalDevice, uint32_t typeFilter,
        VkMemoryPropertyFlags properties, uint32_t* memoryTypeIndex)
{
    VkPhysicalDeviceMemoryProperties memProperties;
    vkGetPhysicalDeviceMemoryProperties(physicalDevice, &memProperties);

    for (uint32_t i = 0; i < memProperties.memoryTypeCount; ++i) {
        //properties define special features of the memory, like being able to map so we can write to it from the CPU. 
        if ((typeFilter & (1 << i)) && (memProperties.memoryTypes[i].propertyFlags & properties) == properties) {
            *memoryTypeIndex = i;
            return true;
        }
    }

    return false;
}
//---------------------------------------------------------------------------------------------------------------------

//initialLayout must be either VK_IMAGE_LAYOUT_UNDEFINED or VK_IMAGE_LAYOUT_PREINITIALIZED
//We use VK_IMAGE_LAYOUT_UNDEFINED here.
//Returns 0 when failed
VkDeviceSize VulkanUtility::CreateImage(const VkPhysicalDevice physicalDevice, const VkDevice device, 
    const VkAllocationCallbacks* allocator,
    const uint32_t width, const uint32_t height,
    const VkImageTiling tiling, const VkImageUsageFlags usage, const VkMemoryPropertyFlags properties,
    const VkFormat format,
    VkImage* image, VkDeviceMemory* imageMemory, bool exportHandle) 
{

    VkImageCreateInfo imageInfo = {};
    imageInfo.sType = VK_STRUCTURE_TYPE_IMAGE_CREATE_INFO;
    imageInfo.imageType = VK_IMAGE_TYPE_2D;
    imageInfo.extent.width = static_cast<uint32_t>(width);
    imageInfo.extent.height = static_cast<uint32_t>(height);
    imageInfo.extent.depth = 1;
    imageInfo.mipLevels = 1;
    imageInfo.arrayLayers = 1;
    imageInfo.format = format;
    imageInfo.tiling = tiling;
    imageInfo.initialLayout = VK_IMAGE_LAYOUT_UNDEFINED;
    imageInfo.usage = usage;
    imageInfo.sharingMode = VK_SHARING_MODE_EXCLUSIVE;
    imageInfo.samples = VK_SAMPLE_COUNT_1_BIT;
    imageInfo.flags = 0; // Optional
    if (vkCreateImage(device, &imageInfo, allocator, image) != VK_SUCCESS) {
        throw std::runtime_error("failed to create image!");
    }

    VkMemoryRequirements memRequirements;
    vkGetImageMemoryRequirements(device, *image, &memRequirements);

    VkMemoryAllocateInfo allocInfo = {};
    allocInfo.sType = VK_STRUCTURE_TYPE_MEMORY_ALLOCATE_INFO;
    allocInfo.allocationSize = memRequirements.size;
    if (!VulkanUtility::FindMemoryTypeInto(
        physicalDevice, memRequirements.memoryTypeBits, properties, &allocInfo.memoryTypeIndex)
       )
    {
        return 0;
    }

    VkExportMemoryAllocateInfoKHR exportInfo = {};
    if (exportHandle)  {
        exportInfo.sType = VK_STRUCTURE_TYPE_EXPORT_MEMORY_ALLOCATE_INFO_KHR;
        exportInfo.handleTypes = VK_EXTERNAL_MEMORY_HANDLE_TYPE_OPAQUE_WIN32_BIT_KHR;
        allocInfo.pNext = &exportInfo;
    }

    if (vkAllocateMemory(device, &allocInfo, allocator, imageMemory) != VK_SUCCESS) {
        return 0;
    }

    vkBindImageMemory(device, *image, *imageMemory, 0);
    return memRequirements.size;
}

//---------------------------------------------------------------------------------------------------------------------
//returns VK_NULL_HANDLE when failed
VkImageView  VulkanUtility::CreateImageView(const VkDevice device, const VkAllocationCallbacks* allocator,
                                              const VkImage image, const VkFormat format)
{
    VkImageViewCreateInfo viewInfo = {};
    viewInfo.sType = VK_STRUCTURE_TYPE_IMAGE_VIEW_CREATE_INFO;
    viewInfo.image = image;
    viewInfo.viewType = VK_IMAGE_VIEW_TYPE_2D;
    viewInfo.format = format;
    viewInfo.subresourceRange.aspectMask = VK_IMAGE_ASPECT_COLOR_BIT;
    viewInfo.subresourceRange.baseMipLevel = 0;
    viewInfo.subresourceRange.levelCount = 1;
    viewInfo.subresourceRange.baseArrayLayer = 0;
    viewInfo.subresourceRange.layerCount = 1;
    viewInfo.components.r = VK_COMPONENT_SWIZZLE_IDENTITY;
    viewInfo.components.g = VK_COMPONENT_SWIZZLE_IDENTITY;
    viewInfo.components.b = VK_COMPONENT_SWIZZLE_IDENTITY;
    viewInfo.components.a = VK_COMPONENT_SWIZZLE_IDENTITY;


    VkImageView imageView = VK_NULL_HANDLE;
    if (vkCreateImageView(device, &viewInfo, allocator, &imageView) != VK_SUCCESS) {
        return VK_NULL_HANDLE;
    }

    return imageView;
}

//---------------------------------------------------------------------------------------------------------------------

//Requires VK_KHR_get_physical_device_properties2 extension
bool VulkanUtility::GetPhysicalDeviceUUIDInto(VkInstance instance, VkPhysicalDevice phyDevice,
    std::array<uint8_t, VK_UUID_SIZE>* deviceUUID)
{
    VkPhysicalDeviceIDPropertiesKHR deviceIDProps = {};
    deviceIDProps.sType = VK_STRUCTURE_TYPE_PHYSICAL_DEVICE_ID_PROPERTIES_KHR;

    VkPhysicalDeviceProperties2KHR props = {};
    props.sType = VK_STRUCTURE_TYPE_PHYSICAL_DEVICE_PROPERTIES_2_KHR;
    props.pNext = &deviceIDProps;

    PFN_vkGetPhysicalDeviceProperties2KHR func = (PFN_vkGetPhysicalDeviceProperties2KHR) 
        vkGetInstanceProcAddr(instance, "vkGetPhysicalDeviceProperties2KHR");
    if (func == nullptr) {
        return false;
    }

    func(phyDevice, &props);
    std::memcpy(deviceUUID->data(), deviceIDProps.deviceUUID, VK_UUID_SIZE);

    return true;
}

//---------------------------------------------------------------------------------------------------------------------

#ifndef _WIN32
void* VulkanUtility::GetExportHandle(const VkDevice device, const VkDeviceMemory memory)
{
    int fd = -1;

    VkMemoryGetFdInfoKHR fdInfo = {};
    fdInfo.sType = VK_STRUCTURE_TYPE_MEMORY_GET_FD_INFO_KHR;
    fdInfo.memory = memory;
    fdInfo.handleType = VK_EXTERNAL_MEMORY_HANDLE_TYPE_OPAQUE_FD_BIT_KHR;

    auto func = (PFN_vkGetMemoryFdKHR) \
        vkGetDeviceProcAddr(device, "vkGetMemoryFdKHR");

    if (!func ||
        func(device, &fdInfo, &fd) != VK_SUCCESS) {
        return nullptr;
    }

    return (void *)(uintptr_t)fd;
}
#else
void* VulkanUtility::GetExportHandle(const VkDevice device, const VkDeviceMemory memory)
{
    HANDLE handle = nullptr;

    VkMemoryGetWin32HandleInfoKHR handleInfo = {};
    handleInfo.sType = VK_STRUCTURE_TYPE_MEMORY_GET_WIN32_HANDLE_INFO_KHR;
    handleInfo.memory = memory;
    handleInfo.handleType = EXTERNAL_MEMORY_HANDLE_SUPPORTED_TYPE;

    auto func = (PFN_vkGetMemoryWin32HandleKHR) \
        vkGetDeviceProcAddr(device, "vkGetMemoryWin32HandleKHR");

    if (!func ||
        func(device, &handleInfo, &handle) != VK_SUCCESS) {
        return nullptr;
    }

    return (void *)handle;
}
#endif

} //end namespace
