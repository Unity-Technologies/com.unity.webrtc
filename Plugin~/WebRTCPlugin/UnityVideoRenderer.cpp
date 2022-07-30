#include "pch.h"

#include <api/video/i420_buffer.h>

#include "UnityVideoRenderer.h"
#include "GraphicsDevice/IGraphicsDevice.h"

namespace unity
{
namespace webrtc
{

    UnityVideoRenderer::UnityVideoRenderer(uint32_t id, DelegateVideoFrameResize callback, bool needFlipVertical, IGraphicsDevice* device)
        : m_id(id)
        , m_last_renderered_timestamp(0)
        , m_timestamp(0)
        , m_callback(callback)
        , m_needFlipVertical(needFlipVertical)
        , m_device(device)
        , m_texture(nullptr)
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

    void UnityVideoRenderer::SetTexture(void* texture) { m_texture = texture; }
    void UnityVideoRenderer::OnFrame(const webrtc::VideoFrame& frame)
    {
        rtc::scoped_refptr<webrtc::VideoFrameBuffer> frame_buffer = frame.video_frame_buffer();
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
            return;

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
        auto buffer = GetFrameBuffer();

        size_t size = static_cast<size_t>(width * height * 4);
        if (tempBuffer.size() != size)
            tempBuffer.resize(size);

        // return a previous texture buffer when framebuffer is returned null.
        if (!buffer)
            return nullptr;

        if (buffer->type() == webrtc::VideoFrameBuffer::Type::kNative)
        {
            if(!m_texture)
                return nullptr;
            bool result = m_device->CopyResourceFromBuffer(m_texture, buffer);
            if(!result)
            {
                RTC_LOG(LS_INFO) << "CopyResourceFromBuffer failed.";
                return nullptr;
            }
            return nullptr;
        }
        
        rtc::scoped_refptr<webrtc::I420BufferInterface> i420_buffer;
        if (width == buffer->width() && height == buffer->height())
        {
            i420_buffer = buffer->ToI420();
        }
        else
        {
            auto temp = webrtc::I420Buffer::Create(width, height);
            temp->ScaleFrom(*buffer->ToI420());
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
