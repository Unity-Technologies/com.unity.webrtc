#include "pch.h"

#include "CudaBufferHandle.h"

namespace unity
{
namespace webrtc
{
    CudaBufferHandle::CudaBufferHandle()
        : array(nullptr)
        , devicePtr(0)
        , resource(nullptr)
        , externalMemory(nullptr)
    {
    }

    CudaBufferHandle::CudaBufferHandle(CudaBufferHandle&& other) = default;
    CudaBufferHandle& CudaBufferHandle::operator=(CudaBufferHandle&& other) = default;

    CudaBufferHandle::~CudaBufferHandle()
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
    }
}
}
