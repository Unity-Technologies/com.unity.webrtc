#pragma once
#include <vector>
#include <thread>
#include <atomic>
#include "nvEncodeAPI.h"
#include "Codec/IEncoder.h"

namespace WebRTC
{
    using OutputFrame = NV_ENC_OUTPUT_PTR;
    class ITexture2D;
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

        virtual void InitV() override;
        static CodecInitializationResult InitializationResult();
        static CodecInitializationResult LoadCodec();
        static void UnloadCodec();
        static uint32_t GetNumChromaPlanes(NV_ENC_BUFFER_FORMAT);
        static uint32_t GetChromaHeight(const NV_ENC_BUFFER_FORMAT bufferFormat, const uint32_t lumaHeight);
        static uint32_t GetWidthInBytes(const NV_ENC_BUFFER_FORMAT bufferFormat, const uint32_t width);

        void SetRate(uint32 rate) override;
        void UpdateSettings() override;
        bool CopyBuffer(void* frame) override;
        bool EncodeFrame() override;
        bool IsSupported() const override { return isNvEncoderSupported; }
        void SetIdrFrame()  override { isIdrFrame = true; }
        virtual uint64 GetCurrentFrameCount() const override { return frameCount; }
        CodecInitializationResult GetCodecInitializationResult() const override { return InitializationResult(); }
    protected:
        int width = 1920;
        int height = 1080;
        int pitch = 0;
        IGraphicsDevice* m_device;

        NV_ENC_DEVICE_TYPE m_deviceType;
        NV_ENC_INPUT_RESOURCE_TYPE m_inputType;

        bool isNvEncoderSupported = false;

        virtual void* AllocateInputResourceV(ITexture2D* tex) = 0;

    private:
        void InitEncoderResources();
        void ReleaseEncoderResources();

        void ReleaseFrameInputBuffer(Frame& frame);
        void ProcessEncodedFrame(Frame& frame);
        NV_ENC_REGISTERED_PTR RegisterResource(NV_ENC_INPUT_RESOURCE_TYPE type, void *pBuffer);
        void MapResources(InputFrame& inputFrame);
        NV_ENC_OUTPUT_PTR InitializeBitstreamBuffer();
        NV_ENC_INITIALIZE_PARAMS nvEncInitializeParams = {};
        NV_ENC_CONFIG nvEncConfig = {};
        NVENCSTATUS errorCode;
        Frame bufferedFrames[bufferedFrameNum];
        ITexture2D* renderTextures[bufferedFrameNum];
        uint64 frameCount = 0;
        void* pEncoderInterface = nullptr;
        bool isIdrFrame = false;
        //10Mbps
        uint32_t bitRate = 10000000;
        //100Mbps
        uint32_t lastBitRate = 100000000;
        //5Mbps
        const uint32_t minBitRate = 5000000;
        uint32_t frameRate = 45;
    };
}
