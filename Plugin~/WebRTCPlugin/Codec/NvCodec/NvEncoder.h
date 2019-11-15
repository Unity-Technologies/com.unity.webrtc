#pragma once
#include <vector>
#include <thread>
#include <atomic>
#include "nvEncodeAPI.h"
#include "Codec/IEncoder.h"

namespace WebRTC
{
    enum class CodecInitializationResult
    {
        NotInitialized,
        Success,
        DriverNotInstalled,
        DriverVersionDoesNotSupportAPI,
        APINotFound,
        EncoderInitializationFailed
    };

    using OutputFrame = NV_ENC_OUTPUT_PTR;
    class IGraphicsDevice;
    class NvEncoder : public IEncoder
    {
    private:
        struct InputFrame
        {
            NV_ENC_REGISTERED_PTR registeredResource;
            NV_ENC_INPUT_PTR mappedResource;
            NV_ENC_BUFFER_FORMAT bufferFormat;
        };

        struct Frame
        {
            InputFrame inputFrame = {nullptr, nullptr, NV_ENC_BUFFER_FORMAT_UNDEFINED };
            OutputFrame outputFrame = nullptr;
            std::vector<uint8> encodedFrame = {};
            bool isIdrFrame = false;
            std::atomic<bool> isEncoding = { false };
        };
    public:
        NvEncoder(
            NV_ENC_DEVICE_TYPE type,
            NV_ENC_INPUT_RESOURCE_TYPE inputType,
            int width, int height, IGraphicsDevice* device);
        virtual ~NvEncoder();

        static CodecInitializationResult LoadCodec();
        static void UnloadCodec();

        void SetRate(uint32 rate) override;
        void UpdateSettings() override;
        bool CopyFrame(void* frame) override;
        void EncodeFrame() override;
        bool IsSupported() const override { return isNvEncoderSupported; }
        void SetIdrFrame()  override { isIdrFrame = true; }
        uint64 GetCurrentFrameCount()  override { return frameCount; }
    protected:
        int width = 1920;
        int height = 1080;
        int pitch = 0;
        IGraphicsDevice* m_device;
        NV_ENC_INPUT_RESOURCE_TYPE m_inputType;
    private:
        virtual void* AllocateInputBuffer() = 0;
        virtual void ReleaseInputBuffers() = 0;
        void InitEncoderResources();
        void ReleaseEncoderResources();
        void ReleaseFrameInputBuffer(Frame& frame);
        void ProcessEncodedFrame(Frame& frame);
        NV_ENC_REGISTERED_PTR RegisterResource(void *pBuffer);
        void MapResources(InputFrame& inputFrame);
        NV_ENC_OUTPUT_PTR InitializeBitstreamBuffer();
        NV_ENC_INITIALIZE_PARAMS nvEncInitializeParams = {};
        NV_ENC_CONFIG nvEncConfig = {};
        NVENCSTATUS errorCode;
        Frame bufferedFrames[bufferedFrameNum];
        void* renderTextures[bufferedFrameNum];
        static uint32_t GetNumChromaPlanes(NV_ENC_BUFFER_FORMAT);
        static uint32_t GetChromaHeight(const NV_ENC_BUFFER_FORMAT bufferFormat, const uint32_t lumaHeight);
        static uint32_t GetWidthInBytes(const NV_ENC_BUFFER_FORMAT bufferFormat, const uint32_t width);
        uint64 frameCount = 0;
        void* pEncoderInterface = nullptr;
        bool isNvEncoderSupported = false;
        bool isIdrFrame = false;
        //10Mbps
        int bitRate = 10000000;
        //100Mbps
        int lastBitRate = 100000000;
        //5Mbps
        const int minBitRate = 5000000;
        int frameRate = 45;
    };
}
