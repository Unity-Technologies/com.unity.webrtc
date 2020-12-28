#pragma once

#include "GraphicsDevice/ITexture2D.h"
#include "WebRTCMacros.h"
#include <CoreVideo/CoreVideo.h>

namespace unity
{
namespace webrtc
{

    class MTLTexture;
    struct MetalTexture2D : ITexture2D {
    public:
        id<MTLTexture> m_texture;
        CVPixelBufferRef m_pixelBuffer;
        CVMetalTextureCacheRef m_textureCache;
        CVMetalTextureRef m_textureRef;

        MetalTexture2D(uint32_t w, uint32_t h, id<MTLTexture> tex, CVPixelBufferRef pixelBuffer = NULL, CVMetalTextureCacheRef textureCache = NULL, CVMetalTextureRef textureRef = NULL);
        virtual ~MetalTexture2D();

        inline virtual void* GetNativeTexturePtrV();
        inline virtual const void* GetNativeTexturePtrV() const;
        inline virtual void* GetEncodeTexturePtrV();
        inline virtual const void* GetEncodeTexturePtrV() const;

    };

//---------------------------------------------------------------------------------------------------------------------

    void* MetalTexture2D::GetNativeTexturePtrV() { return (__bridge void*)m_texture; }
    const void* MetalTexture2D::GetNativeTexturePtrV() const { return (__bridge void*)m_texture; };
    void* MetalTexture2D::GetEncodeTexturePtrV() { return (void*)m_pixelBuffer; }
    const void* MetalTexture2D::GetEncodeTexturePtrV() const { return (void*)m_pixelBuffer; }

} // end namespace webrtc
} // end namespace unity
