#pragma once

#define SAFE_DELETE(a)                                                                                                 \
    {                                                                                                                  \
        if ((a) != NULL)                                                                                               \
        {                                                                                                              \
            delete (a);                                                                                                \
            (a) = nullptr;                                                                                             \
        }                                                                                                              \
    }
#define SAFE_DELETE_ARR(a)                                                                                             \
    {                                                                                                                  \
        if ((a) != NULL)                                                                                               \
        {                                                                                                              \
            delete[](a);                                                                                               \
            (a) = nullptr;                                                                                             \
        }                                                                                                              \
    }

#define VULKAN_SAFE_DESTROY_IMAGE_VIEW(device, obj, allocator)                                                         \
    {                                                                                                                  \
        if (VK_NULL_HANDLE != obj)                                                                                     \
        {                                                                                                              \
            vkDestroyImageView(device, obj, allocator);                                                                \
            obj = VK_NULL_HANDLE;                                                                                      \
        }                                                                                                              \
    }

#define VULKAN_SAFE_DESTROY_COMMAND_POOL(device, obj, allocator)                                                       \
    {                                                                                                                  \
        if (VK_NULL_HANDLE != obj)                                                                                     \
        {                                                                                                              \
            vkDestroyCommandPool(device, obj, allocator);                                                              \
            obj = VK_NULL_HANDLE;                                                                                      \
        }                                                                                                              \
    }

#define VULKAN_CHECK(api)                                                                                              \
    {                                                                                                                  \
        const VkResult result = api;                                                                                   \
        if (VK_SUCCESS != result)                                                                                      \
        {                                                                                                              \
            return result;                                                                                             \
        }                                                                                                              \
    }

#define VULKAN_CHECK_FAILVALUE(api, failValue)                                                                         \
    {                                                                                                                  \
        const VkResult result = api;                                                                                   \
        if (VK_SUCCESS != result)                                                                                      \
        {                                                                                                              \
            return failValue;                                                                                          \
        }                                                                                                              \
    }

#define __VULKAN_API_CALL(call, ret)                                                                                   \
    VkResult err__ = call;                                                                                             \
    if (err__ != VK_SUCCESS)                                                                                           \
    {                                                                                                                  \
        RTC_LOG(LS_ERROR) << "Vulkan call failed, error: " << err__;                                                   \
        return ret;                                                                                                    \
    }

#define VULKAN_API_CALL(call)                                                                                          \
    do                                                                                                                 \
    {                                                                                                                  \
        __VULKAN_API_CALL(call, ;);                                                                                    \
    } while (0)

#define VULKAN_API_CALL_ERROR(call)                                                                                    \
    do                                                                                                                 \
    {                                                                                                                  \
        __VULKAN_API_CALL(call, err__);                                                                                \
    } while (0)

#define VULKAN_API_CALL_ARG(call, arg)                                                                                 \
    do                                                                                                                 \
    {                                                                                                                  \
        __VULKAN_API_CALL(call, arg);                                                                                  \
    } while (0)
