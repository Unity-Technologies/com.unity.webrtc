#pragma once
#include <vector>
#include <rtc_base/timestamp_aligner.h>

#include "nvEncodeAPI.h"
#include "Codec/IEncoder.h"

namespace unity
{
namespace webrtc
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
        };
    public:
        NvEncoder(
            NV_ENC_DEVICE_TYPE type,
            NV_ENC_INPUT_RESOURCE_TYPE inputType,
            NV_ENC_BUFFER_FORMAT bufferFormat,
            int width,
            int height,
            IGraphicsDevice* device,
            UnityRenderingExtTextureFormat textureFormat);
        virtual ~NvEncoder();

        static CodecInitializationResult LoadCodec();
        static bool LoadModule();
        static bool CheckDriverVersion();
        static void UnloadModule();
        static uint32_t GetNumChromaPlanes(NV_ENC_BUFFER_FORMAT);
        static uint32_t GetChromaHeight(const NV_ENC_BUFFER_FORMAT bufferFormat, const uint32_t lumaHeight);
        static uint32_t GetWidthInBytes(const NV_ENC_BUFFER_FORMAT bufferFormat, const uint32_t width);

        void InitV() override;
        void SetRates(uint32_t bitRate, int64_t frameRate) override;
        void UpdateSettings() override;
        bool CopyBuffer(void* frame) override;
        bool EncodeFrame(int64_t timestamp_us) override;
        bool IsSupported() const override { return m_isNvEncoderSupported; }
        void SetIdrFrame()  override { isIdrFrame = true; }
        uint64 GetCurrentFrameCount() const override { return frameCount; }
    protected:
        int m_width;
        int m_height;
        IGraphicsDevice* m_device;
        UnityRenderingExtTextureFormat m_textureFormat;

        NV_ENC_DEVICE_TYPE m_deviceType;
        NV_ENC_INPUT_RESOURCE_TYPE m_inputType;
        NV_ENC_BUFFER_FORMAT m_bufferFormat;

        bool m_isNvEncoderSupported = false;

        virtual void* AllocateInputResourceV(ITexture2D* tex) = 0;
        virtual void ReleaseInputResourceV(void* pResource) = 0;

        void InitEncoderResources();
        void ReleaseEncoderResources();

    private:
        void ReleaseFrameInputBuffer(Frame& frame);
        void ProcessEncodedFrame(Frame& frame, int64_t timestamp_us);
        NV_ENC_REGISTERED_PTR RegisterResource(NV_ENC_INPUT_RESOURCE_TYPE type, void *pBuffer);
        void MapResources(InputFrame& inputFrame);
        NV_ENC_OUTPUT_PTR InitializeBitstreamBuffer();
        NV_ENC_INITIALIZE_PARAMS nvEncInitializeParams = {};
        NV_ENC_CONFIG nvEncConfig = {};
        NVENCSTATUS errorCode;
        Frame bufferedFrames[bufferedFrameNum];
        ITexture2D* m_renderTextures[bufferedFrameNum] = {};
        std::vector<void*> m_buffers;
        uint64 frameCount = 0;
        void* pEncoderInterface = nullptr;
        bool isIdrFrame = false;

        webrtc::Clock* m_clock;

        uint32_t m_frameRate = 30;
        uint32_t m_targetBitrate = 0;
        rtc::TimestampAligner timestamp_aligner_;
    };
    
} // end namespace webrtc
} // end namespace unity
