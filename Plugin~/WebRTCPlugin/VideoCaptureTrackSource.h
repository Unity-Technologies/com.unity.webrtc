#pragma once

/*
 *  Copyright 2012 The WebRTC project authors. All Rights Reserved.
 *
 *  Use of this source code is governed by a BSD-style license
 *  that can be found in the LICENSE file in the root of the source
 *  tree. An additional intellectual property rights grant can be found
 *  in the file PATENTS.  All contributing project authors may
 *  be found in the AUTHORS file in the root of the source tree.
 */

#include <memory>

#include "api/media_stream_interface.h"
#include "media/base/video_common.h"
#include "rtc_base/async_invoker.h"
#include "rtc_base/third_party/sigslot/sigslot.h"

#include "VideoTrackSource.h"
#include "VideoCapturer.h"

 // VideoCapturerTrackSource implements VideoTrackSourceInterface. It owns a
 // cricket::VideoCapturer and make sure the camera is started at a resolution
 // that honors the constraints.
 // The state is set depending on the result of starting the capturer.
 // If the constraint can't be met or the capturer fails to start, the state
 // transition to kEnded, otherwise it transitions to kLive.

namespace webrtc {
    class MediaConstraintsInterface;
 }

namespace webrtc {

    class VideoCapturerTrackSource : public unity::webrtc::VideoTrackSource,
        public sigslot::has_slots<> {
    public:
        static rtc::scoped_refptr<VideoTrackSourceInterface> Create(
            rtc::Thread* worker_thread,
            std::unique_ptr<cricket::VideoCapturer> capturer,
            bool remote);

        bool is_screencast() const final { return video_capturer_->IsScreencast(); }
        absl::optional<bool> needs_denoising() const final {
            return needs_denoising_;
        }

        bool GetStats(Stats* stats) final;

    protected:
        VideoCapturerTrackSource(rtc::Thread* worker_thread,
            std::unique_ptr<cricket::VideoCapturer> capturer,
            bool remote);
        virtual ~VideoCapturerTrackSource();

        rtc::VideoSourceInterface<webrtc::VideoFrame>* source() override {
            return video_capturer_.get();
        }

        void Initialize(const webrtc::MediaConstraintsInterface* constraints);

    private:
        void Stop();

        void OnStateChange(cricket::VideoCapturer* capturer,
            cricket::CaptureState capture_state);

        rtc::Thread* signaling_thread_;
        rtc::Thread* worker_thread_;
        rtc::AsyncInvoker invoker_;
        std::unique_ptr<cricket::VideoCapturer> video_capturer_;
        bool started_;
        cricket::VideoFormat format_;
        absl::optional<bool> needs_denoising_;
    };

} // end namespace webrtc
