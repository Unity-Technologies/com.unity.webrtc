#pragma once

#include "IUnityGraphicsMetal.h"

#include <Metal/Metal.h>

namespace unity
{
namespace webrtc
{
    class MetalDevice
    {
    public:
        static std::unique_ptr<MetalDevice> Create(IUnityGraphicsMetal* device);
        static std::unique_ptr<MetalDevice> CreateForTest();

        virtual ~MetalDevice() { }
        virtual id<MTLDevice> Device() = 0;
        virtual id<MTLCommandBuffer> CurrentCommandBuffer() = 0;
        virtual id<MTLCommandEncoder> CurrentCommandEncoder() = 0;
        virtual void EndCurrentCommandEncoder() = 0;
    };
} // end namespace webrtc
} // end namespace unity
