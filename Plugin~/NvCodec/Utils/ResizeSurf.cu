#include "ResizeSurf.h"

#include <cuda_runtime.h>

__global__ void ResizeSurfNearestNeighborKernel(
    CUsurfObject srcSurface,
    int srcWidth,
    int srcHeight,
    CUsurfObject dstSurface,
    int dstWidth,
    int dstHeight,
    float scaleWidth,
    float scaleHeight)
{
    // calculate surface coordinates
    int x = blockIdx.x * blockDim.x + threadIdx.x;
    int y = blockIdx.y * blockDim.y + threadIdx.y;

    if (x >= dstWidth || y >= dstHeight)
        return;

    uchar4 data;
    surf2Dread(&data, srcSurface, (x * scaleWidth) * 4, y * scaleHeight);

    // read from global memory and write to cuarray (via surface reference)
    surf2Dwrite(data, dstSurface, x * 4, y);
}

__global__ void ResizeSurfBilinearKernel(
    CUsurfObject srcSurface,
    int srcWidth,
    int srcHeight,
    CUsurfObject dstSurface,
    int dstWidth,
    int dstHeight,
    float scaleWidth,
    float scaleHeight)
{
    // calculate surface coordinates
    int x = blockIdx.x * blockDim.x + threadIdx.x;
    int y = blockIdx.y * blockDim.y + threadIdx.y;

    if (x >= dstWidth || y >= dstHeight)
        return;

    int x0 = (x * scaleWidth) - 1;
    int x1 = (x * scaleWidth) + 1;
    int y0 = (y * scaleHeight) - 1;
    int y1 = (y * scaleHeight) + 1;

    if (x0 < 0)
        x0 = 0;
    if (x1 >= srcWidth)
        x1 = srcWidth - 1;
    if (y0 < 0)
        y0 = 0;
    if (y1 >= srcHeight)
        y1 = srcHeight - 1;

    uchar4 c00, c01, c10, c11;
    surf2Dread(&c00, srcSurface, x0 * 4, y0);
    surf2Dread(&c01, srcSurface, x0 * 4, y1);
    surf2Dread(&c10, srcSurface, x1 * 4, y0);
    surf2Dread(&c11, srcSurface, x1 * 4, y1);

    uchar4 data;
    data.x = 0.5f * (0.5f * c00.x + 0.5f * c01.x) + 0.5f * (0.5f * c10.x + 0.5f * c11.x);
    data.y = 0.5f * (0.5f * c00.y + 0.5f * c01.y) + 0.5f * (0.5f * c10.y + 0.5f * c11.y);
    data.z = 0.5f * (0.5f * c00.z + 0.5f * c01.z) + 0.5f * (0.5f * c10.z + 0.5f * c11.z);

    surf2Dwrite(data, dstSurface, x * 4, y);
}

CUresult ResizeSurf(CUarray srcArray, CUarray dstArray)
{
    CUsurfObject srcSurface;
    CUDA_RESOURCE_DESC srcResDesc;
    srcResDesc.flags = 0;
    srcResDesc.resType = CU_RESOURCE_TYPE_ARRAY;
    srcResDesc.res.array.hArray = srcArray;

    CUresult result = cuSurfObjectCreate(&srcSurface, &srcResDesc);
    if (result != CUDA_SUCCESS)
        return result;

    CUsurfObject dstSurface;
    CUDA_RESOURCE_DESC dstResDesc;
    dstResDesc.flags = 0;
    dstResDesc.resType = CU_RESOURCE_TYPE_ARRAY;
    dstResDesc.res.array.hArray = dstArray;

    result = cuSurfObjectCreate(&dstSurface, &dstResDesc);
    if (result != CUDA_SUCCESS)
        return result;

    CUDA_ARRAY_DESCRIPTOR srcArrayDesc;
    result = cuArrayGetDescriptor(&srcArrayDesc, srcArray);
    if (result != CUDA_SUCCESS)
        return result;

    CUDA_ARRAY_DESCRIPTOR dstArrayDesc;
    result = cuArrayGetDescriptor(&dstArrayDesc, dstArray);
    if (result != CUDA_SUCCESS)
        return result;

    int srcWidth = srcArrayDesc.Width;
    int srcHeight = srcArrayDesc.Height;
    int dstWidth = dstArrayDesc.Width;
    int dstHeight = dstArrayDesc.Height;

    dim3 dimBlock(8, 8, 1);

    int gridX = dstWidth / dimBlock.x + (dstWidth % dimBlock.x ? 1 : 0);
    int gridY = dstHeight / dimBlock.y + (dstHeight % dimBlock.y ? 1 : 0);
    dim3 dimGrid(gridX, gridY, 1);

    auto resize_kernel = ResizeSurfBilinearKernel;
    // auto resize_kernel = ResizeSurfNearestNeighborKernel;

    resize_kernel<<<dimGrid, dimBlock>>>(
        srcSurface,
        srcWidth,
        srcHeight,
        dstSurface,
        dstWidth,
        dstHeight,
        (float)srcWidth / (float)dstWidth,
        (float)srcHeight / (float)dstHeight);

    result = cuSurfObjectDestroy(srcSurface);
    if (result != CUDA_SUCCESS)
        return result;
    return cuSurfObjectDestroy(dstSurface);
}
