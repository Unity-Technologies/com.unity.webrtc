#include "pch.h"
#include "UnityVideoCapturer.h"
#include "UnityEncoder.h"
#include "GraphicsDevice/D3D11/D3D11Texture2D.h"

namespace WebRTC
{
    //d3d11 context
    extern ID3D11DeviceContext* context;


    UnityVideoCapturer::UnityVideoCapturer(UnityEncoder* pEncoder, int _width, int _height, void* unityNativeTexPtr)
        : nvEncoder(pEncoder), width(_width), height(_height)
    {
        
        //[TODO-Sin: 2019-19-11] ITexture2D should not be created directly, but should be called using
        //GraphicsDevice->CreateEncoderInputTexture
        m_unityRT = new D3D11Texture2D(_width, _height, reinterpret_cast<ID3D11Texture2D*>(unityNativeTexPtr));

        set_enable_video_adapter(false);
        SetSupportedFormats(std::vector<cricket::VideoFormat>(1, cricket::VideoFormat(width, height, cricket::VideoFormat::FpsToInterval(framerate), cricket::FOURCC_H264)));
    }
    void UnityVideoCapturer::EncodeVideoData()
    {
        if (captureStarted && !captureStopped)
        {
            //[TODO-Sin: 2019-19-11] Use GraphicsDevice for this
            context->CopyResource((ID3D11Resource*)nvEncoder->getRenderTexture(), reinterpret_cast<ID3D11Texture2D*>(m_unityRT->GetNativeTexturePtrV()));
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
