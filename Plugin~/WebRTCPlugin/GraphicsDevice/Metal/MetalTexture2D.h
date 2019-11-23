#pragma once

#include "GraphicsDevice/ITexture2D.h"
#include "WebRTCMacros.h"

namespace WebRTC {

    class MTLTexture;
    struct MetalTexture2D : ITexture2D {
    public:
        id<MTLTexture> m_texture;

        MetalTexture2D(uint32_t w, uint32_t h, id<MTLTexture> tex);
        virtual ~MetalTexture2D();

        inline virtual void* GetNativeTexturePtrV();
        inline virtual const void* GetNativeTexturePtrV() const;
        inline virtual void* GetEncodeTexturePtrV();
        inline virtual const void* GetEncodeTexturePtrV() const;

    };

//---------------------------------------------------------------------------------------------------------------------

    void* MetalTexture2D::GetNativeTexturePtrV() { return m_texture; }
    const void* MetalTexture2D::GetNativeTexturePtrV() const { return m_texture; };
    void* MetalTexture2D::GetEncodeTexturePtrV() { return m_texture; }
    const void* MetalTexture2D::GetEncodeTexturePtrV() const { return m_texture; }

} //end namespace


