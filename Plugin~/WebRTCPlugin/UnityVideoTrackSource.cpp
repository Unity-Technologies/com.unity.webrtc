#include "pch.h"
#include "UnityVideoTrackSource.h"

#include <mutex>

#include "Codec/IEncoder.h"

namespace unity
{
namespace webrtc
{

UnityVideoTrackSource::UnityVideoTrackSource(
    bool is_screencast,
    absl::optional<bool> needs_denoising) :
    AdaptedVideoTrackSource(/*required_alignment=*/1),
    is_screencast_(is_screencast),
    needs_denoising_(needs_denoising),
    encoder_(nullptr)
{
//  DETACH_FROM_THREAD(thread_checker_);
}

UnityVideoTrackSource::~UnityVideoTrackSource()
{
    {
        std::unique_lock<std::mutex> lock(m_mutex);
    }
}

void UnityVideoTrackSource::Init(void* frame)
{
    frame_ = frame;
}

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

CodecInitializationResult UnityVideoTrackSource::GetCodecInitializationResult() const
{
    if (encoder_ == nullptr)
    {
        return CodecInitializationResult::NotInitialized;
    }
    return encoder_->GetCodecInitializationResult();
}

void UnityVideoTrackSource::SetEncoder(IEncoder* encoder)
{
    encoder_ = encoder;
    encoder_->CaptureFrame.connect(
        this,
        &UnityVideoTrackSource::DelegateOnFrame);
}


void UnityVideoTrackSource::OnFrameCaptured(int64_t timestamp_us)
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
    if (!encoder_->EncodeFrame(timestamp_us))
    {
        LogPrint("Encode frame is failed");
        return;
    }
}

} // end namespace webrtc
} // end namespace unity
