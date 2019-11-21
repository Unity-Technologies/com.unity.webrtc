#pragma once
#include "NvEncoder.h"

namespace WebRTC {
    class NvEncoderCuda : public NvEncoder {
    public:
        NvEncoderCuda(uint32_t nWidth, uint32_t nHeight, IGraphicsDevice* device);
        virtual ~NvEncoderCuda() = default;
    protected:
        virtual void* AllocateInputResourceV(ITexture2D* tex) override;
    };
}
