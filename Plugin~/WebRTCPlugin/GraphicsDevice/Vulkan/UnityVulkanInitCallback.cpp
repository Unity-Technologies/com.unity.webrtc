#include "pch.h"

#if defined(_WIN32)
#define VK_NO_PROTOTYPES
#include <vulkan/vulkan_win32.h>
#elif defined(__linux)
#include <dlfcn.h>
#endif

namespace unity
{
namespace webrtc
{

#define EXPORTED_VULKAN_FUNCTION(func) PFN_##func func;
#define GLOBAL_VULKAN_FUNCTION(func) PFN_##func func;
#define INSTANCE_VULKAN_FUNCTION(func) PFN_##func func;
#define DEVICE_VULKAN_FUNCTION(func) PFN_##func func;

#include "ListOfVulkanFunctions.inl"

bool LoadVulkanLibrary(LIBRARY_TYPE& library) {
#if defined(_WIN32)
    library = LoadLibrary("vulkan-1.dll");
#elif defined(__linux)
    library = dlopen("libvulkan.so.1", RTLD_NOW);
#endif
    if (library == nullptr)
        return false;
    return true;
}

bool LoadExportedVulkanFunction(LIBRARY_TYPE const& library) {
#if defined(_WIN32)
#define LoadFunction GetProcAddress
#elif defined(__linux)
#define LoadFunction dlsym
#endif

#define EXPORTED_VULKAN_FUNCTION( name ) \
    name = (PFN_##name)LoadFunction( library, #name ); \
    if (name == nullptr) { \
        return false; \
    }
#include "ListOfVulkanFunctions.inl"
    return true;
}

bool LoadGlobalVulkanFunction() {
#define GLOBAL_VULKAN_FUNCTION( name ) \
    name = (PFN_##name)vkGetInstanceProcAddr( nullptr, #name ); \
    if (name == nullptr) { \
        return false; \
    }
#include "ListOfVulkanFunctions.inl"
    return true;
}

bool LoadInstanceVulkanFunction(VkInstance instance) {
#define INSTANCE_VULKAN_FUNCTION( name ) \
    name = (PFN_##name)vkGetInstanceProcAddr( instance, #name ); \
    if (name == nullptr) { \
        return false; \
    }
#include "ListOfVulkanFunctions.inl"
    return true;
}

bool LoadDeviceVulkanFunction(VkDevice device) {
#define DEVICE_VULKAN_FUNCTION( name ) \
    name = (PFN_##name)vkGetDeviceProcAddr( device, #name ); \
    if (name == nullptr) { \
        return false; \
    }
#include "ListOfVulkanFunctions.inl"
    return true;
}


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
    for(uint32_t i = 0; i < pCreateInfo->enabledExtensionCount; i++)
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
    newCreateInfo.enabledExtensionCount = static_cast<uint32_t>(newExtensions.size());
    VkResult result = vkCreateDevice(physicalDevice, &newCreateInfo, pAllocator, pDevice);
    if(result != VK_SUCCESS)
    {
        RTC_LOG(LS_ERROR) << "vkCreateDevice:" << result;
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
        return VK_ERROR_DEVICE_LOST;

    // copy value 
    VkInstanceCreateInfo newCreateInfo = *pCreateInfo;

    // copy extension name list
    std::vector<const char*> enabledExtensions;
    enabledExtensions.reserve(pCreateInfo->enabledExtensionCount);
    for (uint32_t i = 0; i < pCreateInfo->enabledExtensionCount; i++)
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
    newCreateInfo.enabledExtensionCount = static_cast<uint32_t>(newExtensions.size());

    VkResult result = vkCreateInstance(&newCreateInfo, pAllocator, pInstance);
    if (result != VK_SUCCESS)
    {
        RTC_LOG(LS_ERROR) << "vkCreateInstance:" << result;
    }

    if (!LoadInstanceVulkanFunction(*pInstance))
        return VK_ERROR_DEVICE_LOST;
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
    vkGetInstanceProcAddr = getInstanceProcAddr;
    return Hook_vkGetInstanceProcAddr;
}

}
}
