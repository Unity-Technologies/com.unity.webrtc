#pragma once

#include <d3d11.h>

#include "GpuMemoryBuffer.h"
#include "GraphicsDevice/ITexture2D.h"

namespace unity
{
namespace webrtc
{

    struct D3D11Texture2D : ITexture2D
    {
    public:
        ID3D11Texture2D* m_texture;

        D3D11Texture2D(uint32_t w, uint32_t h, ID3D11Texture2D* tex);

        virtual ~D3D11Texture2D() override { SAFE_RELEASE(m_texture) }

        inline virtual void* GetNativeTexturePtrV() override;
        inline virtual const void* GetNativeTexturePtrV() const override;
        inline virtual void* GetEncodeTexturePtrV() override;
        inline virtual const void* GetEncodeTexturePtrV() const override;
    };

    void* D3D11Texture2D::GetNativeTexturePtrV() { return m_texture; }
    const void* D3D11Texture2D::GetNativeTexturePtrV() const { return m_texture; };
    void* D3D11Texture2D::GetEncodeTexturePtrV() { return m_texture; }
    const void* D3D11Texture2D::GetEncodeTexturePtrV() const { return m_texture; }

} // end namespace webrtc
} // end namespace unity
