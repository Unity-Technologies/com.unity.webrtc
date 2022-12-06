#include "pch.h"

#include "NvEncoder/NvEncoder.h"
#include "NvEncoder/NvEncoderCuda.h"
#include "NvEncoderCudaWithCUarray.h"

namespace unity
{
namespace webrtc
{

    static CUresult
    CreateCUarray(CUarray* pDstArray, uint32_t width, uint32_t height, CUarray_format format, int numChannels)
    {
        CUDA_ARRAY3D_DESCRIPTOR arrayDesc = CUDA_ARRAY3D_DESCRIPTOR();
        arrayDesc.Width = width;
        arrayDesc.Height = height;
        arrayDesc.Depth = 0; /* CUDA 2D arrays are defined to have depth 0 */
        arrayDesc.Format = format;
        arrayDesc.NumChannels = static_cast<uint32_t>(numChannels);
        arrayDesc.Flags = CUDA_ARRAY3D_SURFACE_LDST;

        return cuArray3DCreate(pDstArray, &arrayDesc);
    }

    NvEncoderCudaWithCUarray::NvEncoderCudaWithCUarray(
        CUcontext cuContext,
        uint32_t nWidth,
        uint32_t nHeight,
        NV_ENC_BUFFER_FORMAT eBufferFormat,
        uint32_t nExtraOutputDelay,
        bool bMotionEstimationOnly,
        bool bOutputInVideoMemory)
        : NvEncoder(
              NV_ENC_DEVICE_TYPE_CUDA,
              cuContext,
              nWidth,
              nHeight,
              eBufferFormat,
              nExtraOutputDelay,
              bMotionEstimationOnly,
              bOutputInVideoMemory)
        , m_cuContext(cuContext)
    {
        if (!m_hEncoder)
        {
            NVENC_THROW_ERROR("Encoder Initialization failed", NV_ENC_ERR_INVALID_DEVICE);
        }

        if (!m_cuContext)
        {
            NVENC_THROW_ERROR("Invalid Cuda Context", NV_ENC_ERR_INVALID_DEVICE);
        }
    }

    NvEncoderCudaWithCUarray::~NvEncoderCudaWithCUarray() { ReleaseCudaResources(); }

    void NvEncoderCudaWithCUarray::AllocateInputBuffers(int32_t numInputBuffers)
    {
        if (!IsHWEncoderInitialized())
        {
            NVENC_THROW_ERROR("Encoder intialization failed", NV_ENC_ERR_ENCODER_NOT_INITIALIZED);
        }

        // for MEOnly mode we need to allocate seperate set of buffers for reference frame
        int numCount = m_bMotionEstimationOnly ? 2 : 1;

        for (int count = 0; count < numCount; count++)
        {
            CUDA_DRVAPI_CALL(cuCtxPushCurrent(m_cuContext));
            std::vector<void*> inputFrames;
            for (int i = 0; i < numInputBuffers; i++)
            {
                CUarray frame;
                CUDA_DRVAPI_CALL(
                    CreateCUarray(&frame, GetMaxEncodeWidth(), GetMaxEncodeHeight(), CU_AD_FORMAT_UNSIGNED_INT32, 1));
                inputFrames.push_back(static_cast<void*>(frame));
            }
            CUDA_DRVAPI_CALL(cuCtxPopCurrent(NULL));

            int encodeWidth = static_cast<int>(GetMaxEncodeWidth());
            int encodeHeight = static_cast<int>(GetMaxEncodeHeight());
            int widthInBytes = static_cast<int>(GetWidthInBytes(GetPixelFormat(), GetMaxEncodeWidth()));

            RegisterInputResources(
                inputFrames,
                NV_ENC_INPUT_RESOURCE_TYPE_CUDAARRAY,
                encodeWidth,
                encodeHeight,
                widthInBytes,
                GetPixelFormat(),
                (count == 1) ? true : false);
        }
    }

    void NvEncoderCudaWithCUarray::ReleaseInputBuffers() { ReleaseCudaResources(); }

    void NvEncoderCudaWithCUarray::ReleaseCudaResources()
    {
        if (!m_hEncoder)
        {
            return;
        }

        if (!m_cuContext)
        {
            return;
        }

        UnregisterInputResources();

        cuCtxPushCurrent(m_cuContext);

        for (uint32_t i = 0; i < m_vInputFrames.size(); ++i)
        {
            if (m_vInputFrames[i].inputPtr)
            {
                cuMemFree(reinterpret_cast<CUdeviceptr>(m_vInputFrames[i].inputPtr));
            }
        }
        m_vInputFrames.clear();

        for (uint32_t i = 0; i < m_vReferenceFrames.size(); ++i)
        {
            if (m_vReferenceFrames[i].inputPtr)
            {
                cuMemFree(reinterpret_cast<CUdeviceptr>(m_vReferenceFrames[i].inputPtr));
            }
        }
        m_vReferenceFrames.clear();

        cuCtxPopCurrent(nullptr);
        m_cuContext = nullptr;
    }

    void NvEncoderCudaWithCUarray::CopyToDeviceFrame(
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
        bool bUnAlignedDeviceCopy,
        CUstream stream)
    {
        if (srcMemoryType != CU_MEMORYTYPE_HOST && srcMemoryType != CU_MEMORYTYPE_ARRAY)
        {
            NVENC_THROW_ERROR("Invalid source memory type for copy", NV_ENC_ERR_INVALID_PARAM);
        }

        CUDA_DRVAPI_CALL(cuCtxPushCurrent(device));

        uint32_t srcPitch =
            nSrcPitch ? nSrcPitch : NvEncoder::GetWidthInBytes(pixelFormat, static_cast<uint32_t>(width));
        CUDA_MEMCPY2D m = CUDA_MEMCPY2D();
        m.srcMemoryType = srcMemoryType;
        if (srcMemoryType == CU_MEMORYTYPE_HOST)
        {
            m.srcHost = pSrcArray;
        }
        else
        {
            m.srcArray = static_cast<CUarray>(pSrcArray);
        }
        m.srcPitch = srcPitch;
        m.dstMemoryType = CU_MEMORYTYPE_ARRAY;
        m.dstArray = pDstArray;
        m.dstPitch = dstPitch;
        m.WidthInBytes = NvEncoder::GetWidthInBytes(pixelFormat, static_cast<uint32_t>(width));
        m.Height = static_cast<size_t>(height);
        if (bUnAlignedDeviceCopy && srcMemoryType == CU_MEMORYTYPE_ARRAY)
        {
            CUDA_DRVAPI_CALL(cuMemcpy2DUnaligned(&m));
        }
        else
        {
            CUDA_DRVAPI_CALL(stream == NULL ? cuMemcpy2D(&m) : cuMemcpy2DAsync(&m, stream));
        }

        std::vector<uint32_t> srcChromaOffsets;
        NvEncoder::GetChromaSubPlaneOffsets(pixelFormat, srcPitch, static_cast<uint32_t>(height), srcChromaOffsets);
        uint32_t chromaHeight = NvEncoder::GetChromaHeight(pixelFormat, static_cast<uint32_t>(height));
        uint32_t destChromaPitch = NvEncoder::GetChromaPitch(pixelFormat, dstPitch);
        uint32_t srcChromaPitch = NvEncoder::GetChromaPitch(pixelFormat, srcPitch);
        uint32_t chromaWidthInBytes = NvEncoder::GetChromaWidthInBytes(pixelFormat, static_cast<uint32_t>(width));

        for (uint32_t i = 0; i < numChromaPlanes; ++i)
        {
            if (chromaHeight)
            {
                if (srcMemoryType == CU_MEMORYTYPE_HOST)
                {
                    m.srcHost = (static_cast<uint8_t*>(pSrcArray) + srcChromaOffsets[i]);
                }
                else
                {
                    m.srcArray = (CUarray)(static_cast<uint8_t*>(pSrcArray) + srcChromaOffsets[i]);
                }
                m.srcPitch = srcChromaPitch;

                m.dstArray = (CUarray)((uint8_t*)pDstArray + dstChromaOffsets[i]);
                m.dstPitch = destChromaPitch;
                m.WidthInBytes = chromaWidthInBytes;
                m.Height = chromaHeight;
                if (bUnAlignedDeviceCopy && srcMemoryType == CU_MEMORYTYPE_ARRAY)
                {
                    CUDA_DRVAPI_CALL(cuMemcpy2DUnaligned(&m));
                }
                else
                {
                    CUDA_DRVAPI_CALL(stream == NULL ? cuMemcpy2D(&m) : cuMemcpy2DAsync(&m, stream));
                }
            }
        }
        CUDA_DRVAPI_CALL(cuCtxPopCurrent(NULL));
    }

} // end namespace webrtc
} // end namespace unity
