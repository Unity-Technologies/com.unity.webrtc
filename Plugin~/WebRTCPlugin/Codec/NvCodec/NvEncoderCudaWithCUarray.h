#pragma once

#include <cuda.h>
#include <mutex>
#include <stdint.h>

#include "NvEncoder/NvEncoder.h"

namespace unity
{
namespace webrtc
{

    /**
     *  @brief Encoder for CUDA device memory.
     */
    class NvEncoderCudaWithCUarray : public ::NvEncoder
    {
    public:
        NvEncoderCudaWithCUarray(
            CUcontext cuContext,
            uint32_t nWidth,
            uint32_t nHeight,
            NV_ENC_BUFFER_FORMAT eBufferFormat,
            uint32_t nExtraOutputDelay = 3,
            bool bMotionEstimationOnly = false,
            bool bOPInVideoMemory = false);
        virtual ~NvEncoderCudaWithCUarray() override;

        /**
         *  @brief This is a static function to copy input data from host memory to device memory.
         *  This function assumes YUV plane is a single contiguous memory segment.
         */
        static void CopyToDeviceFrame(
            CUcontext device,
            void* pSrcArray,
            uint32_t nSrcPitch,
            CUarray pDstArray,
            uint32_t dstPitch,
            int width,
            int height,
            CUmemorytype srcMemoryType,
            NV_ENC_BUFFER_FORMAT pixelFormat,
            const uint32_t dstChromaOffsets[],
            uint32_t numChromaPlanes,
            bool bUnAlignedDeviceCopy = false,
            CUstream stream = nullptr);

    protected:
        /**
         *  @brief This function is used to release the input buffers allocated for encoding.
         *  This function is an override of virtual function NvEncoder::ReleaseInputBuffers().
         */
        virtual void ReleaseInputBuffers() override;

    private:
        /**
         *  @brief This function is used to allocate input buffers for encoding.
         *  This function is an override of virtual function NvEncoder::AllocateInputBuffers().
         */
        virtual void AllocateInputBuffers(int32_t numInputBuffers) override;

    private:
        /**
         *  @brief This is a private function to release CUDA device memory used for encoding.
         */
        void ReleaseCudaResources();

    protected:
        CUcontext m_cuContext;
    };
} // end namespace webrtc
} // end namespace unity
