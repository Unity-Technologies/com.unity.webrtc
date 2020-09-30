#include "pch.h"
#include "UnityVideoRenderer.h"

namespace unity {
namespace webrtc {

UnityVideoRenderer::UnityVideoRenderer()
{
    //Do nothing
    DebugLog("Create UnityVideoRenderer");
}

UnityVideoRenderer::~UnityVideoRenderer() = default;

void UnityVideoRenderer::OnFrame(const webrtc::VideoFrame &frame)
{
    //ToDo Implement
    DebugLog("Invoked OnFrame on UnityVideoRenderer");

    rtc::scoped_refptr<webrtc::VideoFrameBuffer> frame_buffer = frame.video_frame_buffer();

    if (frame_buffer->type() == webrtc::VideoFrameBuffer::Type::kNative)
    {
        frame_buffer = frame_buffer->ToI420();
    }

    std::lock_guard<std::mutex> guard(m_mutex);
    m_frameBuffer = frame_buffer;
}

UnityVideoRenderer::ImageData* UnityVideoRenderer::GetFrameBuffer()
{
    DebugLog("Get frame buffer on UnityCideoRenderer");

    std::lock_guard<std::mutex> guard(m_mutex);

    if (m_imageData.RawData != nullptr)
    {
        delete[] m_imageData.RawData;
    }
    

    m_imageData.Height = m_frameBuffer->height();
    m_imageData.Width = m_frameBuffer->width();
    m_imageData.RawData = new uint8_t[webrtc::CalcBufferSize(webrtc::VideoType::kABGR, m_imageData.Width, m_imageData.Height)];

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
