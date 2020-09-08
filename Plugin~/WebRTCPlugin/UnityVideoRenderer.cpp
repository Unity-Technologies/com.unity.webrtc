#include "pch.h"
#include "UnityVideoRenderer.h"

namespace unity {
namespace webrtc {

UnityVideoRenderer::UnityVideoRenderer(uint32_t id) : m_id(id)
{
    DebugLog("Create UnityVideoRenderer Id:%d", id);
}

UnityVideoRenderer::~UnityVideoRenderer() = default;

void UnityVideoRenderer::OnFrame(const webrtc::VideoFrame &frame)
{
    rtc::scoped_refptr<webrtc::VideoFrameBuffer> frame_buffer = frame.video_frame_buffer();

    if (frame_buffer->type() == webrtc::VideoFrameBuffer::Type::kNative)
    {
        frame_buffer = frame_buffer->ToI420();
    }

    SetFrameBuffer(frame_buffer);
}

uint32_t UnityVideoRenderer::GetId()
{
    return m_id;
}

rtc::scoped_refptr<webrtc::VideoFrameBuffer> UnityVideoRenderer::GetFrameBuffer()
{
    std::lock_guard<std::mutex> guard(m_mutex);
    return m_frameBuffer;
}

void UnityVideoRenderer::SetFrameBuffer(rtc::scoped_refptr<webrtc::VideoFrameBuffer> buffer)
{
    std::lock_guard<std::mutex> guard(m_mutex);
    m_frameBuffer = buffer;
}

UnityVideoRenderer::ImageData* UnityVideoRenderer::GetImageData()
{
    std::lock_guard<std::mutex> guard(m_mutex);

    if (m_frameBuffer == nullptr)
    {
        DebugLog("FrameBuffer is not received yet");
        return &m_imageData;
    }

    if (m_imageData.Height != m_frameBuffer->height() || m_imageData.Width != m_frameBuffer->width())
    {
        m_imageData.Height = m_frameBuffer->height();
        m_imageData.Width = m_frameBuffer->width();
        m_imageData.RawData = new uint8_t[m_frameBuffer->height() * m_frameBuffer->width() * 4];
    }

    rtc::scoped_refptr<webrtc::I420Buffer> i420_buffer = webrtc::I420Buffer::Copy(*m_frameBuffer);

    libyuv::ConvertFromI420(
      i420_buffer->DataY(), i420_buffer->StrideY(), i420_buffer->DataU(),
      i420_buffer->StrideU(), i420_buffer->DataV(), i420_buffer->StrideV(),
      m_imageData.RawData, 0, m_imageData.Width, m_imageData.Height,
      ConvertVideoType(webrtc::VideoType::kABGR));

    return &m_imageData;
}

} // end namespace webrtc
} // end namespace unity
