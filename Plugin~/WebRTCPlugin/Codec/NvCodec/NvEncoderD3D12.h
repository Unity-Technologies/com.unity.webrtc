#pragma once
#include "NvEncoder.h"

namespace unity
{
namespace webrtc
{

    class NvEncoderD3D12 : public NvEncoder {
    public:
        NvEncoderD3D12(uint32_t nWidth, uint32_t nHeight, IGraphicsDevice* device);
        virtual ~NvEncoderD3D12();
    protected:

        virtual void* AllocateInputResourceV(ITexture2D* tex) override;
    };

} // end namespace webrtc
} // end namespace unity
