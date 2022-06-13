#include "pch.h"

#include "GraphicsUtility.h"

#if defined(SUPPORT_VULKAN)
#include "Vulkan/VulkanGraphicsDevice.h"
#endif

namespace unity
{
namespace webrtc
{

    namespace webrtc = ::webrtc;

    void* GraphicsUtility::TextureHandleToNativeGraphicsPtr(
        void* textureHandle, IGraphicsDevice* device, UnityGfxRenderer renderer)
    {
#if defined(SUPPORT_VULKAN)
        if (renderer == kUnityGfxRendererVulkan)
        {
            VulkanGraphicsDevice* vulkanDevice = static_cast<VulkanGraphicsDevice*>(device);
            std::unique_ptr<UnityVulkanImage> unityVulkanImage = vulkanDevice->AccessTexture(textureHandle);
            return unityVulkanImage.release();
        }
#endif
        return textureHandle;
    }

} // end namespace webrtc
} // end namespace unity
