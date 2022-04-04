#pragma once

#include <cuda.h>
#include "GpuMemoryBuffer.h"

namespace unity
{
namespace webrtc
{
    struct GpuMemoryBufferCudaHandle : public GpuMemoryBufferHandle
    {
        GpuMemoryBufferCudaHandle();
        GpuMemoryBufferCudaHandle(GpuMemoryBufferCudaHandle&& other);
        GpuMemoryBufferCudaHandle& operator=(GpuMemoryBufferCudaHandle&& other);
        virtual ~GpuMemoryBufferCudaHandle();

        CUarray array;
        CUarray mappedArray;
        CUdeviceptr mappedPtr;
        CUgraphicsResource resource;
        CUexternalMemory externalMemory;
    };
}
}
