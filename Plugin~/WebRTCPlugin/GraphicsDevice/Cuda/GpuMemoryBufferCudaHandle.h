#pragma once

#if _WIN32
#include <d3d11.h>
#include <d3d12.h>
#endif

#ifdef __linux__
#include <cudaGL.h>
#include <glad/gl.h>
#endif

#include <cuda.h>
#include <vulkan/vulkan.h>

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

#if _WIN32
        static std::unique_ptr<GpuMemoryBufferCudaHandle> CreateHandle(CUcontext context, ID3D11Resource* resource);
        static std::unique_ptr<GpuMemoryBufferCudaHandle>
        CreateHandle(CUcontext context, ID3D12Resource* resource, HANDLE sharedHandle, size_t memorySize);
#endif
        static std::unique_ptr<GpuMemoryBufferCudaHandle>
        CreateHandle(CUcontext context, void* exportHandle, size_t memorySize, const Size& size);
#ifdef __linux__
        static std::unique_ptr<GpuMemoryBufferCudaHandle> CreateHandle(CUcontext context, GLuint texture);
#endif
    };
}
}
