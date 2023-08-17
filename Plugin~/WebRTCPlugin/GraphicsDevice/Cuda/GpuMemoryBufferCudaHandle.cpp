#include "pch.h"

#include "GpuMemoryBufferCudaHandle.h"
#include "GraphicsDevice/ScopedGraphicsDeviceLock.h"

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
        ScopedGraphicsDeviceLock guard;
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

#if _WIN32
    std::unique_ptr<GpuMemoryBufferCudaHandle>
    GpuMemoryBufferCudaHandle::CreateHandle(CUcontext context, ID3D11Resource* resource)
    {
        ScopedGraphicsDeviceLock guard;
        GMB_CUDA_CALL_NULLPTR(cuCtxPushCurrent(context));

        std::unique_ptr<GpuMemoryBufferCudaHandle> handle = std::make_unique<GpuMemoryBufferCudaHandle>();
        handle->context = context;

        GMB_CUDA_CALL_NULLPTR(
            cuGraphicsD3D11RegisterResource(&handle->resource, resource, CU_GRAPHICS_REGISTER_FLAGS_SURFACE_LDST));
        GMB_CUDA_CALL_NULLPTR(cuGraphicsMapResources(1, &handle->resource, nullptr));
        GMB_CUDA_CALL_NULLPTR(cuGraphicsSubResourceGetMappedArray(&handle->mappedArray, handle->resource, 0, 0));
        GMB_CUDA_CALL_NULLPTR(cuCtxPopCurrent(nullptr));

        return handle;
    }

    std::unique_ptr<GpuMemoryBufferCudaHandle> GpuMemoryBufferCudaHandle::CreateHandle(
        CUcontext context, ID3D12Resource* resource, HANDLE sharedHandle, size_t memorySize)
    {
        ScopedGraphicsDeviceLock guard;
        GMB_CUDA_CALL_NULLPTR(cuCtxPushCurrent(context));

        std::unique_ptr<GpuMemoryBufferCudaHandle> handle = std::make_unique<GpuMemoryBufferCudaHandle>();
        handle->context = context;

        D3D12_RESOURCE_DESC desc = resource->GetDesc();
        size_t width = desc.Width;
        size_t height = desc.Height;

        CUDA_EXTERNAL_MEMORY_HANDLE_DESC memDesc = {};
        memDesc.type = CU_EXTERNAL_MEMORY_HANDLE_TYPE_D3D12_RESOURCE;
        memDesc.handle.win32.handle = static_cast<void*>(sharedHandle);
        memDesc.size = memorySize;
        memDesc.flags = CUDA_EXTERNAL_MEMORY_DEDICATED;

        GMB_CUDA_CALL_NULLPTR(cuImportExternalMemory(&handle->externalMemory, &memDesc));

        CUDA_ARRAY3D_DESCRIPTOR arrayDesc = {};
        arrayDesc.Width = width;
        arrayDesc.Height = height;
        arrayDesc.Depth = 0; /* CUDA 2D arrays are defined to have depth 0 */
        arrayDesc.Format = CU_AD_FORMAT_UNSIGNED_INT32;
        arrayDesc.NumChannels = 1;
        arrayDesc.Flags = CUDA_ARRAY3D_SURFACE_LDST | CUDA_ARRAY3D_COLOR_ATTACHMENT;

        CUDA_EXTERNAL_MEMORY_MIPMAPPED_ARRAY_DESC mipmapArrayDesc = {};
        mipmapArrayDesc.arrayDesc = arrayDesc;
        mipmapArrayDesc.numLevels = 1;

        GMB_CUDA_CALL_NULLPTR(
            cuExternalMemoryGetMappedMipmappedArray(&handle->mipmappedArray, handle->externalMemory, &mipmapArrayDesc));
        GMB_CUDA_CALL_NULLPTR(cuMipmappedArrayGetLevel(&handle->mappedArray, handle->mipmappedArray, 0));
        GMB_CUDA_CALL_NULLPTR(cuCtxPopCurrent(nullptr));

        return handle;
    }
#endif

    std::unique_ptr<GpuMemoryBufferCudaHandle>
    GpuMemoryBufferCudaHandle::CreateHandle(CUcontext context, void* exportHandle, size_t memorySize, const Size& size)
    {
        ScopedGraphicsDeviceLock guard;
        GMB_CUDA_CALL_NULLPTR(cuCtxPushCurrent(context));

        std::unique_ptr<GpuMemoryBufferCudaHandle> handle = std::make_unique<GpuMemoryBufferCudaHandle>();
        handle->context = context;

        CUDA_EXTERNAL_MEMORY_HANDLE_DESC memDesc = {};
#if _WIN32
        memDesc.type = CU_EXTERNAL_MEMORY_HANDLE_TYPE_OPAQUE_WIN32;
#else
        memDesc.type = CU_EXTERNAL_MEMORY_HANDLE_TYPE_OPAQUE_FD;
#endif
        memDesc.handle.fd = static_cast<int>(reinterpret_cast<uintptr_t>(exportHandle));
        memDesc.size = memorySize;

        GMB_CUDA_CALL_NULLPTR(cuImportExternalMemory(&handle->externalMemory, &memDesc));

        CUDA_ARRAY3D_DESCRIPTOR arrayDesc = {};
        arrayDesc.Width = static_cast<size_t>(size.width());
        arrayDesc.Height = static_cast<size_t>(size.height());
        arrayDesc.Depth = 0; /* CUDA 2D arrays are defined to have depth 0 */
        arrayDesc.Format = CU_AD_FORMAT_UNSIGNED_INT32;
        arrayDesc.NumChannels = 1;
        arrayDesc.Flags = CUDA_ARRAY3D_SURFACE_LDST | CUDA_ARRAY3D_COLOR_ATTACHMENT;

        CUDA_EXTERNAL_MEMORY_MIPMAPPED_ARRAY_DESC mipmapArrayDesc = {};
        mipmapArrayDesc.arrayDesc = arrayDesc;
        mipmapArrayDesc.numLevels = 1;

        GMB_CUDA_CALL_NULLPTR(
            cuExternalMemoryGetMappedMipmappedArray(&handle->mipmappedArray, handle->externalMemory, &mipmapArrayDesc));
        GMB_CUDA_CALL_NULLPTR(cuMipmappedArrayGetLevel(&handle->mappedArray, handle->mipmappedArray, 0));
        GMB_CUDA_CALL_NULLPTR(cuCtxPopCurrent(nullptr));

        return handle;
    }

#ifdef __linux__
    std::unique_ptr<GpuMemoryBufferCudaHandle>
    GpuMemoryBufferCudaHandle::CreateHandle(CUcontext context, GLuint texture)
    {
        ScopedGraphicsDeviceLock guard;
        GMB_CUDA_CALL_NULLPTR(cuCtxPushCurrent(context));

        std::unique_ptr<GpuMemoryBufferCudaHandle> handle = std::make_unique<GpuMemoryBufferCudaHandle>();
        handle->context = context;

        GMB_CUDA_CALL_NULLPTR(cuGraphicsGLRegisterImage(
            &handle->resource, texture, GL_TEXTURE_2D, CU_GRAPHICS_REGISTER_FLAGS_SURFACE_LDST));
        GMB_CUDA_CALL_NULLPTR(cuGraphicsMapResources(1, &handle->resource, 0));
        GMB_CUDA_CALL_NULLPTR(cuGraphicsSubResourceGetMappedArray(&handle->mappedArray, handle->resource, 0, 0));
        GMB_CUDA_CALL_NULLPTR(cuCtxPopCurrent(nullptr));

        return handle;
    }
#endif
}
}
