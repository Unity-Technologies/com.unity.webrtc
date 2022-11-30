/*
* Copyright 2017-2022 NVIDIA Corporation.  All rights reserved.
*
* Please refer to the NVIDIA end user license agreement (EULA) associated
* with this source code for terms and conditions that govern your use of
* this software. Any use, reproduction, disclosure, or distribution of
* this software and related documentation outside the terms of the EULA
* is strictly prohibited.
*
*/

#include <cuda_runtime.h>
#include <stdint.h>
#include <stdio.h>

static __global__ void ConvertUInt8ToUInt16Kernel(uint8_t *dpUInt8, uint16_t *dpUInt16, int nSrcPitch, int nDestPitch, int nWidth, int nHeight)
{
    int x = blockIdx.x * blockDim.x + threadIdx.x,
        y = blockIdx.y * blockDim.y + threadIdx.y;

    if (x >= nWidth || y >= nHeight)
    {
        return;
    }
    int destStrideInPixels = nDestPitch / (sizeof(uint16_t));
    *(uchar2 *)&dpUInt16[y * destStrideInPixels + x] = uchar2{ 0, dpUInt8[y * nSrcPitch + x] };
}

static __global__ void ConvertUInt16ToUInt8Kernel(uint16_t *dpUInt16, uint8_t *dpUInt8, int nSrcPitch, int nDestPitch, int nWidth, int nHeight)
{
    int x = blockIdx.x * blockDim.x + threadIdx.x,
        y = blockIdx.y * blockDim.y + threadIdx.y;

    if (x >= nWidth || y >= nHeight)
    {
        return;
    }
    int srcStrideInPixels = nSrcPitch / (sizeof(uint16_t));
    dpUInt8[y * nDestPitch + x] = ((uchar2 *)&dpUInt16[y * srcStrideInPixels + x])->y;
}

void ConvertUInt8ToUInt16(uint8_t *dpUInt8, uint16_t *dpUInt16, int nSrcPitch, int nDestPitch, int nWidth, int nHeight)
{
    dim3 blockSize(16, 16, 1);
    dim3 gridSize(((uint32_t)nWidth + blockSize.x - 1) / blockSize.x, ((uint32_t)nHeight + blockSize.y - 1) / blockSize.y, 1);
    ConvertUInt8ToUInt16Kernel <<< gridSize, blockSize >>>(dpUInt8, dpUInt16, nSrcPitch, nDestPitch, nWidth, nHeight);
}

void ConvertUInt16ToUInt8(uint16_t *dpUInt16, uint8_t *dpUInt8, int nSrcPitch, int nDestPitch, int nWidth, int nHeight)
{
    dim3 blockSize(16, 16, 1);
    dim3 gridSize(((uint32_t)nWidth + blockSize.x - 1) / blockSize.x, ((uint32_t)nHeight + blockSize.y - 1) / blockSize.y, 1);
    ConvertUInt16ToUInt8Kernel <<<gridSize, blockSize >>>(dpUInt16, dpUInt8, nSrcPitch, nDestPitch, nWidth, nHeight);
}
