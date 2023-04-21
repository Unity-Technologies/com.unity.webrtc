#include "pch.h"

#include "GraphicsUtility.h"

namespace unity
{
namespace webrtc
{

    namespace webrtc = ::webrtc;

    void* GraphicsUtility::TextureHandleToNativeGraphicsPtr(
        void* textureHandle, IGraphicsDevice* device, UnityGfxRenderer renderer)
    {
        return textureHandle;
    }

} // end namespace webrtc
} // end namespace unity
