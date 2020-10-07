#pragma once

#include <mutex>
#include "Codec/IEncoder.h"
#include "rtc_base/timestamp_aligner.h"

namespace unity {
namespace webrtc {

class IEncoder;

// This class implements webrtc's VideoTrackSourceInterface. To pass frames down
// the webrtc video pipeline, each received a media::VideoFrame is converted to
// a webrtc::VideoFrame, taking any adaptation requested by downstream classes
// into account.
class UnityVideoTrackSource :
    public rtc::AdaptedVideoTrackSource,
    public sigslot::has_slots<>
{
    public:
        struct FrameAdaptationParams
        {
            bool should_drop_frame;
            int crop_x;
            int crop_y;
            int crop_width;
            int crop_height;
            int scale_to_width;
            int scale_to_height;
        };

    UnityVideoTrackSource(
        void* frame,
        UnityGfxRenderer gfxRenderer,
        bool is_screencast,
        absl::optional<bool> needs_denoising);
    ~UnityVideoTrackSource() override;

    SourceState state() const override;

    bool remote() const override;
    bool is_screencast() const override;
    absl::optional<bool> needs_denoising() const override;

    // todo(kazuki)::
    void OnFrameCaptured();

    // todo(kazuki)::
    void DelegateOnFrame(const ::webrtc::VideoFrame& frame) { OnFrame(frame); }

    // todo(kazuki)::
    void SetEncoder(IEncoder* encoder);

    // todo(kazuki)::
    CodecInitializationResult GetCodecInitializationResult() const
    {
        if (encoder_ == nullptr)
        {
            return CodecInitializationResult::NotInitialized;
        }
        return encoder_->GetCodecInitializationResult();
    }

    using ::webrtc::VideoTrackSourceInterface::AddOrUpdateSink;
    using ::webrtc::VideoTrackSourceInterface::RemoveSink;

 private:
    FrameAdaptationParams ComputeAdaptationParams(
        int width, int height, int64_t time_us);

    // Delivers |frame| to base class method
    // rtc::AdaptedVideoTrackSource::OnFrame(). If the cropping (given via
    // |frame->visible_rect()|) has changed since the last delivered frame, the
    // whole frame is marked as updated.
    // void DeliverFrame(rtc::scoped_refptr<::webrtc::VideoFrame> frame,
    //                  gfx::Rect* update_rect,
    //                  int64_t timestamp_us);

    // |thread_checker_| is bound to the libjingle worker thread.
    // THREAD_CHECKER(thread_checker_);
    // media::VideoFramePool scaled_frame_pool_;
    // State for the timestamp translation.
    rtc::TimestampAligner timestamp_aligner_;

    const bool is_screencast_;
    const absl::optional<bool> needs_denoising_;

    std::mutex m_mutex;
    IEncoder* encoder_;
    void* frame_;

#if defined(SUPPORT_VULKAN)
    UnityVulkanImage unityVulkanImage_;
#endif
};

} // end namespace webrtc
} // end namespace unity
