#include "pch.h"
#include "SoftwareEncoder.h"
#include "Context.h"
#include <cstring>
#include "GraphicsDevice/IGraphicsDevice.h"

#if _WIN32
#else
#include <dlfcn.h>
#endif

namespace WebRTC
{
    SoftwareEncoder::SoftwareEncoder(int _width, int _height, IGraphicsDevice* device) : m_width(_width), m_height(_height), m_device(device)
    {

    }

    void SoftwareEncoder::InitV()
    {
        m_encodeTex = m_device->CreateCPUReadTextureV(m_width, m_height);
    }

    bool SoftwareEncoder::CopyBuffer(void* frame)
    {
        m_device->CopyResourceFromNativeV(m_encodeTex, frame);
        return true;
    }

    bool SoftwareEncoder::EncodeFrame()
    {
        webrtc::VideoFrame frame = webrtc::VideoFrame::Builder().set_video_frame_buffer(m_device->ConvertRGBToI420(m_encodeTex)).set_rotation(webrtc::kVideoRotation_0).set_timestamp_us(0).build();
        CaptureFrame(frame);
        return true;
    }
}
