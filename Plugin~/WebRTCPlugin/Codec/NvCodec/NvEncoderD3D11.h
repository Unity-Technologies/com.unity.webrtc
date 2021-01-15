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
        ~NvEncoderD3D11() override;
    protected:

        void* AllocateInputResourceV(ITexture2D* tex) override;
        void ReleaseInputResourceV(void* pResource) override {}
    };

} // end namespace webrtc
} // end namespace unity
