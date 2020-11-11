#include "pch.h"
#include "SoftwareEncoder.h"
#include "GraphicsDevice/IGraphicsDevice.h"
#include "GraphicsDevice/ITexture2D.h"

#if defined(_WIN32)
#else
#include <dlfcn.h>
#endif

namespace unity
{
namespace webrtc
{

    SoftwareEncoder::SoftwareEncoder(int width, int height, IGraphicsDevice* device, UnityRenderingExtTextureFormat textureFormat) :
    m_device(device),
    m_width(width),
    m_height(height),
    m_encodeTex(nullptr),
    m_textureFormat(textureFormat)
    {
    }

    SoftwareEncoder::~SoftwareEncoder()
    {
        delete m_encodeTex;
        m_encodeTex = nullptr;
    }

    void SoftwareEncoder::InitV()
    {
        m_encodeTex = m_device->CreateCPUReadTextureV(m_width, m_height, m_textureFormat);
        m_initializationResult = CodecInitializationResult::Success;
    }

    bool SoftwareEncoder::CopyBuffer(void* frame)
    {
        m_device->CopyResourceFromNativeV(m_encodeTex, frame);
        return true;
    }

    bool SoftwareEncoder::EncodeFrame(int64_t timestamp_us)
    {
        const rtc::scoped_refptr<webrtc::I420Buffer> i420Buffer = m_device->ConvertRGBToI420(m_encodeTex);
        if (nullptr == i420Buffer)
            return false;

        webrtc::VideoFrame frame =
            webrtc::VideoFrame::Builder()
            .set_video_frame_buffer(i420Buffer)
            .set_rotation(webrtc::kVideoRotation_0)
            .set_timestamp_us(timestamp_us)
            .build();
            
        CaptureFrame(frame);
        m_frameCount++;
        return true;
    }
    
} // end namespace webrtc
} // end namespace unity
