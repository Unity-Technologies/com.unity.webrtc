#include "pch.h"
#include "Codec/VideoToolbox/VTEncoderMetal.h"
#include "GraphicsDevice/IGraphicsDevice.h"
#include "GraphicsDevice/ITexture2D.h"

namespace WebRTC {

    VTEncoderMetal::VTEncoderMetal(uint32_t nWidth, uint32_t nHeight, IGraphicsDevice* device)
    {
    }

    VTEncoderMetal::~VTEncoderMetal()
    {
    }
    void VTEncoderMetal::SetRate(uint32_t rate)
    {
    }
    void VTEncoderMetal::UpdateSettings()
    {
    }
    bool VTEncoderMetal::CopyBuffer(void* frame)
    {
        return true;
    }
    void VTEncoderMetal::EncodeFrame()
    {
    }
    bool VTEncoderMetal::IsSupported() const
    {
        return true;
    }
    void VTEncoderMetal::SetIdrFrame()
    {
    }
    uint64 VTEncoderMetal::GetCurrentFrameCount()
    {
        return 0;
    }

}
