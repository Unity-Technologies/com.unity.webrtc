#pragma once

#define SAFE_DELETE(a)      { if( (a) != NULL ) { delete     (a); (a) = nullptr; } }  
#define SAFE_DELETE_ARR(a)  { if( (a) != NULL ) { delete[]   (a); (a) = nullptr; } }

#define VULKAN_SAFE_DESTROY_IMAGE_VIEW(device, obj, allocator) { \
    if (VK_NULL_HANDLE != obj) { \
        vkDestroyImageView(device, obj, allocator); \
        obj = VK_NULL_HANDLE; \
    } \
}

#define VULKAN_SAFE_DESTROY_COMMAND_POOL(device, obj, allocator) { \
    if (VK_NULL_HANDLE != obj) { \
        vkDestroyCommandPool(device, obj, allocator); \
        obj = VK_NULL_HANDLE; \
    } \
}

#define VULKAN_CHECK(api) { \
    const VkResult result = api; \
    if (VK_SUCCESS != result) { \
        return result; \
    } \
}

#define VULKAN_CHECK_FAILVALUE(api, failValue) { \
    const VkResult result = api; \
    if (VK_SUCCESS != result) { \
        return failValue; \
    } \
}
