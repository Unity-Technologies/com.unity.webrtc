#pragma once
#include <vector>
#include <thread>
#include <atomic>
#include "Codec/IEncoder.h"

namespace WebRTC
{
    class IGraphicsDevice;
    class ITexture2D;
    class SoftwareEncoder : public IEncoder
    {
    public:
        SoftwareEncoder(int _width, int _height, IGraphicsDevice* device);
        void InitV() override;
        void SetRate(uint32_t rate) override {}
        void UpdateSettings() override {}
        bool CopyBuffer(void* frame) override;
        bool EncodeFrame() override;
        bool IsSupported() const override { return true; }
        void SetIdrFrame() override {}
        uint64 GetCurrentFrameCount() override { return 0; }

    private:
        IGraphicsDevice* m_device;
        ITexture2D* m_encodeTex;
        int m_width = 1920;
        int m_height = 1080;
    };
}
