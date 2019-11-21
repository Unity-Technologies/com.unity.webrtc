﻿#pragma once
#include "NvEncoder.h"

namespace WebRTC {
    class NvEncoderGL : public NvEncoder {
    public:
        NvEncoderGL(uint32_t nWidth, uint32_t nHeight, IGraphicsDevice* device);
        virtual ~NvEncoderGL();
    protected:
        virtual void* AllocateInputResourceV(ITexture2D* tex) override;
    };
}
