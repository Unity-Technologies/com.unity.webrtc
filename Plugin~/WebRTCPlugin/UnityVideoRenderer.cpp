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

    std::lock_guard<std::mutex> guard(m_mutex);
    m_imageData.Height = frame.height();
    m_imageData.Width = frame.width();
    m_imageData.RawData = new uint8_t[m_imageData.Height * m_imageData.Width * 4];

    webrtc::ConvertFromI420(frame, webrtc::VideoType::kARGB, 0, m_imageData.RawData);
}

UnityVideoRenderer::ImageData* UnityVideoRenderer::GetFrameBuffer()
{
    DebugLog("Get frame buffer on UnityCideoRenderer");

    std::lock_guard<std::mutex> guard(m_mutex);
    return &m_imageData;
}

} // end namespace webrtc
} // end namespace unity
