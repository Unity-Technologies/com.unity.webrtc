#pragma once
#include "NvEncoder.h"

namespace unity
{
namespace webrtc
{

    class NvEncoderD3D11 : public NvEncoder
    {
    public:
        NvEncoderD3D11(uint32_t nWidth, uint32_t nHeight, IGraphicsDevice* device, UnityRenderingExtTextureFormat textureFromat);
        virtual ~NvEncoderD3D11();
    protected:

        virtual void* AllocateInputResourceV(ITexture2D* tex) override;
    };

} // end namespace webrtc
} // end namespace unity
