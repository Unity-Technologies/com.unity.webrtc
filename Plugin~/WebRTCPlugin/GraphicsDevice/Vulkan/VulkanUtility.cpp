#include "pch.h"
#include "VulkanUtility.h"
#include "WebRTCMacros.h"

#ifdef _WIN32
#include <Windows.h>
#include <vulkan/vulkan_win32.h>
#endif

#ifndef _WIN32
#define EXTERNAL_MEMORY_HANDLE_SUPPORTED_TYPE    VK_EXTERNAL_MEMORY_HANDLE_TYPE_OPAQUE_FD_BIT_KHR
#else
#define EXTERNAL_MEMORY_HANDLE_SUPPORTED_TYPE    VK_EXTERNAL_MEMORY_HANDLE_TYPE_OPAQUE_WIN32_BIT_KHR
#endif

namespace unity
{
namespace webrtc
{

//
bool VulkanUtility::FindMemoryTypeInto(
    const VkPhysicalDevice physicalDevice, uint32_t typeFilter,
        VkMemoryPropertyFlags properties, uint32_t* memoryTypeIndex)
{
    VkPhysicalDeviceMemoryProperties memProperties;
    vkGetPhysicalDeviceMemoryProperties(physicalDevice, &memProperties);

    for (uint32_t i = 0; i < memProperties.memoryTypeCount; ++i) {
        //properties define special features of the memory, like being able to map so we can write to it from the CPU. 
        if ((typeFilter & (1 << i)) &&
            (memProperties.memoryTypes[i].propertyFlags & properties) == properties) {
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
        return 0;
    }

    VkMemoryRequirements memRequirements;
    vkGetImageMemoryRequirements(device, *image, &memRequirements);

    VkMemoryAllocateInfo allocInfo = {};
    allocInfo.sType = VK_STRUCTURE_TYPE_MEMORY_ALLOCATE_INFO;
    allocInfo.allocationSize = memRequirements.size;
    if (!VulkanUtility::FindMemoryTypeInto(
        physicalDevice, memRequirements.memoryTypeBits,
        properties, &allocInfo.memoryTypeIndex)
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

    VULKAN_CHECK_FAILVALUE(vkBindImageMemory(device, *image, *imageMemory, 0), 0);

    return memRequirements.size;
}

//---------------------------------------------------------------------------------------------------------------------
VkResult VulkanUtility::CreateImage(
    const VkPhysicalDevice physicalDevice, const VkDevice device,
    const VkAllocationCallbacks* allocator,
    const uint32_t width, const uint32_t height,
    const VkImageTiling tiling, const VkImageUsageFlags usage, const VkMemoryPropertyFlags properties,
    const VkFormat format,
    UnityVulkanImage* unityVulkanImage, bool exportHandle)
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
    VkResult result = vkCreateImage(device, &imageInfo, allocator, &unityVulkanImage->image);
    if(result != VK_SUCCESS) {
        return result;
    }

    VkMemoryRequirements memRequirements;
    vkGetImageMemoryRequirements(device, unityVulkanImage->image, &memRequirements);

    VkMemoryAllocateInfo allocInfo = {};
    allocInfo.sType = VK_STRUCTURE_TYPE_MEMORY_ALLOCATE_INFO;
    allocInfo.allocationSize = memRequirements.size;
    bool success = VulkanUtility::FindMemoryTypeInto(
        physicalDevice, memRequirements.memoryTypeBits,
        properties, &allocInfo.memoryTypeIndex);
    RTC_CHECK(success);

    VkExportMemoryAllocateInfoKHR exportInfo = {};
    if (exportHandle) {
        exportInfo.sType = VK_STRUCTURE_TYPE_EXPORT_MEMORY_ALLOCATE_INFO_KHR;
        exportInfo.handleTypes = VK_EXTERNAL_MEMORY_HANDLE_TYPE_OPAQUE_WIN32_BIT_KHR;
        allocInfo.pNext = &exportInfo;
    }

    result = vkAllocateMemory(
        device, &allocInfo, allocator, &unityVulkanImage->memory.memory);
    if(result != VK_SUCCESS) {
        return result;
    }

    const VkDeviceSize memoryOffset = 0;
    result = vkBindImageMemory(
        device, unityVulkanImage->image,
        unityVulkanImage->memory.memory, memoryOffset);
    if (result != VK_SUCCESS) {
        return result;
    }

    unityVulkanImage->memory.offset = memoryOffset;
    unityVulkanImage->memory.size = memRequirements.size;
    unityVulkanImage->memory.flags = properties;
    unityVulkanImage->memory.memoryTypeIndex = allocInfo.memoryTypeIndex;
    unityVulkanImage->layout = imageInfo.initialLayout;
    unityVulkanImage->usage = imageInfo.usage;
    unityVulkanImage->format = imageInfo.format;
    unityVulkanImage->extent = imageInfo.extent;
    unityVulkanImage->tiling = imageInfo.tiling;
    unityVulkanImage->type = imageInfo.imageType;
    unityVulkanImage->samples = imageInfo.samples;
    unityVulkanImage->layers = imageInfo.arrayLayers;
    unityVulkanImage->mipCount = imageInfo.mipLevels;

    return VK_SUCCESS;
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

    return reinterpret_cast<void*>(handle);
}
#endif

//---------------------------------------------------------------------------------------------------------------------

VkResult VulkanUtility::BeginOneTimeCommandBufferInto(const VkDevice device, const VkCommandPool commandPool,
    VkCommandBuffer* commandBuffer)
{
    //Create a command buffer to copy
    VkCommandBufferAllocateInfo allocInfo = {};
    allocInfo.sType = VK_STRUCTURE_TYPE_COMMAND_BUFFER_ALLOCATE_INFO;
    allocInfo.level = VK_COMMAND_BUFFER_LEVEL_PRIMARY;
    allocInfo.commandPool = commandPool;
    allocInfo.commandBufferCount = 1;

    vkAllocateCommandBuffers(device, &allocInfo, commandBuffer);

    VkCommandBufferBeginInfo beginInfo = {};
    beginInfo.sType = VK_STRUCTURE_TYPE_COMMAND_BUFFER_BEGIN_INFO;
    beginInfo.flags = VK_COMMAND_BUFFER_USAGE_ONE_TIME_SUBMIT_BIT; //used only once

    VULKAN_CHECK(vkBeginCommandBuffer(*commandBuffer, &beginInfo));

    return VK_SUCCESS;   
}

//---------------------------------------------------------------------------------------------------------------------

//Uses vkQueueWaitIdle to synchronize
VkResult VulkanUtility::EndAndSubmitOneTimeCommandBuffer(const VkDevice device, const VkCommandPool commandPool, 
                                                  const VkQueue queue, VkCommandBuffer commandBuffer)
{
    vkEndCommandBuffer(commandBuffer);

    VkSubmitInfo submitInfo = {};
    submitInfo.sType = VK_STRUCTURE_TYPE_SUBMIT_INFO;
    submitInfo.commandBufferCount = 1;
    submitInfo.pCommandBuffers = &commandBuffer;

    VULKAN_CHECK(vkQueueSubmit(queue, 1, &submitInfo, VK_NULL_HANDLE));
    VULKAN_CHECK(vkQueueWaitIdle(queue));

    vkFreeCommandBuffers(device, commandPool, 1, &commandBuffer);

    return VK_SUCCESS;
}

//---------------------------------------------------------------------------------------------------------------------

VkResult VulkanUtility::DoImageLayoutTransition(
    const VkDevice device, const VkCommandPool commandPool, const VkQueue queue, 
    const VkImage image, const VkFormat format, 
    const VkImageLayout oldLayout, const VkPipelineStageFlags oldStage,
    const VkImageLayout newLayout, const VkPipelineStageFlags newStage) 
{ 
    VkCommandBuffer commandBuffer = VK_NULL_HANDLE;
    VULKAN_CHECK(BeginOneTimeCommandBufferInto(device, commandPool, &commandBuffer));

    VkImageMemoryBarrier barrier = {};
    barrier.sType = VK_STRUCTURE_TYPE_IMAGE_MEMORY_BARRIER;
    barrier.oldLayout = oldLayout;
    barrier.newLayout = newLayout;
    barrier.srcQueueFamilyIndex = VK_QUEUE_FAMILY_IGNORED; //for transferring queue family ownership
    barrier.dstQueueFamilyIndex = VK_QUEUE_FAMILY_IGNORED;
    
    barrier.image = image;
    barrier.subresourceRange.aspectMask = VK_IMAGE_ASPECT_COLOR_BIT;
    barrier.subresourceRange.baseMipLevel = 0;
    barrier.subresourceRange.levelCount = 1; //No mip map
    barrier.subresourceRange.baseArrayLayer = 0;
    barrier.subresourceRange.layerCount = 1;

	switch (oldLayout) 	{
	    case VK_IMAGE_LAYOUT_UNDEFINED:
	        // undefined (or does not matter). Only valid as initial layout
		    // No flags required.
		    barrier.srcAccessMask = 0;
		    break;

	    case VK_IMAGE_LAYOUT_PREINITIALIZED:
		    // Image is preinitialized. Only valid as initial layout for linear images, preserves memory contents
		    // Make sure host writes have been finished
		    barrier.srcAccessMask = VK_ACCESS_HOST_WRITE_BIT;
		    break;

	    case VK_IMAGE_LAYOUT_COLOR_ATTACHMENT_OPTIMAL:
		    // Image is a color attachment
		    // Make sure any writes to the color buffer have been finished
		    barrier.srcAccessMask = VK_ACCESS_COLOR_ATTACHMENT_WRITE_BIT;
		    break;

	    case VK_IMAGE_LAYOUT_DEPTH_STENCIL_ATTACHMENT_OPTIMAL:
		    // Image is a depth/stencil attachment
		    // Make sure any writes to the depth/stencil buffer have been finished
		    barrier.srcAccessMask = VK_ACCESS_DEPTH_STENCIL_ATTACHMENT_WRITE_BIT;
		    break;

	    case VK_IMAGE_LAYOUT_TRANSFER_SRC_OPTIMAL:
		    // Image is a transfer source 
		    // Make sure any reads from the image have been finished
		    barrier.srcAccessMask = VK_ACCESS_TRANSFER_READ_BIT;
		    break;

	    case VK_IMAGE_LAYOUT_TRANSFER_DST_OPTIMAL:
		    // Image is a transfer destination
		    // Make sure any writes to the image have been finished
		    barrier.srcAccessMask = VK_ACCESS_TRANSFER_WRITE_BIT;
		    break;

	    case VK_IMAGE_LAYOUT_SHADER_READ_ONLY_OPTIMAL:
		    // Image is read by a shader
		    // Make sure any shader reads from the image have been finished
		    barrier.srcAccessMask = VK_ACCESS_SHADER_READ_BIT;
		    break;
	    default:
		    // Other source layouts aren't handled (yet)
		    break;
	}

	switch (newLayout) 	{
	    case VK_IMAGE_LAYOUT_TRANSFER_DST_OPTIMAL:
		    // Image will be used as a transfer destination
		    // Make sure any writes to the image have been finished
		    barrier.dstAccessMask = VK_ACCESS_TRANSFER_WRITE_BIT;
		    break;

	    case VK_IMAGE_LAYOUT_TRANSFER_SRC_OPTIMAL:
		    // Image will be used as a transfer source
		    // Make sure any reads from the image have been finished
		    barrier.dstAccessMask = VK_ACCESS_TRANSFER_READ_BIT;
		    break;

	    case VK_IMAGE_LAYOUT_COLOR_ATTACHMENT_OPTIMAL:
		    // Image will be used as a color attachment
		    // Make sure any writes to the color buffer have been finished
		    barrier.dstAccessMask = VK_ACCESS_COLOR_ATTACHMENT_WRITE_BIT;
		    break;

	    case VK_IMAGE_LAYOUT_DEPTH_STENCIL_ATTACHMENT_OPTIMAL:
		    // Image layout will be used as a depth/stencil attachment
		    // Make sure any writes to depth/stencil buffer have been finished
		    barrier.dstAccessMask = barrier.dstAccessMask | VK_ACCESS_DEPTH_STENCIL_ATTACHMENT_WRITE_BIT;
		    break;

	    case VK_IMAGE_LAYOUT_SHADER_READ_ONLY_OPTIMAL:
		    // Image will be read in a shader (sampler, input attachment)
		    // Make sure any writes to the image have been finished
		    if (barrier.srcAccessMask == 0) {
			    barrier.srcAccessMask = VK_ACCESS_HOST_WRITE_BIT | VK_ACCESS_TRANSFER_WRITE_BIT;
		    }
		    barrier.dstAccessMask = VK_ACCESS_SHADER_READ_BIT;
		    break;
	    default:
		    // Other source layouts aren't handled (yet)
		    break;
	}

    vkCmdPipelineBarrier(
        commandBuffer,
        oldStage, newStage,
        0,
        0, nullptr,
        0, nullptr,
        1, &barrier
    );

    return EndAndSubmitOneTimeCommandBuffer(device, commandPool, queue, commandBuffer);
}

//---------------------------------------------------------------------------------------------------------------------

VkResult VulkanUtility::CopyImage(
    const VkDevice device, const VkCommandPool commandPool, const VkQueue queue,
    const VkImage srcImage, const VkImage dstImage,
    const uint32_t width, const uint32_t height) 
{
    VkCommandBuffer commandBuffer = VK_NULL_HANDLE;
    VULKAN_CHECK(BeginOneTimeCommandBufferInto(device, commandPool, &commandBuffer));

    //Start copy
	VkImageCopy copyRegion{};
	copyRegion.srcSubresource = { VK_IMAGE_ASPECT_COLOR_BIT, 0, 0, 1 };
	copyRegion.srcOffset = { 0, 0, 0 };
	copyRegion.dstSubresource = { VK_IMAGE_ASPECT_COLOR_BIT, 0, 0, 1 };
	copyRegion.dstOffset = { 0, 0, 0 };
	copyRegion.extent = { width, height, 1 };
	vkCmdCopyImage(commandBuffer,
        srcImage, VK_IMAGE_LAYOUT_TRANSFER_SRC_OPTIMAL,
        dstImage, VK_IMAGE_LAYOUT_TRANSFER_DST_OPTIMAL,
        1, &copyRegion);

    return EndAndSubmitOneTimeCommandBuffer(device,commandPool,queue,commandBuffer);
}

} // end namespace webrtc
} // end namespace unity
