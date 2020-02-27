#pragma once

/*
 *  Copyright 2016 The WebRTC project authors. All Rights Reserved.
 *
 *  Use of this source code is governed by a BSD-style license
 *  that can be found in the LICENSE file in the root of the source
 *  tree. An additional intellectual property rights grant can be found
 *  in the file PATENTS.  All contributing project authors may
 *  be found in the AUTHORS file in the root of the source tree.
 */

#include "api/media_stream_interface.h"
#include "api/notifier.h"
#include "api/video/video_sink_interface.h"
#include "media/base/media_channel.h"
#include "rtc_base/system/rtc_export.h"
#include "rtc_base/thread_checker.h"

namespace WebRTC {

    // VideoTrackSource is a convenience base class for implementations of
    // VideoTrackSourceInterface.
    class RTC_EXPORT VideoTrackSource : public webrtc::Notifier<webrtc::VideoTrackSourceInterface> {
    public:
        explicit VideoTrackSource(bool remote);
        void SetState(SourceState new_state);

        SourceState state() const override { return state_; }
        bool remote() const override { return remote_; }

        bool is_screencast() const override { return false; }
        absl::optional<bool> needs_denoising() const override {
            return absl::nullopt;
        }

        bool GetStats(Stats* stats) override { return false; }

        void AddOrUpdateSink(rtc::VideoSinkInterface<webrtc::VideoFrame>* sink,
            const rtc::VideoSinkWants& wants) override;
        void RemoveSink(rtc::VideoSinkInterface<webrtc::VideoFrame>* sink) override;

    protected:
        virtual rtc::VideoSourceInterface<webrtc::VideoFrame>* source() = 0;

    private:
        webrtc::SequenceChecker sequence_checker_;
        rtc::ThreadChecker worker_thread_checker_;
        SourceState state_;
        const bool remote_;
    };

}
