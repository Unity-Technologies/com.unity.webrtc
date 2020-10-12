#include "pch.h"
#include "UnityVideoTrackSource.h"

#include <mutex>

#include "DummyVideoEncoder.h"
#include "WebRTCPlugin.h"
#include "common_video/libyuv/include/webrtc_libyuv.h"

namespace unity
{
namespace webrtc
{

UnityVideoTrackSource::UnityVideoTrackSource(
    void* frame,
    UnityGfxRenderer gfxRenderer,
    bool is_screencast,
    absl::optional<bool> needs_denoising) :
    AdaptedVideoTrackSource(/*required_alignment=*/1),
    frame_(frame),
    encoder_(nullptr),
    is_screencast_(is_screencast),
    needs_denoising_(needs_denoising),
    on_encode_(nullptr)
{
//  DETACH_FROM_THREAD(thread_checker_);

#if defined(SUPPORT_VULKAN)
    if(gfxRenderer == kUnityGfxRendererVulkan)
    {
        unityVulkanImage_ = *static_cast<UnityVulkanImage*>(frame_);
        frame_ = &unityVulkanImage_;
    }
#endif
}

UnityVideoTrackSource::~UnityVideoTrackSource()
{
    {
        std::unique_lock<std::mutex> lock(m_mutex);
        std::unique_lock<std::mutex> onencode_lock(m_onEncodeMutex);
    }
};

UnityVideoTrackSource::SourceState UnityVideoTrackSource::state() const
{
  // TODO(nisse): What's supposed to change this state?
  return MediaSourceInterface::SourceState::kLive;
}

bool UnityVideoTrackSource::remote() const {
  return false;
}

bool UnityVideoTrackSource::is_screencast() const {
  return is_screencast_;
}

absl::optional<bool> UnityVideoTrackSource::needs_denoising() const
{
    return needs_denoising_;
}

void UnityVideoTrackSource::SetEncoder(IEncoder* encoder)
{
    encoder_ = encoder;
    encoder_->CaptureFrame.connect(
        this,
        &UnityVideoTrackSource::DelegateOnFrame);
}


void UnityVideoTrackSource::OnFrameCaptured()
{
    // todo::(kazuki)
    // OnFrame(frame);
    std::unique_lock<std::mutex> lock(m_mutex, std::try_to_lock);
    if (!lock.owns_lock()) {
        return;
    }
    if (encoder_ == nullptr)
    {
        LogPrint("encoder is null");
        return;
    }
    if (!encoder_->CopyBuffer(frame_))
    {
        LogPrint("Copy texture buffer is failed");
        return;
    }
    if (!encoder_->EncodeFrame())
    {
        LogPrint("Encode frame is failed");
        return;
    }
}

void UnityVideoTrackSource::DelegateOnFrame(const ::webrtc::VideoFrame& frame) {
    if (on_encode_ != nullptr) {
        std::unique_lock<std::mutex> lock(m_onEncodeMutex, std::try_to_lock);
        if (lock.owns_lock()) {

            int width = frame.width();
            int height = frame.height();

            auto i420Buffer = frame.video_frame_buffer()->ToI420();
            if (i420Buffer != nullptr) {
                auto bufferSz = ::webrtc::CalcBufferSize(::webrtc::VideoType::kI420, width, height);
                static std::vector<uint8_t> buffer(bufferSz);
                int extractSz = ::webrtc::ExtractBuffer(i420Buffer, bufferSz, buffer.data());

                if (extractSz != bufferSz) {
                    LogPrint("OnEncodeFrame i420 buffer extract size did not match expected size.");
                }
                else if (on_encode_ != nullptr) {
                    on_encode_(track_, encoder_->Id(), width, height, buffer.data(), bufferSz);
                }
            }
            else {
                // Hardware encoder is in use. Encoding has been done.
                auto encodedFrameBuffer = static_cast<unity::webrtc::FrameBuffer*>(frame.video_frame_buffer().get());
                if (encodedFrameBuffer != nullptr && encodedFrameBuffer->buffer().size() > 0) {
                    int bufferSz = encodedFrameBuffer->buffer().size();
                    //char msgbuf[100];
                    //sprintf(msgbuf, "Encoded buffer size is %d\n", encodedFrameBuffer->buffer().size());
                    //OutputDebugString(msgbuf);
                    //_onEncodedFrame(frame.width(), frame.height(), encodedFrameBuffer->buffer().data(), encodedFrameBuffer->buffer().size());
                    //OutputDebugString("OnEncodedFrame complete.\n");
                    if (on_encode_ != nullptr) {
                        on_encode_(track_, encoder_->Id(), width, height, nullptr, bufferSz);
                    }
                }
            }
        }
    }
    OnFrame(frame);
}

void UnityVideoTrackSource::SetVideoFrameOnEncodeCallback(::webrtc::MediaStreamTrackInterface* track, DelegateOnVideoFrameEncode callback)
{
    track_ = track;
    on_encode_ = callback;
    if (on_encode_ != nullptr) {
        on_encode_(track, 0, 640, 480, nullptr, -1);
    }
}

} // end namespace webrtc
} // end namespace unity
