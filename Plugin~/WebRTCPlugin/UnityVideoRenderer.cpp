#include "pch.h"

#include <api/video/i420_buffer.h>

#include "UnityVideoRenderer.h"

namespace unity
{
namespace webrtc
{

    UnityVideoRenderer::UnityVideoRenderer(uint32_t id, DelegateVideoFrameResize callback, bool needFlipVertical)
        : m_id(id)
        , m_last_renderered_timestamp(0)
        , m_timestamp(0)
        , m_callback(callback)
        , m_needFlipVertical(needFlipVertical)
    {
        DebugLog("Create UnityVideoRenderer Id:%d", id);
    }

    UnityVideoRenderer::~UnityVideoRenderer()
    {
        DebugLog("Destroy UnityVideoRenderer Id:%d", m_id);
        {
            std::unique_lock<std::mutex> lock(m_mutex);
        }
    }

    void UnityVideoRenderer::OnFrame(const webrtc::VideoFrame& frame)
    {
        rtc::scoped_refptr<webrtc::VideoFrameBuffer> frame_buffer = frame.video_frame_buffer();

        if (frame_buffer->type() == webrtc::VideoFrameBuffer::Type::kNative)
        {
            frame_buffer = frame_buffer->ToI420();
        }
        SetFrameBuffer(frame_buffer, frame.timestamp_us());
    }

    uint32_t UnityVideoRenderer::GetId() { return m_id; }

    rtc::scoped_refptr<webrtc::VideoFrameBuffer> UnityVideoRenderer::GetFrameBuffer()
    {
        std::unique_lock<std::mutex> lock(m_mutex);
        if (!lock.owns_lock())
        {
            return nullptr;
        }
        if (m_last_renderered_timestamp == m_timestamp)
        {
            // skipped copying texture
            return nullptr;
        }
        m_last_renderered_timestamp = m_timestamp;
        return m_frameBuffer;
    }

    void UnityVideoRenderer::SetFrameBuffer(rtc::scoped_refptr<webrtc::VideoFrameBuffer> buffer, int64_t timestamp)
    {
        std::unique_lock<std::mutex> lock(m_mutex);
        if (!lock.owns_lock())
        {
            return;
        }
        if (!buffer)
        {
            RTC_LOG(LS_INFO) << "The video buffer is already released.";
            return;
        }

        if (m_frameBuffer == nullptr || m_frameBuffer->width() != buffer->width() ||
            m_frameBuffer->height() != buffer->height())
        {
            m_callback(this, buffer->width(), buffer->height());
        }

        m_frameBuffer = buffer;
        m_timestamp = timestamp;
    }

    void* UnityVideoRenderer::ConvertVideoFrameToTextureAndWriteToBuffer(int width, int height, libyuv::FourCC format)
    {
        auto frame = GetFrameBuffer();

        size_t size = static_cast<size_t>(width * height * 4);
        if (tempBuffer.size() != size)
            tempBuffer.resize(size);

        // return a previous texture buffer when framebuffer is returned null.
        if (!frame)
            return tempBuffer.data();

        rtc::scoped_refptr<webrtc::I420BufferInterface> i420_buffer = frame->ToI420();
        if (!i420_buffer)
            return tempBuffer.data();

        if (width != frame->width() || height != frame->height())
        {
            auto temp = webrtc::I420Buffer::Create(width, height);
            temp->ScaleFrom(*i420_buffer);
            i420_buffer = temp;
        }

        if (m_needFlipVertical)
            height = -height;

        int result = libyuv::ConvertFromI420(
            i420_buffer->DataY(),
            i420_buffer->StrideY(),
            i420_buffer->DataU(),
            i420_buffer->StrideU(),
            i420_buffer->DataV(),
            i420_buffer->StrideV(),
            tempBuffer.data(),
            0,
            width,
            height,
            static_cast<uint32_t>(format));

        if (result)
        {
            RTC_LOG(LS_INFO) << "libyuv::ConvertFromI420 failed. error:" << result;
        }
        return tempBuffer.data();
    }

} // end namespace webrtc
} // end namespace unity
