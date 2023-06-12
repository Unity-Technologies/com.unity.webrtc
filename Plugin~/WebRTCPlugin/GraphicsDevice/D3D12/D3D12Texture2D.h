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
        D3D12Texture2D(uint32_t w, uint32_t h, ID3D12Resource* nativeTex, HANDLE handle);

        virtual ~D3D12Texture2D() override { CloseHandle(m_sharedHandle); }

        inline void* GetNativeTexturePtrV() override;
        inline const void* GetNativeTexturePtrV() const override;
        inline void* GetEncodeTexturePtrV() override;
        inline const void* GetEncodeTexturePtrV() const override;
        ID3D12Fence* GetFence() const { return m_fence.Get(); }
        uint64_t GetSyncCount() const { return m_syncCount; }
        void UpdateSyncCount() const { m_syncCount = m_fence->GetCompletedValue(); }
        HRESULT CreateReadbackResource(ID3D12Device* device);
        inline ID3D12Resource* GetReadbackResource() const;
        inline const D3D12ResourceFootprint* GetNativeTextureFootprint() const;
        HANDLE GetHandle() const { return m_sharedHandle; }
        D3D12_RESOURCE_DESC GetDesc() const { return m_nativeTexture->GetDesc(); }

    private:
        ComPtr<ID3D12Resource> m_nativeTexture;
        HANDLE m_sharedHandle;
        ID3D11Texture2D* m_sharedTexture; // Shared between DX11 and DX12
        ComPtr<ID3D12Fence> m_fence;
        mutable uint64_t m_syncCount;

        // For CPU Read
        ComPtr<ID3D12Resource> m_readbackResource;
        D3D12ResourceFootprint m_nativeTextureFootprint;
    };

    void* D3D12Texture2D::GetNativeTexturePtrV() { return m_nativeTexture.Get(); }
    const void* D3D12Texture2D::GetNativeTexturePtrV() const { return m_nativeTexture.Get(); };

    void* D3D12Texture2D::GetEncodeTexturePtrV() { return m_nativeTexture.Get(); }
    const void* D3D12Texture2D::GetEncodeTexturePtrV() const { return m_nativeTexture.Get(); }
    ID3D12Resource* D3D12Texture2D::GetReadbackResource() const { return m_readbackResource.Get(); }
    const D3D12ResourceFootprint* D3D12Texture2D::GetNativeTextureFootprint() const
    {
        return &m_nativeTextureFootprint;
    }

} // end namespace webrtc
} // end namespace unity
