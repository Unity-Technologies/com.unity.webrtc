#include "pch.h"
#include "NvVideoCapturer.h"
#include "Codec/EncoderFactory.h"

namespace unity
{
namespace webrtc
{

    NvVideoCapturer::NvVideoCapturer()
    {
        set_enable_video_adapter(false);
        SetSupportedFormats(
            std::vector<cricket::VideoFormat>(1,
                cricket::VideoFormat(width,
                    height,
                    cricket::VideoFormat::FpsToInterval(framerate),
                    cricket::FOURCC_H264)));
    }

    bool NvVideoCapturer::EncodeVideoData()
    {
        if (captureStarted && !captureStopped)
        {
            if(encoder_ == nullptr)
            {
                LogPrint("encoder is null");
                return false;
            }
            if(!encoder_->CopyBuffer(unityRT))
            {
                LogPrint("Copy texture buffer is failed");
                return false;
            }
            if(!encoder_->EncodeFrame())
            {
                LogPrint("Encode frame is failed");
                return false;
            }
        }
        else {
            return false;
        }
        return true;
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
    CodecInitializationResult NvVideoCapturer::GetCodecInitializationResult() const
    {
        if(encoder_ == nullptr)
        {
            return CodecInitializationResult::NotInitialized;
        }
        return encoder_->GetCodecInitializationResult();
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

    void NvVideoCapturer::SetEncoder(IEncoder* encoder)
    {
        encoder_ = encoder;
        encoder_->CaptureFrame.connect(this, &NvVideoCapturer::CaptureFrame);
    }
    
} // end namespace webrtc
} // end namespace unity
