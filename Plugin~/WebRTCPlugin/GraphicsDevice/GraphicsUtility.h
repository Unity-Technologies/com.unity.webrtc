#pragma once

#include <memory>

#include "IGraphicsDevice.h"

namespace unity
{
namespace webrtc
{

    // Vulkan owns only the descriptor allocated by AccessTexture, never Unity's VkImage.
    // Other graphics APIs return a borrowed Unity texture handle.
    using NativeGraphicsPtr = std::unique_ptr<void, void (*)(void*)>;

    class GraphicsUtility
    {
    public:
        static NativeGraphicsPtr
        TextureHandleToNativeGraphicsPtr(void* textureHandle, IGraphicsDevice* device, UnityGfxRenderer renderer);
    };

} // end namespace webrtc
} // end namespace unity
