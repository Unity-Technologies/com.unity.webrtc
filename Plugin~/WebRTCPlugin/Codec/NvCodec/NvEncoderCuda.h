#pragma once
#include "NvEncoder.h"

namespace unity
{
namespace webrtc
{

    class NvEncoderCuda : public NvEncoder
    {
    public:
        NvEncoderCuda(uint32_t nWidth, uint32_t nHeight, IGraphicsDevice* device, UnityRenderingExtTextureFormat textureFormat);
        void InitV() override;
        ~NvEncoderCuda() override;
    protected:
        void* AllocateInputResourceV(ITexture2D* tex) override;
        void ReleaseInputResourceV(void* pResource) override {}
    };

} // end namespace webrtc
} // end namespace unity
