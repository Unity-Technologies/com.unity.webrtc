#pragma once

#include <mutex>

#include <absl/types/optional.h>
#include <api/media_stream_interface.h>
#include <api/task_queue/task_queue_factory.h>
#include <media/base/adapted_video_track_source.h>
#include <rtc_base/task_queue.h>

#include "VideoFrame.h"

namespace unity
{
namespace webrtc
{

    using namespace ::webrtc;

    // This class implements webrtc's VideoTrackSourceInterface. To pass frames down
    // the webrtc video pipeline, each received a media::VideoFrame is converted to
    // a webrtc::VideoFrame, taking any adaptation requested by downstream classes
    // into account.
    class VideoFrameScheduler;
    class UnityVideoTrackSource : public rtc::AdaptedVideoTrackSource
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
            bool is_screencast, absl::optional<bool> needs_denoising, TaskQueueFactory* taskQueueFactory);
        ~UnityVideoTrackSource() override;
        SourceState state() const override;

        bool remote() const override;
        bool is_screencast() const override;
        absl::optional<bool> needs_denoising() const override;
        bool syncApplicationFramerate() const { return syncApplicationFramerate_; };
        void OnFrameCaptured(rtc::scoped_refptr<VideoFrame> frame);
        void SetSyncApplicationFramerate(bool value);
        using VideoTrackSourceInterface::AddOrUpdateSink;
        using VideoTrackSourceInterface::RemoveSink;

        static rtc::scoped_refptr<UnityVideoTrackSource>
        Create(bool is_screencast, absl::optional<bool> needs_denoising, TaskQueueFactory* taskQueueFactory);

    private:
        void OnUpdateVideoFrame();
        void CaptureVideoFrame();
        void SendFeedback();
        FrameAdaptationParams ComputeAdaptationParams(int width, int height, int64_t time_us);

        // Delivers |frame| to base class method
        // rtc::AdaptedVideoTrackSource::OnFrame(). If the cropping (given via
        // |frame->visible_rect()|) has changed since the last delivered frame, the
        // whole frame is marked as updated.
        // void DeliverFrame(rtc::scoped_refptr<::webrtc::VideoFrame> frame,
        //                  gfx::Rect* update_rect,
        //                  int64_t timestamp_us);

        // |thread_checker_| is bound to the libjingle worker thread.
        // todo::(kazuki) change compiler vc to clang
        // media::VideoFramePool scaled_frame_pool_;

        // State for the timestamp translation.
        rtc::TimestampAligner timestamp_aligner_;

        const bool is_screencast_;
        const absl::optional<bool> needs_denoising_;
        std::mutex mutex_;

        std::unique_ptr<rtc::TaskQueue> taskQueue_;
        std::unique_ptr<VideoFrameScheduler> scheduler_;
        rtc::scoped_refptr<unity::webrtc::VideoFrame> frame_;
        bool syncApplicationFramerate_;
    };

} // end namespace webrtc
} // end namespace unity
