#pragma once

#include <cuda.h>

#include "GpuMemoryBuffer.h"

#define __GMB_CUDA_CALL(call, ret)                                                                                     \
    CUresult err__ = call;                                                                                             \
    if (err__ != CUDA_SUCCESS)                                                                                         \
    {                                                                                                                  \
        const char* szErrName = NULL;                                                                                  \
        cuGetErrorName(err__, &szErrName);                                                                             \
        RTC_LOG(LS_ERROR) << "GpuMemoryBufferCudaHandle error " << szErrName;                                          \
        return ret;                                                                                                    \
    }

#define GMB_CUDA_CALL(call)                                                                                            \
    do                                                                                                                 \
    {                                                                                                                  \
        __GMB_CUDA_CALL(call, ;);                                                                                      \
    } while (0)

#define GMB_CUDA_CALL_ERROR(call)                                                                                      \
    do                                                                                                                 \
    {                                                                                                                  \
        __GMB_CUDA_CALL(call, err__);                                                                                  \
    } while (0)

#define GMB_CUDA_CALL_NULLPTR(call)                                                                                    \
    do                                                                                                                 \
    {                                                                                                                  \
        __GMB_CUDA_CALL(call, nullptr);                                                                                \
    } while (0)

#define GMB_CUDA_CALL_THROW(call)                                                                                      \
    do                                                                                                                 \
    {                                                                                                                  \
        __GMB_CUDA_CALL(call, throw);                                                                                  \
    } while (0)

namespace unity
{
namespace webrtc
{
    struct GpuMemoryBufferCudaHandle : public GpuMemoryBufferHandle
    {
        static std::unique_ptr<GpuMemoryBufferCudaHandle>
        CreateHandle(CUcontext context, CUdeviceptr ptr, AccessMode mode);

        GpuMemoryBufferCudaHandle();
        GpuMemoryBufferCudaHandle(GpuMemoryBufferCudaHandle&& other);
        GpuMemoryBufferCudaHandle& operator=(GpuMemoryBufferCudaHandle&& other);
        virtual ~GpuMemoryBufferCudaHandle() override;

        CUcontext context;
        CUmipmappedArray mipmappedArray;
        CUarray mappedArray;
        CUdeviceptr mappedPtr;
        CUgraphicsResource resource;
        CUexternalMemory externalMemory;
    };
}
}
