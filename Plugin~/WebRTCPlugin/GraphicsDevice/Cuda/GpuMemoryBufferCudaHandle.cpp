#include "pch.h"

#include "GpuMemoryBufferCudaHandle.h"

namespace unity
{
namespace webrtc
{
    GpuMemoryBufferCudaHandle::GpuMemoryBufferCudaHandle()
        : context(nullptr)
        , mipmappedArray(nullptr)
        , mappedArray(nullptr)
        , mappedPtr(0)
        , resource(nullptr)
        , externalMemory(nullptr)
    {
    }

    GpuMemoryBufferCudaHandle::GpuMemoryBufferCudaHandle(GpuMemoryBufferCudaHandle&& other) = default;
    GpuMemoryBufferCudaHandle& GpuMemoryBufferCudaHandle::operator=(GpuMemoryBufferCudaHandle&& other) = default;

    GpuMemoryBufferCudaHandle::~GpuMemoryBufferCudaHandle()
    {
        GMB_CUDA_CALL(cuCtxPushCurrent(context));

        mappedArray = nullptr;
        if (mipmappedArray != nullptr)
        {
            GMB_CUDA_CALL(cuMipmappedArrayDestroy(mipmappedArray));
            mipmappedArray = nullptr;
        }
        if (externalMemory != nullptr)
        {
            GMB_CUDA_CALL(cuDestroyExternalMemory(externalMemory));
            externalMemory = nullptr;
        }
        if (resource != nullptr)
        {
            GMB_CUDA_CALL(cuGraphicsUnmapResources(1, &resource, nullptr));
            GMB_CUDA_CALL(cuGraphicsUnregisterResource(resource));
            resource = nullptr;
        }

        GMB_CUDA_CALL(cuCtxPopCurrent(nullptr));
    }
}
}
