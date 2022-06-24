#pragma once

#include "IGraphicsDevice.h"

namespace unity
{
namespace webrtc
{

    // Singleton
    class GraphicsDevice
    {
    public:
        static GraphicsDevice& GetInstance();
        IGraphicsDevice* Init(IUnityInterfaces* unityInterface, ProfilerMarkerFactory* profiler);
        IGraphicsDevice*
        Init(UnityGfxRenderer renderer, void* device, void* unityInterface, ProfilerMarkerFactory* profiler);

    private:
        GraphicsDevice();
        GraphicsDevice(GraphicsDevice const&) = delete;
        void operator=(GraphicsDevice const&) = delete;
    };

} // end namespace webrtc
} // end namespace unity
