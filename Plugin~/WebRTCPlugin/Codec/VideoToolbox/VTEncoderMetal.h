#pragma once
#include "Codec/IEncoder.h"
#include "GraphicsDevice/IGraphicsDevice.h"
#include <VideoToolbox/VideoToolbox.h>

namespace unity
{
namespace webrtc
{

    class VTEncoderMetal : public IEncoder{
    public:
        VTEncoderMetal(uint32_t nWidth, uint32_t nHeight, IGraphicsDevice* device);
        ~VTEncoderMetal();
        void SetRates(const webrtc::VideoEncoder::RateControlParameters& parameters) override;
        void UpdateSettings() override;
        bool CopyBuffer(void* frame) override;
        bool EncodeFrame() override;
        bool IsSupported() const override;
        void SetIdrFrame() override;
        uint64 GetCurrentFrameCount() const override { return frameCount; }
        
    private:
        uint64 frameCount = 0;
        uint64 m_width = 0;
        uint64 m_height = 0;
        IGraphicsDevice* m_device;
        ITexture2D* renderTextures[bufferedFrameNum];
        CVPixelBufferRef pixelBuffers[bufferedFrameNum];
        std::vector<uint8> encodedBuffers[bufferedFrameNum];

        VTCompressionSessionRef encoderSession;
    };
    
} // end namespace webrtc
} // end namespace unity
