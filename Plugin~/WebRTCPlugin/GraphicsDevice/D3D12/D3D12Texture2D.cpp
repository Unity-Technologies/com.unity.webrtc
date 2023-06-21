#include "pch.h"

#include "D3D12Constants.h"
#include "D3D12Texture2D.h"

// nonstandard extension used : class rvalue used as lvalue
#pragma clang diagnostic ignored "-Wlanguage-extension-token"

namespace unity
{
namespace webrtc
{
    D3D12Texture2D::D3D12Texture2D(uint32_t w, uint32_t h, ID3D12Resource* nativeTex, HANDLE handle)
        : ITexture2D(w, h)
        , m_nativeTexture(nativeTex)
        , m_sharedHandle(handle)
        , m_syncCount(0)
        , m_readbackResource(false)
    {
    }

    D3D12Texture2D::D3D12Texture2D(
        uint32_t w, uint32_t h, ID3D12Resource* nativeTex, const D3D12ResourceFootprint& footprint)
        : ITexture2D(w, h)
        , m_nativeTexture(nativeTex)
        , m_sharedHandle(nullptr)
        , m_syncCount(0)
        , m_nativeTextureFootprint(footprint)
        , m_readbackResource(true)
    {
    }
} // end namespace webrtc
} // end namespace unity
