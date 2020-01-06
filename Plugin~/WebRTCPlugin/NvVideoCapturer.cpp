﻿#include "pch.h"
#include "NvVideoCapturer.h"
#include "Codec/EncoderFactory.h"

namespace WebRTC
{
    NvVideoCapturer::NvVideoCapturer()
    {
        set_enable_video_adapter(false);
        SetSupportedFormats(std::vector<cricket::VideoFormat>(1, cricket::VideoFormat(width, height, cricket::VideoFormat::FpsToInterval(framerate), cricket::FOURCC_H264)));
    }
    void NvVideoCapturer::EncodeVideoData()
    {
        if (captureStarted && !captureStopped)
        {
            if(encoder_ == nullptr)
            {
                LogPrint("nvEncoder is null");
                return;
            }
            if(!encoder_->CopyBuffer(unityRT))
            {
                LogPrint("CopyRenderTexture Failed");
                return;
            }
            encoder_->EncodeFrame();
        }
        else
        {
            LogPrint("Video capture is not started");
        }
    }

    void NvVideoCapturer::CaptureFrame(webrtc::VideoFrame& videoFrame)
    {
        OnFrame(videoFrame, width, height);
    }

    void NvVideoCapturer::StartEncoder()
    {
        captureStarted = true;
        SetKeyFrame();
    }
    void NvVideoCapturer::SetFrameBuffer(void* frameBuffer)
    {
        unityRT = frameBuffer;
    }

    void NvVideoCapturer::SetSize(int32 width, int32 height)
    {
        this->width = width;
        this->height = height;
    }

    void NvVideoCapturer::SetKeyFrame()
    {
        encoder_->SetIdrFrame();
    }
    void NvVideoCapturer::SetRate(uint32 rate)
    {
        encoder_->SetRate(rate);
    }

    bool NvVideoCapturer::InitializeEncoder(IGraphicsDevice* device, UnityEncoderType encoderType)
    {
        try
        {
            EncoderFactory::GetInstance().Init(width, height, device, encoderType);
        }
        catch(std::runtime_error& exception)
        {
            LogPrint(exception.what());
            return false;
        }
        encoder_ = EncoderFactory::GetInstance().GetEncoder();
        if (encoder_ == nullptr)
            return false;
        encoder_->CaptureFrame.connect(this, &NvVideoCapturer::CaptureFrame);
        return true;
    }

    void NvVideoCapturer::FinalizeEncoder()
    {
        EncoderFactory::GetInstance().Shutdown();
    }
}
