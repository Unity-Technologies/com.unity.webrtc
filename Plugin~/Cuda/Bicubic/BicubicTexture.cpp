
#include "BicubicTexture.h"

#include <cuda_runtime.h>

typedef unsigned int uint;
typedef unsigned char uchar;

//extern "C" void initTexture(int imageWidth, int imageHeight, uchar *h_data);
//extern "C" void freeTexture();
extern "C" void render(int width, int height, float tx, float ty, float scale,
                       float cx, float cy, dim3 blockSize, dim3 gridSize,
                       int filter_mode, uchar4 *output);

namespace unity
{
namespace webrtc
{
    void Resize(const CUarray& src, CUarray& dst, int width, int height, FilterMode mode)
    {
        CUDA_ARRAY_DESCRIPTOR desc;
        cuArrayGetDescriptor(&desc, src);
        int cx = desc.Width * 0.5f;
        int cy = desc.Height * 0.5f;
        float scale = width / static_cast<float>(desc.Width);
        dim3 blockSize(16, 16);
        dim3 gridSize(width / blockSize.x, height / blockSize.y);

        CUDA_RESOURCE_DESC srcResDesc = {};
        srcResDesc.resType = CU_RESOURCE_TYPE_ARRAY;
        srcResDesc.res.array.hArray = src;

        CUDA_TEXTURE_DESC srcTexDesc = {};
//        texDesc.normalizedCoords = false;
        srcTexDesc.filterMode = CU_TR_FILTER_MODE_LINEAR;
        srcTexDesc.addressMode[0] = CU_TR_ADDRESS_MODE_CLAMP;
        srcTexDesc.addressMode[1] = CU_TR_ADDRESS_MODE_CLAMP;
//        texDesc.readMode = cudaReadModeNormalizedFloat;

        cudaTextureObject_t srcTexObj;
        CUresult result = cuTexObjectCreate(&srcTexObj, &srcResDesc, &srcTexDesc, nullptr);
        if (result != CUDA_SUCCESS)
        {
            return;
        }

        CUDA_RESOURCE_DESC dstResDesc = {};
        dstResDesc.resType = CU_RESOURCE_TYPE_ARRAY;
        dstResDesc.res.array.hArray = dst;

        CUDA_TEXTURE_DESC dstTexDesc = {};
        //        texDesc.normalizedCoords = false;
        dstTexDesc.filterMode = CU_TR_FILTER_MODE_LINEAR;
        dstTexDesc.addressMode[0] = CU_TR_ADDRESS_MODE_CLAMP;
        dstTexDesc.addressMode[1] = CU_TR_ADDRESS_MODE_CLAMP;
        //        texDesc.readMode = cudaReadModeNormalizedFloat;

        cudaTextureObject_t dstTexObj;
        result = cuTexObjectCreate(&dstTexObj, &dstResDesc, &srcTexDesc, nullptr);
        if (result != CUDA_SUCCESS)
        {
            return;
        }

        render(desc.Width, desc.Height, 0, 0, scale, cx, cy, blockSize, gridSize, mode, (uchar4*)dstTexObj);

        result = cuTexObjectDestroy(srcTexObj);
        if (result != CUDA_SUCCESS)
        {
            return;
        }
        result = cuTexObjectDestroy(dstTexObj);
        if (result != CUDA_SUCCESS)
        {
            return;
        }

    }
}
}

