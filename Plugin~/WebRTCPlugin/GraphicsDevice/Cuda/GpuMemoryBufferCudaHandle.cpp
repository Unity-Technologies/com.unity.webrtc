#include "pch.h"

#include "GpuMemoryBufferCudaHandle.h"

namespace unity
{
namespace webrtc
{
    GpuMemoryBufferCudaHandle::GpuMemoryBufferCudaHandle()
        : context(nullptr)
        , array(nullptr)
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
        cuCtxPushCurrent(context);

        CUresult result;
        if (externalMemory != nullptr)
        {
            result = cuDestroyExternalMemory(externalMemory);
            if (result != CUDA_SUCCESS)
            {
                RTC_LOG(LS_ERROR) << "faild cuDestroyExternalMemory CUresult: " << result;
            }
        }
        if (resource != nullptr)
        {
            result = cuGraphicsUnmapResources(1, &resource, nullptr);
            if (result != CUDA_SUCCESS)
            {
                RTC_LOG(LS_ERROR) << "faild cuGraphicsUnmapResources CUresult: " << result;
            }
            result = cuGraphicsUnregisterResource(resource);
            if (result != CUDA_SUCCESS)
            {
                RTC_LOG(LS_ERROR) << "faild cuGraphicsUnregisterResource CUresult: " << result;
            }
        }
        if (array != nullptr)
        {
            result = cuArrayDestroy(array);
            if (result != CUDA_SUCCESS)
            {
                RTC_LOG(LS_ERROR) << "faild cuArrayDestroy CUresult: " << result;
            }
        }

        cuCtxPopCurrent(nullptr);
    }
}
}
