#include "pch.h"

#include "GraphicsUtility.h"

#if SUPPORT_VULKAN
#include "Vulkan/VulkanGraphicsDevice.h"
#endif

namespace unity
{
namespace webrtc
{

    namespace webrtc = ::webrtc;

    NativeGraphicsPtr GraphicsUtility::TextureHandleToNativeGraphicsPtr(
        void* textureHandle, IGraphicsDevice* device, UnityGfxRenderer renderer)
    {
#if SUPPORT_VULKAN
        if (renderer == kUnityGfxRendererVulkan)
        {
            VulkanGraphicsDevice* vulkanDevice = static_cast<VulkanGraphicsDevice*>(device);
            std::unique_ptr<UnityVulkanImage> unityVulkanImage = vulkanDevice->AccessTexture(textureHandle);
            return NativeGraphicsPtr(
                unityVulkanImage.release(), [](void* ptr) { delete static_cast<UnityVulkanImage*>(ptr); });
        }
#endif
        return NativeGraphicsPtr(textureHandle, [](void*) {});
    }

} // end namespace webrtc
} // end namespace unity
