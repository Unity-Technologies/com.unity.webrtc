#include "pch.h"

#include "MetalTexture2D.h"

#include <third_party/libyuv/include/libyuv/convert.h>

namespace unity
{
namespace webrtc
{

    MetalTexture2D::MetalTexture2D(uint32_t w, uint32_t h, id<MTLTexture> tex)
        : ITexture2D(w, h)
        , m_texture(tex)
        , m_semaphore(dispatch_semaphore_create(1))
    {
    }

    MetalTexture2D::~MetalTexture2D()
    {
        // waiting for finishing usage of semaphore
        dispatch_semaphore_wait(m_semaphore, DISPATCH_TIME_FOREVER);
        dispatch_semaphore_signal(m_semaphore);

        dispatch_release(m_semaphore);
        [m_texture release];
    }

    rtc::scoped_refptr<I420Buffer> MetalTexture2D::ConvertI420Buffer()
    {
        RTC_DCHECK(m_texture);
        RTC_DCHECK_GT(m_width, 0);
        RTC_DCHECK_GT(m_height, 0);

        const uint32_t BYTES_PER_PIXEL = 4;
        const uint32_t bytesPerRow = m_width * BYTES_PER_PIXEL;
        const uint32_t bufferSize = bytesPerRow * m_height;

        if (m_buffer.size() != bufferSize)
            m_buffer.resize(bufferSize);

        [m_texture getBytes:m_buffer.data()
                bytesPerRow:bytesPerRow
                 fromRegion:MTLRegionMake2D(0, 0, m_width, m_height)
                mipmapLevel:0];

        if (!m_i420Buffer)
            m_i420Buffer = I420Buffer::Create(static_cast<int32_t>(m_width), static_cast<int32_t>(m_height));

        libyuv::ARGBToI420(
            m_buffer.data(),
            static_cast<int32_t>(bytesPerRow),
            m_i420Buffer->MutableDataY(),
            m_i420Buffer->StrideY(),
            m_i420Buffer->MutableDataU(),
            m_i420Buffer->StrideU(),
            m_i420Buffer->MutableDataV(),
            m_i420Buffer->StrideV(),
            static_cast<int32_t>(m_width),
            static_cast<int32_t>(m_height));

        return m_i420Buffer;
    }

} // end namespace webrtc
} // end namespace unity
