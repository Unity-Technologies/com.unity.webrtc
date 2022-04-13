#include "pch.h"

#include "GpuMemoryBufferCudaHandle.h"

namespace unity
{
namespace webrtc
{
    GpuMemoryBufferCudaHandle::GpuMemoryBufferCudaHandle()
        : array(nullptr)
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
        CUresult result;
        if (externalMemory != nullptr)
        {
            result = cuDestroyExternalMemory(externalMemory);
            if (result != CUDA_SUCCESS)
            {
                RTC_LOG(LS_ERROR) << "faild cuDestroyExternalMemory CUresult: " << result;
                throw;
            }
        }
        if (resource != nullptr)
        {
            result = cuGraphicsUnmapResources(1, &resource, 0);
            if (result != CUDA_SUCCESS)
            {
                RTC_LOG(LS_ERROR) << "faild cuGraphicsUnmapResources CUresult: " << result;
                throw;
            }
            result = cuGraphicsUnregisterResource(resource);
            if (result != CUDA_SUCCESS)
            {
                RTC_LOG(LS_ERROR) << "faild cuGraphicsUnregisterResource CUresult: " << result;
                throw;
            }
        }
        if (array != nullptr)
        {
            cuArrayDestroy(array);
            if (result != CUDA_SUCCESS)
            {
                RTC_LOG(LS_ERROR) << "faild cuArrayDestroy CUresult: " << result;
                throw;
            }
        }
    }
}
}
