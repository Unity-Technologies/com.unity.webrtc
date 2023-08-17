#include "pch.h"

#include "IGraphicsDevice.h"
#include "ScopedGraphicsDeviceLock.h"
#include "WebRTCPlugin.h"

namespace unity
{
namespace webrtc
{
    ScopedGraphicsDeviceLock::ScopedGraphicsDeviceLock()
    {
        IGraphicsDevice* device = Plugin::GraphicsDevice();
        if (device)
            device->Enter();
    }

    ScopedGraphicsDeviceLock::~ScopedGraphicsDeviceLock()
    {
        IGraphicsDevice* device = Plugin::GraphicsDevice();
        if (device)
            device->Leave();
    }
}
}
