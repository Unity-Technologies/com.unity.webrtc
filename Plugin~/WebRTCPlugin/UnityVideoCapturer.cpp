#include "pch.h"
#include "UnityVideoCapturer.h"
#include "UnityEncoder.h"

#include "GraphicsDevice/GraphicsDevice.h"
#include "GraphicsDevice/ITexture2D.h"
#include "WebRTCMacros.h"

namespace WebRTC
{
    UnityVideoCapturer::UnityVideoCapturer(UnityEncoder* pEncoder, int _width, int _height, void* unityNativeTexPtr)
        : nvEncoder(pEncoder), width(_width), height(_height)
    {       
        m_unityRT = GraphicsDevice::GetInstance().CreateDefaultTextureFromNative(_width, _height, unityNativeTexPtr);

        set_enable_video_adapter(false);
        SetSupportedFormats(std::vector<cricket::VideoFormat>(1, cricket::VideoFormat(width, height, cricket::VideoFormat::FpsToInterval(framerate), cricket::FOURCC_H264)));
    }

    UnityVideoCapturer::~UnityVideoCapturer() {
        SAFE_DELETE(m_unityRT);

    }

    void UnityVideoCapturer::EncodeVideoData()
    {
        if (captureStarted && !captureStopped)
        {
            GraphicsDevice::GetInstance().CopyResource(nvEncoder->getRenderTexture(),m_unityRT);
            nvEncoder->EncodeFrame(width, height);
        }
    }
    void UnityVideoCapturer::CaptureFrame(std::vector<uint8>& data)
    {
        rtc::scoped_refptr<FrameBuffer> buffer = new rtc::RefCountedObject<FrameBuffer>(width, height, data);
        int64 timestamp = rtc::TimeMillis();
        webrtc::VideoFrame videoFrame{buffer, webrtc::VideoRotation::kVideoRotation_0, timestamp};
        videoFrame.set_ntp_time_ms(timestamp);
        OnFrame(videoFrame, width, height);
    }
    void UnityVideoCapturer::StartEncoder()
    {
        captureStarted = true;
        SetKeyFrame();
    }
    void UnityVideoCapturer::SetKeyFrame()
    {
        nvEncoder->SetIdrFrame();
    }
    void UnityVideoCapturer::SetRate(uint32 rate)
    {
        nvEncoder->SetRate(rate);
    }

    void UnityVideoCapturer::InitializeEncoder()
    {
        nvEncoder->captureFrame.connect(this, &UnityVideoCapturer::CaptureFrame);
    }
}
