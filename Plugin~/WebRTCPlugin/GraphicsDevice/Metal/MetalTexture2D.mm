#include "pch.h"
#include "MetalTexture2D.h"

namespace unity
{
namespace webrtc
{

//---------------------------------------------------------------------------------------------------------------------

    MetalTexture2D::MetalTexture2D(uint32_t w, uint32_t h, id<MTLTexture> tex, CVPixelBufferRef pixelBuffer, CVMetalTextureCacheRef textureCache, CVMetalTextureRef textureRef)
    : ITexture2D(w,h) , m_texture(tex), m_pixelBuffer(pixelBuffer), m_textureCache(textureCache), m_textureRef(textureRef)
    {
    }

    MetalTexture2D::~MetalTexture2D()
    {
        m_texture = nullptr;

        if (m_textureRef) {
            CFRelease(m_textureRef);
            m_textureRef = nil;
        }
        if (m_pixelBuffer) {
            CVPixelBufferRelease(m_pixelBuffer);
            m_pixelBuffer = nil;
        }
        if (m_textureCache) {
            CFRelease(m_textureCache);
            m_textureCache = nil;
        }
    }
    
} // end namespace webrtc
} // end namespace unity
