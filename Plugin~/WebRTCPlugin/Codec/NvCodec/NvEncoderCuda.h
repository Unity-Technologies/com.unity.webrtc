#pragma once
#include "NvEncoder.h"

namespace unity
{
namespace webrtc
{

    class NvEncoderCuda : public NvEncoder
    {
    public:
        NvEncoderCuda(uint32_t nWidth, uint32_t nHeight, IGraphicsDevice* device);
        void InitV() override;
        virtual ~NvEncoderCuda() = default;
    protected:
        virtual void* AllocateInputResourceV(ITexture2D* tex) override;
    };

} // end namespace webrtc
} // end namespace unity
