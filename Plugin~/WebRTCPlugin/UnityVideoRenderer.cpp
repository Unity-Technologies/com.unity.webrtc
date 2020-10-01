#include "pch.h"
#include "UnityVideoRenderer.h"

namespace unity {
namespace webrtc {

UnityVideoRenderer::UnityVideoRenderer(uint32_t id) : m_id(id)
{
    DebugLog("Create UnityVideoRenderer Id:%d", id);
}

UnityVideoRenderer::~UnityVideoRenderer()
{
    DebugLog("Destory UnityVideoRenderer Id:%d", m_id);
    {
        std::unique_lock<std::mutex> lock(m_mutex);
    }
}

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
    std::unique_lock<std::mutex> lock(m_mutex);
    if (!lock.owns_lock())
    {
        return nullptr;
    }

    return m_frameBuffer;
}

void UnityVideoRenderer::SetFrameBuffer(rtc::scoped_refptr<webrtc::VideoFrameBuffer> buffer)
{
    std::unique_lock<std::mutex> lock(m_mutex);
    if (!lock.owns_lock())
    {
        return;
    }

    m_frameBuffer = buffer;
}

void UnityVideoRenderer::ConvertVideoFrameToTextureAndWriteToBuffer(int width, int height, webrtc::VideoType format)
{
    auto frame = GetFrameBuffer();
    if (frame == nullptr)
    {
        return;
    }

    rtc::scoped_refptr<webrtc::I420BufferInterface> i420_buffer;
    if (width == frame->width() && height == frame->height())
    {
        i420_buffer = frame->ToI420();
    }
    else
    {
        auto temp = webrtc::I420Buffer::Create(width, height);
        temp->ScaleFrom(*frame->ToI420());
        i420_buffer = temp;
    }

    size_t size = width * height * 4;
    if (tempBuffer.size() != size)
        tempBuffer.resize(size);

    libyuv::ConvertFromI420(
        i420_buffer->DataY(), i420_buffer->StrideY(), i420_buffer->DataU(),
        i420_buffer->StrideU(), i420_buffer->DataV(), i420_buffer->StrideV(),
        tempBuffer.data(), 0, width, height,
        ConvertVideoType(format));
}

} // end namespace webrtc
} // end namespace unity
