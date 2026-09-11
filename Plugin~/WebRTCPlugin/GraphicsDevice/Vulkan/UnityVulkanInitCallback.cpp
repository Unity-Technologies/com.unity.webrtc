#include "pch.h"

#include "UnityVulkanInitCallback.h"
#include <cstring>
#include <set>

namespace unity
{
namespace webrtc
{
    const std::vector<const char*> requestedInstanceExtensions = {
        VK_KHR_GET_PHYSICAL_DEVICE_PROPERTIES_2_EXTENSION_NAME,
        VK_KHR_EXTERNAL_MEMORY_CAPABILITIES_EXTENSION_NAME,
        VK_KHR_EXTERNAL_SEMAPHORE_CAPABILITIES_EXTENSION_NAME
    };

    static std::vector<const char*> requestedDeviceExtensions = { VK_KHR_EXTERNAL_MEMORY_EXTENSION_NAME,
                                                                  VK_KHR_EXTERNAL_SEMAPHORE_EXTENSION_NAME,
#ifdef UNITY_LINUX
                                                                  VK_KHR_EXTERNAL_MEMORY_FD_EXTENSION_NAME,
                                                                  VK_KHR_EXTERNAL_SEMAPHORE_FD_EXTENSION_NAME
#elif UNITY_WIN
                                                                  VK_KHR_EXTERNAL_MEMORY_WIN32_EXTENSION_NAME,
                                                                  VK_KHR_EXTERNAL_SEMAPHORE_WIN32_EXTENSION_NAME
#endif
    };

    namespace
    {
        // Compare type to use const char* in set
        struct StrCompare {
            bool operator()(const char* a, const char* b) const {
                return std::strcmp(a, b) < 0;
            }
        };
    }

    static VKAPI_ATTR VkResult VKAPI_CALL Hook_vkCreateDevice(
        VkPhysicalDevice physicalDevice,
        const VkDeviceCreateInfo* pCreateInfo,
        const VkAllocationCallbacks* pAllocator,
        VkDevice* pDevice)
    {
        // copy value
        VkDeviceCreateInfo newCreateInfo = *pCreateInfo;

        // Use a set to prevent duplicate extensions
        // Add extensions from CreateInfo
        std::set<const char*, StrCompare> uniqueExtensions(newCreateInfo.ppEnabledExtensionNames,
            newCreateInfo.ppEnabledExtensionNames + newCreateInfo.enabledExtensionCount);

        // Add additional requested extensions
        uniqueExtensions.insert(requestedDeviceExtensions.begin(), requestedDeviceExtensions.end());

        // Use vector for continuous memory layout
        std::vector<const char*> newExtensions(uniqueExtensions.begin(), uniqueExtensions.end());

        RTC_LOG(LS_INFO) << "WebRTC plugin intercepts vkCreateDevice.";

        for (auto extension : newExtensions)
        {
            RTC_LOG(LS_INFO) << "[Vulkan init intercept] extensions: name=" << extension;
        }

        // replace extension name list
        newCreateInfo.ppEnabledExtensionNames = newExtensions.data();
        newCreateInfo.enabledExtensionCount = static_cast<uint32_t>(newExtensions.size());
        VkResult result = vkCreateDevice(physicalDevice, &newCreateInfo, pAllocator, pDevice);
        if (result != VK_SUCCESS)
        {
            RTC_LOG(LS_ERROR) << "vkCreateDevice failed. error:" << result;
            return result;
        }
        if (!LoadDeviceVulkanFunction(*pDevice))
            return VK_ERROR_INITIALIZATION_FAILED;
        return result;
    }

    static VKAPI_ATTR VkResult VKAPI_CALL Hook_vkCreateInstance(
        const VkInstanceCreateInfo* pCreateInfo, const VkAllocationCallbacks* pAllocator, VkInstance* pInstance)
    {
        if (!LoadGlobalVulkanFunction())
            return VK_ERROR_INITIALIZATION_FAILED;

        // copy value
        VkInstanceCreateInfo newCreateInfo = *pCreateInfo;

        // Use a set to prevent duplicate extensions
        // Add extensions from CreateInfo
        std::set<const char*, StrCompare> uniqueExtensions(newCreateInfo.ppEnabledExtensionNames,
            newCreateInfo.ppEnabledExtensionNames + newCreateInfo.enabledExtensionCount);

        // Add additional requested extensions
        uniqueExtensions.insert(requestedInstanceExtensions.begin(), requestedInstanceExtensions.end());

        // Use vector for continuous memory layout
        std::vector<const char*> newExtensions(uniqueExtensions.begin(), uniqueExtensions.end());

        RTC_LOG(LS_INFO) << "WebRTC plugin intercepts vkCreateInstance.";

        for (auto extension : newExtensions)
        {
            RTC_LOG(LS_INFO) << "[Vulkan init intercept] extensions: name=" << extension;
        }

        // replace extension name list
        newCreateInfo.ppEnabledExtensionNames = newExtensions.data();
        newCreateInfo.enabledExtensionCount = static_cast<uint32_t>(newExtensions.size());

        VkResult result = vkCreateInstance(&newCreateInfo, pAllocator, pInstance);
        if (result != VK_SUCCESS)
        {
            RTC_LOG(LS_ERROR) << "vkCreateInstance failed. error:" << result;
            return result;
        }

        if (!LoadInstanceVulkanFunction(*pInstance))
            return VK_ERROR_INITIALIZATION_FAILED;
        return result;
    }

    static VKAPI_ATTR PFN_vkVoidFunction VKAPI_CALL
    Hook_vkGetInstanceProcAddr(VkInstance instance, const char* funcName)
    {
        if (!funcName)
            return nullptr;

        std::string strFuncName = funcName;

        if (strFuncName == "vkCreateInstance")
        {
            return reinterpret_cast<PFN_vkVoidFunction>(&Hook_vkCreateInstance);
        }
        if (strFuncName == "vkCreateDevice")
        {
            return reinterpret_cast<PFN_vkVoidFunction>(&Hook_vkCreateDevice);
        }
        return vkGetInstanceProcAddr(instance, funcName);
    }

    PFN_vkGetInstanceProcAddr
    InterceptVulkanInitialization(PFN_vkGetInstanceProcAddr getInstanceProcAddr, void* userdata)
    {
        vkGetInstanceProcAddr = getInstanceProcAddr;
        return Hook_vkGetInstanceProcAddr;
    }

}
}
