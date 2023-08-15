#pragma once

namespace unity
{
namespace webrtc
{
    class ScopedGraphicsDeviceLock
    {
    public:
        ScopedGraphicsDeviceLock();
        ~ScopedGraphicsDeviceLock();
    };
} // end namespace webrtc
} // end namespace unity
