#pragma once

#define SAFE_DELETE(a)      { if( (a) != NULL ) { delete     (a); (a) = nullptr; } }  
#define SAFE_DELETE_ARR(a)  { if( (a) != NULL ) { delete[]   (a); (a) = nullptr; } }

#define VULKAN_SAFE_DESTROY_IMAGE_VIEW(device, obj, allocator) { \
    if (VK_NULL_HANDLE != obj) { \
        vkDestroyImageView(device, obj, allocator); \
        obj = VK_NULL_HANDLE; \
    } \
}

#define VULKAN_SAFE_DESTROY_IMAGE(device, obj, allocator) { \
    if (VK_NULL_HANDLE!=obj) { \
        vkDestroyImage(device, obj, allocator); \
        obj = VK_NULL_HANDLE; \
    } \
}

#define VULKAN_SAFE_FREE_MEMORY(device, obj, allocator) { \
    if (VK_NULL_HANDLE!=obj) { \
        vkFreeMemory(device, obj, allocator); \
        obj = VK_NULL_HANDLE; \
    } \
}

#define VULKAN_SAFE_DESTROY_COMMAND_POOL(device, obj, allocator) { \
    if (VK_NULL_HANDLE != obj) { \
        vkDestroyCommandPool(device, obj, allocator); \
        obj = VK_NULL_HANDLE; \
    } \
}
