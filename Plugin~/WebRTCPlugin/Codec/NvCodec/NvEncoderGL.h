#pragma once
#include "NvEncoder.h"

namespace unity
{
namespace webrtc
{

    class NvEncoderGL : public NvEncoder {
    public:
        NvEncoderGL(uint32_t nWidth, uint32_t nHeight, IGraphicsDevice* device, UnityRenderingExtTextureFormat textureFormat);
        ~NvEncoderGL() override;
    protected:
        virtual void* AllocateInputResourceV(ITexture2D* tex) override;
        virtual void ReleaseInputResourceV(void* pResource) override;
    };

} // end namespace webrtc
} // end namespace unity 

