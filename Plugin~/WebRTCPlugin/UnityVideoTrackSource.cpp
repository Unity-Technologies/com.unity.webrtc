#include "pch.h"
#include "UnityVideoTrackSource.h"

#include <mutex>

#include "Codec/IEncoder.h"

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
    needs_denoising_(needs_denoising)
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

} // end namespace webrtc
} // end namespace unity
