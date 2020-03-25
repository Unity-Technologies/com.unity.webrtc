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
        virtual void InitV() override;
        virtual void SetRate(uint32_t rate) override {}
        virtual void UpdateSettings() override {}
        virtual bool CopyBuffer(void* frame) override;
        virtual bool EncodeFrame() override;
        virtual bool IsSupported() const override { return true; }
        virtual void SetIdrFrame() override {}
        virtual uint64 GetCurrentFrameCount() const override { return m_frameCount; }        

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
