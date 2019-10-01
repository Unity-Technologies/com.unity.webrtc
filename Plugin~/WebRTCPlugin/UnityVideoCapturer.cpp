﻿#include "pch.h"
#include "UnityVideoCapturer.h"
#include "UnityEncoder.h"

namespace WebRTC
{
    UnityVideoCapturer::UnityVideoCapturer(UnityEncoder* pEncoder, int _width, int _height) : nvEncoder(pEncoder), width(_width), height(_height)
    {
        set_enable_video_adapter(false);
        SetSupportedFormats(std::vector<cricket::VideoFormat>(1, cricket::VideoFormat(width, height, cricket::VideoFormat::FpsToInterval(framerate), cricket::FOURCC_H264)));
    }
    void UnityVideoCapturer::EncodeVideoData()
    {
        if (captureStarted && !captureStopped)
        {
            context->CopyResource((ID3D11Resource*)nvEncoder->getRenderTexture(), unityRT);
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
