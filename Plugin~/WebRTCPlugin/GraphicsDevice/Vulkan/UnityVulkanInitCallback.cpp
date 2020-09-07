#include "pch.h"
#include <IUnityGraphicsVulkan.h>

#if defined(_WIN32)
#include <vulkan/vulkan_win32.h>
#endif
#include "UnityVulkanInitCallback.h"

namespace unity
{
namespace webrtc
{

static PFN_vkGetInstanceProcAddr s_vkGetInstanceProcAddr;
static PFN_vkCreateInstance s_vkCreateInstance;

const std::vector<const char*> requestedInstanceExtensions =
{
    //VK_EXT_DEBUG_REPORT_EXTENSION_NAME,
    VK_KHR_GET_PHYSICAL_DEVICE_PROPERTIES_2_EXTENSION_NAME,
    VK_KHR_EXTERNAL_MEMORY_CAPABILITIES_EXTENSION_NAME,
    VK_KHR_EXTERNAL_SEMAPHORE_CAPABILITIES_EXTENSION_NAME
};

static std::vector<const char*> requestedDeviceExtensions =
{
    VK_KHR_EXTERNAL_MEMORY_EXTENSION_NAME,
    VK_KHR_EXTERNAL_SEMAPHORE_EXTENSION_NAME,
#ifndef _WIN32
    VK_KHR_EXTERNAL_MEMORY_FD_EXTENSION_NAME,
    VK_KHR_EXTERNAL_SEMAPHORE_FD_EXTENSION_NAME
#else
    VK_KHR_EXTERNAL_MEMORY_WIN32_EXTENSION_NAME,
    VK_KHR_EXTERNAL_SEMAPHORE_WIN32_EXTENSION_NAME
#endif
};

static VKAPI_ATTR VkResult VKAPI_CALL Hook_vkCreateDevice(
    VkPhysicalDevice physicalDevice, const VkDeviceCreateInfo* pCreateInfo,
    const VkAllocationCallbacks* pAllocator, VkDevice* pDevice)
{
    // copy value 
    VkDeviceCreateInfo newCreateInfo = *pCreateInfo;

    // copy extension name list
    std::vector<const char*> enabledExtensions;
    enabledExtensions.reserve(pCreateInfo->enabledExtensionCount);
    for(int i = 0; i < pCreateInfo->enabledExtensionCount; i++)
    {
        enabledExtensions.push_back(newCreateInfo.ppEnabledExtensionNames[i]);
    }

    // get the union of the two
    std::vector<const char*> newExtensions;
    std::set_union(
        requestedDeviceExtensions.begin(), requestedDeviceExtensions.end(),
        enabledExtensions.begin(), enabledExtensions.end(),
        std::inserter(newExtensions, std::end(newExtensions))
        );

    // replace extension name list
    newCreateInfo.ppEnabledExtensionNames = newExtensions.data();
    newCreateInfo.enabledExtensionCount = newExtensions.size();
    VkResult result = vkCreateDevice(physicalDevice, &newCreateInfo, pAllocator, pDevice);
    if(result != VK_SUCCESS)
    {
        RTC_LOG(LS_ERROR) << "vkCreateDevice:" << result;
    }
    return result;
}

static VKAPI_ATTR VkResult VKAPI_CALL Hook_vkCreateInstance(
    const VkInstanceCreateInfo* pCreateInfo, const VkAllocationCallbacks* pAllocator, VkInstance* pInstance)
{
    s_vkCreateInstance = reinterpret_cast<PFN_vkCreateInstance>(
        s_vkGetInstanceProcAddr(VK_NULL_HANDLE, "vkCreateInstance"));

    // copy value 
    VkInstanceCreateInfo newCreateInfo = *pCreateInfo;

    // copy extension name list
    std::vector<const char*> enabledExtensions;
    enabledExtensions.reserve(pCreateInfo->enabledExtensionCount);
    for (int i = 0; i < pCreateInfo->enabledExtensionCount; i++)
    {
        enabledExtensions.push_back(newCreateInfo.ppEnabledExtensionNames[i]);
    }

    // get the union of the two
    std::vector<const char*> newExtensions;
    std::set_union(
        requestedInstanceExtensions.begin(), requestedInstanceExtensions.end(),
        enabledExtensions.begin(), enabledExtensions.end(),
        std::inserter(newExtensions, std::end(newExtensions))
    );

    // replace extension name list
    newCreateInfo.ppEnabledExtensionNames = newExtensions.data();
    newCreateInfo.enabledExtensionCount = newExtensions.size();

    VkResult result = s_vkCreateInstance(&newCreateInfo, pAllocator, pInstance);
    if (result != VK_SUCCESS)
    {
        RTC_LOG(LS_ERROR) << "vkCreateInstance:" << result;
    }
    return result;
}

static VKAPI_ATTR PFN_vkVoidFunction VKAPI_CALL Hook_vkGetInstanceProcAddr(VkInstance instance, const char* funcName)
{
    if (!funcName)
        return nullptr;

    std::string strFuncName = funcName;

    if(strFuncName == "vkCreateInstance")
    {
        return reinterpret_cast<PFN_vkVoidFunction>(&Hook_vkCreateInstance);
    }
    if(strFuncName == "vkCreateDevice")
    {
        return reinterpret_cast<PFN_vkVoidFunction>(&Hook_vkCreateDevice);
    }
    return nullptr;
}

PFN_vkGetInstanceProcAddr InterceptVulkanInitialization(
    PFN_vkGetInstanceProcAddr getInstanceProcAddr, void* userdata)
{
    s_vkGetInstanceProcAddr = getInstanceProcAddr;
    return Hook_vkGetInstanceProcAddr;
}

}
}
