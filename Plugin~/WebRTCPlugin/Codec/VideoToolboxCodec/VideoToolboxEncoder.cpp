#include "pch.h"
#include "VideoToolboxEncoder.h"
#include "GraphicsDevice/IGraphicsDevice.h"
#include "GraphicsDevice/ITexture2D.h"
#include "sdk/objc/components/video_frame_buffer/RTCCVPixelBuffer.h"
#include "sdk/objc/native/src/objc_frame_buffer.h"

namespace unity
{
namespace webrtc
{

    VideoToolboxEncoder::VideoToolboxEncoder(int width, int height, IGraphicsDevice* device, UnityRenderingExtTextureFormat textureFormat) :
    m_device(device),
    m_width(width),
    m_height(height),
    m_textureFormat(textureFormat),
    m_frameCount(0)
    {
        for (uint32 i = 0; i < bufferedFrameNum; i++)
        {
            m_encodeTex[i] = nullptr;
        }
    }

    VideoToolboxEncoder::~VideoToolboxEncoder()
    {
        for (uint32 i = 0; i < bufferedFrameNum; i++)
        {
            delete m_encodeTex[i];
            m_encodeTex[i] = nullptr;
        }
    }

    void VideoToolboxEncoder::InitV()
    {
        for (uint32 i = 0; i < bufferedFrameNum; i++)
        {
            m_encodeTex[i] = m_device->CreateCPUReadTextureV(m_width, m_height, m_textureFormat);
            CVPixelBufferRef pixelBuffer = (CVPixelBufferRef) m_encodeTex[i]->GetEncodeTexturePtrV();
            m_videoFrameBuffer[i] = new rtc::RefCountedObject<webrtc::ObjCFrameBuffer>([[RTC_OBJC_TYPE(RTCCVPixelBuffer) alloc] initWithPixelBuffer: pixelBuffer]);
        }
        m_initializationResult = CodecInitializationResult::Success;
    }

    bool VideoToolboxEncoder::CopyBuffer(void* frame)
    {
        const int curFrameNum = GetCurrentFrameCount() % bufferedFrameNum;
        m_device->CopyResourceFromNativeV(m_encodeTex[curFrameNum], frame);
        return true;
    }

    bool VideoToolboxEncoder::EncodeFrame(int64_t timestamp_us)
    {
        const int curFrameNum = GetCurrentFrameCount() % bufferedFrameNum;

        webrtc::VideoFrame frame =
            webrtc::VideoFrame::Builder()
            .set_video_frame_buffer(m_videoFrameBuffer[curFrameNum])
            .set_rotation(webrtc::kVideoRotation_0)
            .set_timestamp_us(timestamp_us)
            .build();

        CaptureFrame(frame);
        m_frameCount++;
        return true;
    }

} // end namespace webrtc
} // end namespace unity
