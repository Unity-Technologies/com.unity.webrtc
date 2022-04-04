#include "pch.h"

#include "GpuMemoryBufferCudaHandle.h"

#include <nppi.h>

namespace unity
{
namespace webrtc
{
    GpuMemoryBufferCudaHandle::GpuMemoryBufferCudaHandle()
        : array(nullptr)
        , mappedArray(nullptr)
        , devicePtr(0)
        , resource(nullptr)
        , externalMemory(nullptr)
    {
    }

    GpuMemoryBufferCudaHandle::GpuMemoryBufferCudaHandle(GpuMemoryBufferCudaHandle&& other) = default;
    GpuMemoryBufferCudaHandle& GpuMemoryBufferCudaHandle::operator=(GpuMemoryBufferCudaHandle&& other) = default;

    GpuMemoryBufferCudaHandle::~GpuMemoryBufferCudaHandle()
    {
        if (externalMemory != nullptr)
        {
            cuDestroyExternalMemory(externalMemory);
        }
        if (resource != nullptr)
        {
            cuGraphicsUnmapResources(1, &resource, 0);
            cuGraphicsUnregisterResource(resource);
        }
        if (array != nullptr)
        {
            cuArrayDestroy(array);
        }
    }
}
}
