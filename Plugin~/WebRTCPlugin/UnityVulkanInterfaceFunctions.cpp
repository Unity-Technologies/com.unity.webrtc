#include "pch.h"

#include "UnityVulkanInterfaceFunctions.h"

namespace unity
{
namespace webrtc
{
    std::unique_ptr<UnityGraphicsVulkan> UnityGraphicsVulkan::Get(IUnityInterfaces* unityInterfaces)
    {
        IUnityGraphicsVulkanV2* vulkanV2 = unityInterfaces->Get<IUnityGraphicsVulkanV2>();
        if (vulkanV2)
            return std::make_unique<UnityGraphicsVulkanImpl<IUnityGraphicsVulkanV2>>(vulkanV2);
        IUnityGraphicsVulkan* vulkan = unityInterfaces->Get<IUnityGraphicsVulkan>();
        if (vulkan)
            return std::make_unique<UnityGraphicsVulkanImpl<IUnityGraphicsVulkan>>(vulkan);
        return nullptr;
    }
}
}
