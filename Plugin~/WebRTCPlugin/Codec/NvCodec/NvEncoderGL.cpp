#include "pch.h"
#include "nvEncodeAPI.h"
#include "NvEncoderGL.h"
#include "GraphicsDevice/OpenGL/OpenGLTexture2D.h"

namespace WebRTC {

    NvEncoderGL::NvEncoderGL(uint32_t nWidth, uint32_t nHeight, IGraphicsDevice* device) :
        NvEncoder(NV_ENC_DEVICE_TYPE_OPENGL, NV_ENC_INPUT_RESOURCE_TYPE_OPENGL_TEX, nWidth, nHeight, device)
    {
    }

    NvEncoderGL::~NvEncoderGL()
    {
    }

    void NvEncoderGL::ReleaseInputBuffers()
    {
    }

    void* NvEncoderGL::AllocateInputBuffers()
    {
        NV_ENC_INPUT_RESOURCE_OPENGL_TEX *pResource = new NV_ENC_INPUT_RESOURCE_OPENGL_TEX;
        NV_ENC_BUFFER_FORMAT format = NV_ENC_BUFFER_FORMAT_ARGB;
        uint32_t chromaHeight = GetNumChromaPlanes(format) * GetChromaHeight(format, height);

        auto tex = m_device->CreateDefaultTextureV(width, height);
        pResource->texture = *tex->GetEncodeTexturePtrV();
        pResource->target = GL_TEXTURE_2D;
        return pResource;
    }
}
