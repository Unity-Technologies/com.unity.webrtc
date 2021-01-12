#pragma once
#include "NvEncoder.h"

namespace unity
{
namespace webrtc
{

    class NvEncoderD3D12 : public NvEncoder {
    public:
        NvEncoderD3D12(uint32_t nWidth, uint32_t nHeight, IGraphicsDevice* device, UnityRenderingExtTextureFormat textureFormat);
        ~NvEncoderD3D12() override;
    protected:

        void* AllocateInputResourceV(ITexture2D* tex) override;
        void ReleaseInputResourceV(void* pResource) override {}

    };

} // end namespace webrtc
} // end namespace unity
