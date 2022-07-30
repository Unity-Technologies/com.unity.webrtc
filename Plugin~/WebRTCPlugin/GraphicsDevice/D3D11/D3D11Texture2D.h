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
        D3D11Texture2D(
            uint32_t width,
            uint32_t height,
            UnityRenderingExtTextureFormat format,
            ID3D11Texture2D* tex,
            bool externalTexture = false);

        virtual ~D3D11Texture2D() override
        {
            if (!m_externalTexture)
                SAFE_RELEASE(m_texture)
        }

        inline virtual void* GetNativeTexturePtrV() override;
        inline virtual const void* GetNativeTexturePtrV() const override;
        inline virtual void* GetEncodeTexturePtrV() override;
        inline virtual const void* GetEncodeTexturePtrV() const override;

    private:
        ID3D11Texture2D* m_texture;
        bool m_externalTexture;
    };

    void* D3D11Texture2D::GetNativeTexturePtrV() { return m_texture; }
    const void* D3D11Texture2D::GetNativeTexturePtrV() const { return m_texture; };
    void* D3D11Texture2D::GetEncodeTexturePtrV() { return m_texture; }
    const void* D3D11Texture2D::GetEncodeTexturePtrV() const { return m_texture; }
} // end namespace webrtc
} // end namespace unity
