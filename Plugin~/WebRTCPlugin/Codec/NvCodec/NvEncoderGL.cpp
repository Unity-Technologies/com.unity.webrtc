#include "pch.h"
#include "nvEncodeAPI.h"
#include "NvEncoderGL.h"
#include "GraphicsDevice/IGraphicsDevice.h"
#include "GraphicsDevice/ITexture2D.h"

namespace unity
{
namespace webrtc
{

    NvEncoderGL::NvEncoderGL(uint32_t nWidth, uint32_t nHeight, IGraphicsDevice* device, UnityRenderingExtTextureFormat textureFormat) :
        NvEncoder(NV_ENC_DEVICE_TYPE_OPENGL, NV_ENC_INPUT_RESOURCE_TYPE_OPENGL_TEX, NV_ENC_BUFFER_FORMAT_ABGR, nWidth, nHeight, device, textureFormat)
    {
    }

    NvEncoderGL::~NvEncoderGL()
    {
        ReleaseEncoderResources();
    }

    void* NvEncoderGL::AllocateInputResourceV(ITexture2D* tex) {
        NV_ENC_INPUT_RESOURCE_OPENGL_TEX *pResource = new NV_ENC_INPUT_RESOURCE_OPENGL_TEX;
        pResource->texture = (GLuint)(size_t)(tex->GetEncodeTexturePtrV());
        pResource->target = GL_TEXTURE_2D;
        return pResource;
    }

    void NvEncoderGL::ReleaseInputResourceV(void* pResource)
    {
        NV_ENC_INPUT_RESOURCE_OPENGL_TEX* tex =
            static_cast<NV_ENC_INPUT_RESOURCE_OPENGL_TEX*>(pResource);
        delete tex;
    }

} // end namespace webrtc
} // end namespace unity 
