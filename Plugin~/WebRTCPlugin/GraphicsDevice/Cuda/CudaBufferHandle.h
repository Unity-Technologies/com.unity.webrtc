#pragma once

#include <cuda.h>
#include "GpuMemoryBuffer.h"

namespace unity
{
namespace webrtc
{
    struct CudaBufferHandle : public GpuMemoryBufferHandle
    {
        CudaBufferHandle();
        CudaBufferHandle(CudaBufferHandle&& other);
        CudaBufferHandle& operator=(CudaBufferHandle&& other);
        virtual ~CudaBufferHandle();

        CUarray array;
        CUdeviceptr devicePtr;
        CUgraphicsResource resource;
        CUexternalMemory externalMemory;
    };
}
}
