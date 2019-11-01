#include "pch.h"
#include "NvVideoCapturer.h"
#if _WIN32
#else
#include <GL/glew.h>
#endif

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
            int curFrameNum = nvEncoder->GetCurrentFrameCount() % bufferedFrameNum;
            auto dst = renderTextures[curFrameNum];

            if(!CopyRenderTexture(dst, unityRT))
            {
                LogPrint("CopyRenderTexture Failed");
                return;
            }
            if(nvEncoder == nullptr)
            {
                LogPrint("nvEncoder is null");
                return;
            }
            nvEncoder->EncodeFrame();
        }
        else
        {
            LogPrint("Video capture is not started");
        }
    }

    bool NvVideoCapturer::CopyRenderTexture(void*& dst, UnityFrameBuffer*& src)
    {
#if _WIN32
        context->CopyResource(dst, src);
#else
        auto tex = static_cast<NV_ENC_INPUT_RESOURCE_OPENGL_TEX*>(dst);

        GLuint srcName = (GLuint)(size_t)(src);
        GLuint dstName = tex->texture;

        if(glIsTexture(srcName) == GL_FALSE)
        {
            LogPrint("srcName is not texture");
            return false;
        }

        if(glIsTexture(dstName) == GL_FALSE)
        {
            LogPrint("dstName is not texture");
            return false;
        }
        glCopyImageSubData(
            srcName, GL_TEXTURE_2D, 0, 0, 0, 0,
            dstName, GL_TEXTURE_2D, 0, 0, 0, 0,
            width, height, 1);
#endif
        return true;
    }

    void NvVideoCapturer::CaptureFrame(std::vector<uint8>& data)
    {
        rtc::scoped_refptr<FrameBuffer> buffer = new rtc::RefCountedObject<FrameBuffer>(width, height, data);
        int64 timestamp = rtc::TimeMillis();
        webrtc::VideoFrame videoFrame{buffer, webrtc::VideoRotation::kVideoRotation_0, timestamp};
        videoFrame.set_ntp_time_ms(timestamp);
        OnFrame(videoFrame, width, height);
    }
    void NvVideoCapturer::StartEncoder()
    {
        captureStarted = true;
        SetKeyFrame();
    }
    void NvVideoCapturer::SetFrameBuffer(UnityFrameBuffer* frameBuffer)
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
        nvEncoder->SetIdrFrame();
    }
    void NvVideoCapturer::SetRate(uint32 rate)
    {
        nvEncoder->SetRate(rate);
    }

    void NvVideoCapturer::InitializeEncoder()
    {
        nvEncoder = std::make_unique<NvEncoder>(width, height);
        nvEncoder->CaptureFrame.connect(this, &NvVideoCapturer::CaptureFrame);
    }

    void NvVideoCapturer::FinalizeEncoder()
    {
        nvEncoder.release();
    }
}
