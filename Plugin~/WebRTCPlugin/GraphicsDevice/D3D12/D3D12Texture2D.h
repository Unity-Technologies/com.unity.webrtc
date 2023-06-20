#pragma once

#include <d3d12.h>
#include <wrl/client.h>

#include "D3D12ResourceFootprint.h"
#include "GraphicsDevice/ITexture2D.h"
#include "PlatformBase.h"
#include "WebRTCMacros.h"

using namespace Microsoft::WRL;

namespace unity
{
namespace webrtc
{
    class D3D12Texture2D : public ITexture2D
    {
    public:
        // copy to GPU texture
        D3D12Texture2D(uint32_t w, uint32_t h, ID3D12Resource* nativeTex, HANDLE handle);

        // copy to CPU buffer
        D3D12Texture2D(uint32_t w, uint32_t h, ID3D12Resource* nativeTex, const D3D12ResourceFootprint& footprint);

        virtual ~D3D12Texture2D() override { CloseHandle(m_sharedHandle); }

        inline void* GetNativeTexturePtrV() override;
        inline const void* GetNativeTexturePtrV() const override;
        inline void* GetEncodeTexturePtrV() override;
        inline const void* GetEncodeTexturePtrV() const override;
        uint64_t GetSyncCount() const { return m_syncCount; }
        void SetSyncCount(uint64_t value) { m_syncCount = value; }
        inline const D3D12ResourceFootprint* GetNativeTextureFootprint() const;
        HANDLE GetHandle() const { return m_sharedHandle; }
        D3D12_RESOURCE_DESC GetDesc() const { return m_nativeTexture->GetDesc(); }
        bool IsReadbackResource() { return m_readbackResource; }

        static D3D12Texture2D* CreateReadbackResource(ID3D12Device* device, uint32_t w, uint32_t h);

    private:
        ComPtr<ID3D12Resource> m_nativeTexture;
        HANDLE m_sharedHandle;
        mutable uint64_t m_syncCount;
        D3D12ResourceFootprint m_nativeTextureFootprint;

        bool m_readbackResource;
    };

    void* D3D12Texture2D::GetNativeTexturePtrV() { return m_nativeTexture.Get(); }
    const void* D3D12Texture2D::GetNativeTexturePtrV() const { return m_nativeTexture.Get(); };

    void* D3D12Texture2D::GetEncodeTexturePtrV() { return m_nativeTexture.Get(); }
    const void* D3D12Texture2D::GetEncodeTexturePtrV() const { return m_nativeTexture.Get(); }
    const D3D12ResourceFootprint* D3D12Texture2D::GetNativeTextureFootprint() const
    {
        return &m_nativeTextureFootprint;
    }

} // end namespace webrtc
} // end namespace unity
