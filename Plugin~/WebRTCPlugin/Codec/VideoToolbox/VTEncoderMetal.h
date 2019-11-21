#pragma once
#include "Codec/IEncoder.h"
#include "GraphicsDevice/IGraphicsDevice.h"

namespace WebRTC {
    class VTEncoderMetal : public IEncoder{
    public:
        VTEncoderMetal(uint32_t nWidth, uint32_t nHeight, IGraphicsDevice* device);
        virtual ~VTEncoderMetal();
        virtual void SetRate(uint32_t rate);
        virtual void UpdateSettings();
        virtual bool CopyBuffer(void* frame);
        virtual void EncodeFrame();
        virtual bool IsSupported() const;
        virtual void SetIdrFrame();
        virtual uint64 GetCurrentFrameCount();
    };
}
