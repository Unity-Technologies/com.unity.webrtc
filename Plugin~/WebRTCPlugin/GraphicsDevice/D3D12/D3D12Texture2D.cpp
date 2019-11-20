#include "pch.h"
#include "D3D12Texture2D.h"

namespace WebRTC {

//---------------------------------------------------------------------------------------------------------------------

D3D12Texture2D::D3D12Texture2D(uint32_t w, uint32_t h, ID3D12Resource* nativeTex, HANDLE handle
                               , ID3D11Texture2D* sharedTex)
    : ITexture2D(w,h)
    , m_nativeTexture(nativeTex), m_sharedHandle(handle), m_sharedTexture(sharedTex)
{

}

} //end namespace
