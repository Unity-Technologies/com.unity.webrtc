#include "pch.h"

#include "OpenGLTexture2D.h"

#if CUDA_PLATFORM
#include <cudaGL.h>
#endif

namespace unity
{
namespace webrtc
{

//---------------------------------------------------------------------------------------------------------------------

OpenGLTexture2D::OpenGLTexture2D(uint32_t w, uint32_t h, GLuint tex) : ITexture2D(w,h)
        , m_texture(tex)
        , m_buffer(nullptr)
{
}

OpenGLTexture2D::~OpenGLTexture2D()
{
    glDeleteTextures(1, &m_texture);
    m_texture = 0;

    glBindBuffer(GL_ARRAY_BUFFER, m_pbo);
    glDeleteBuffers(1, &m_pbo);
    m_pbo = 0;

    free(m_buffer);
    m_buffer = nullptr;
}

void OpenGLTexture2D::CreatePBO()
{
    glGenBuffers(1, &m_pbo);
    glBindBuffer(GL_PIXEL_UNPACK_BUFFER, m_pbo);

    const size_t bufferSize = GetBufferSize();
    glBufferData(GL_PIXEL_UNPACK_BUFFER, bufferSize, nullptr, GL_DYNAMIC_DRAW);
    glBindBuffer(GL_PIXEL_UNPACK_BUFFER, 0);

    if(m_buffer == nullptr)
    {
        m_buffer = static_cast<byte*>(malloc(bufferSize));
    }
}

    std::unique_ptr<GpuMemoryBufferHandle> OpenGLTexture2D::Map()
    {
        CUarray mappedArray;
        CUgraphicsResource resource;
        GLuint image = m_texture;
        GLenum target = GL_TEXTURE_2D;

        CUresult result = cuGraphicsGLRegisterImage(&resource, image, target, CU_GRAPHICS_REGISTER_FLAGS_SURFACE_LDST);
        if (result != CUDA_SUCCESS)
        {
            RTC_LOG(LS_ERROR) << "cuGraphicsD3D11RegisterResource error" << result;
            throw;
        }

        result = cuGraphicsMapResources(1, &resource, 0);
        if (result != CUDA_SUCCESS)
        {
            RTC_LOG(LS_ERROR) << "cuGraphicsMapResources";
            throw;
        }

        result = cuGraphicsSubResourceGetMappedArray(&mappedArray, resource, 0, 0);
        if (result != CUDA_SUCCESS)
        {
            RTC_LOG(LS_ERROR) << "cuGraphicsSubResourceGetMappedArray";
            throw;
        }
        std::unique_ptr<GpuMemoryBufferHandle> handle = std::make_unique<GpuMemoryBufferHandle>();
        handle->array = mappedArray;
        handle->resource = resource;
        return handle;
    }

} // end namespace webrtc
} // end namespace unity