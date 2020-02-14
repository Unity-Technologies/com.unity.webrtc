/*
 *  Copyright 2016 The WebRTC project authors. All Rights Reserved.
 *
 *  Use of this source code is governed by a BSD-style license
 *  that can be found in the LICENSE file in the root of the source
 *  tree. An additional intellectual property rights grant can be found
 *  in the file PATENTS.  All contributing project authors may
 *  be found in the AUTHORS file in the root of the source tree.
 */

#include "pch.h"
#include "VideoTrackSource.h"

namespace WebRTC {

    VideoTrackSource::VideoTrackSource(
        bool remote)
        : state_(kInitializing), remote_(remote) {
        sequence_checker_.Detach();
    }

    void VideoTrackSource::SetState(SourceState new_state) {
        if (state_ != new_state) {
            state_ = new_state;
            FireOnChanged();
        }
    }

    void VideoTrackSource::AddOrUpdateSink(
        rtc::VideoSinkInterface<webrtc::VideoFrame>* sink,
        const rtc::VideoSinkWants& wants) {
        RTC_DCHECK(sequence_checker_.IsCurrent());
        source()->AddOrUpdateSink(sink, wants);
    }

    void VideoTrackSource::RemoveSink(rtc::VideoSinkInterface<webrtc::VideoFrame>* sink) {
        RTC_DCHECK(sequence_checker_.IsCurrent());
        source()->RemoveSink(sink);
    }

}  //  namespace webrtc
