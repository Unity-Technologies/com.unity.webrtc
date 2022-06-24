#pragma once

#include <d3d11.h>
#include <d3d11_4.h>
#include <wrl/client.h>

#include "GpuMemoryBuffer.h"
#include "GraphicsDevice/ITexture2D.h"

using namespace Microsoft::WRL;

namespace unity
{
namespace webrtc
{

    struct D3D11Texture2D : ITexture2D
    {
    public:
        D3D11Texture2D(uint32_t w, uint32_t h, ID3D11Texture2D* tex, ID3D11Fence* fence);
        ~D3D11Texture2D() override = default;

        inline void* GetNativeTexturePtrV() override;
        inline const void* GetNativeTexturePtrV() const override;
        inline void* GetEncodeTexturePtrV() override;
        inline const void* GetEncodeTexturePtrV() const override;
        ID3D11Fence* GetFence() const { return m_fence.Get(); }
        uint64_t GetSyncCount() const { return m_syncCount; }
        void UpdateSyncCount() const { m_syncCount = m_fence->GetCompletedValue(); }

    private:
        ComPtr<ID3D11Texture2D> m_texture;
        ComPtr<ID3D11Fence> m_fence;
        mutable uint64_t m_syncCount;
    };

    void* D3D11Texture2D::GetNativeTexturePtrV() { return m_texture.Get(); }
    const void* D3D11Texture2D::GetNativeTexturePtrV() const { return m_texture.Get(); };
    void* D3D11Texture2D::GetEncodeTexturePtrV() { return m_texture.Get(); }
    const void* D3D11Texture2D::GetEncodeTexturePtrV() const { return m_texture.Get(); }

} // end namespace webrtc
} // end namespace unity
