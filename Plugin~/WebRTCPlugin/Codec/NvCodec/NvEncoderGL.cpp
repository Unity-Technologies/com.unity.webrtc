#include "pch.h"
#include "nvEncodeAPI.h"
#include "NvEncoderGL.h"
#include "GraphicsDevice/IGraphicsDevice.h"
#include "GraphicsDevice/ITexture2D.h"

namespace WebRTC {

    NvEncoderGL::NvEncoderGL(uint32_t nWidth, uint32_t nHeight, IGraphicsDevice* device) :
        NvEncoder(NV_ENC_DEVICE_TYPE_OPENGL, NV_ENC_INPUT_RESOURCE_TYPE_OPENGL_TEX, nWidth, nHeight, device)
    {
        InitEncoderResources();
        isNvEncoderSupported = true;
    }

    NvEncoderGL::~NvEncoderGL()
    {
    }

    void* NvEncoderGL::AllocateInputBuffer()
    {
        NV_ENC_INPUT_RESOURCE_OPENGL_TEX *pResource = new NV_ENC_INPUT_RESOURCE_OPENGL_TEX;
        NV_ENC_BUFFER_FORMAT format = NV_ENC_BUFFER_FORMAT_ARGB;
        uint32_t chromaHeight = GetNumChromaPlanes(format) * GetChromaHeight(format, height);

        auto tex = m_device->CreateDefaultTextureV(width, height);
        pResource->texture = (GLuint)(size_t)(tex->GetEncodeTexturePtrV());
        pResource->target = GL_TEXTURE_2D;
        return pResource;
    }
    ITexture2D* NvEncoderGL::CreateTexture2DFromInputBuffer(void* buffer)
    {
        auto pResource = static_cast<NV_ENC_INPUT_RESOURCE_OPENGL_TEX*>(buffer);
        return m_device->CreateDefaultTextureFromNativeV(width, height, &(pResource->texture));
    }
}
