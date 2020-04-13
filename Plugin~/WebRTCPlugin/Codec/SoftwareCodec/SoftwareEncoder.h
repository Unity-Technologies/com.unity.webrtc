#pragma once
#include <vector>
#include <thread>
#include <atomic>
#include "Codec/IEncoder.h"

namespace unity
{
namespace webrtc
{
    
    class IGraphicsDevice;
    class ITexture2D;
    class SoftwareEncoder : public IEncoder
    {
    public:
        SoftwareEncoder(int _width, int _height, IGraphicsDevice* device);
        void InitV() override;
        void SetRates(const webrtc::VideoEncoder::RateControlParameters& parameters) override {}
        void UpdateSettings() override {}
        bool CopyBuffer(void* frame) override;
        bool EncodeFrame() override;
        bool IsSupported() const override { return true; }
        void SetIdrFrame() override {}
        uint64 GetCurrentFrameCount() const override { return m_frameCount; }

    private:
        IGraphicsDevice* m_device;
        ITexture2D* m_encodeTex;
        int m_width = 1920;
        int m_height = 1080;
        uint64 m_frameCount = 0;
    };
//---------------------------------------------------------------------------------------------------------------------
    
} //end namespace webrtc
} //end namespace unity
