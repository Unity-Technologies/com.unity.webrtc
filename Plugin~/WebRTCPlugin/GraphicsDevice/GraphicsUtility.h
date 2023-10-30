#pragma once

#include "IGraphicsDevice.h"

namespace unity
{
namespace webrtc
{

    class GraphicsUtility
    {
    public:
        static void*
        TextureHandleToNativeGraphicsPtr(void* textureHandle, IGraphicsDevice* device, UnityGfxRenderer renderer);
    };

} // end namespace webrtc
} // end namespace unity
