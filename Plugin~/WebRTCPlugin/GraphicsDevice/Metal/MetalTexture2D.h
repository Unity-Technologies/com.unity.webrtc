#pragma once

#include "GraphicsDevice/ITexture2D.h"
#include "WebRTCMacros.h"

#include <api/video/i420_buffer.h>

namespace unity
{
namespace webrtc
{
    using namespace ::webrtc;

    class MTLTexture;
    struct MetalTexture2D : ITexture2D
    {
    public:
        MetalTexture2D(uint32_t w, uint32_t h, id<MTLTexture> tex);
        virtual ~MetalTexture2D() override;

        inline void* GetNativeTexturePtrV() override;
        inline const void* GetNativeTexturePtrV() const override;
        inline void* GetEncodeTexturePtrV() override;
        inline const void* GetEncodeTexturePtrV() const override;

        void SetSemaphore(dispatch_semaphore_t semaphore) { m_semaphore = semaphore; }
        dispatch_semaphore_t GetSemaphore() const { return m_semaphore; }

        rtc::scoped_refptr<I420Buffer> ConvertI420Buffer();

    private:
        id<MTLTexture> m_texture;
        dispatch_semaphore_t m_semaphore;
        std::vector<uint8_t> m_buffer;
        rtc::scoped_refptr<I420Buffer> m_i420Buffer;
    };

    void* MetalTexture2D::GetNativeTexturePtrV() { return m_texture; }
    const void* MetalTexture2D::GetNativeTexturePtrV() const { return m_texture; };
    void* MetalTexture2D::GetEncodeTexturePtrV() { return m_texture; }
    const void* MetalTexture2D::GetEncodeTexturePtrV() const { return m_texture; }

} // end namespace webrtc
} // end namespace unity
