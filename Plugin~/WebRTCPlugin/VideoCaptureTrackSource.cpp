/*
 *  Copyright 2012 The WebRTC project authors. All Rights Reserved.
 *
 *  Use of this source code is governed by a BSD-style license
 *  that can be found in the LICENSE file in the root of the source
 *  tree. An additional intellectual property rights grant can be found
 *  in the file PATENTS.  All contributing project authors may
 *  be found in the AUTHORS file in the root of the source tree.
 */

#include "pch.h"
#include "VideoCaptureTrackSource.h"

namespace {

    // Minimum interval is 10k fps.
    #define FPS_TO_INTERVAL(fps) \
    (fps ? rtc::kNumNanosecsPerSec / fps : rtc::kNumNanosecsPerSec / 10000)

    const double kRoundingTruncation = 0.0005;

    // Default resolution. If no constraint is specified, this is the resolution we
    // will use.
    static const cricket::VideoFormatPod kDefaultFormat = {
        640, 480, FPS_TO_INTERVAL(30), cricket::FOURCC_ANY };

    // List of formats used if the camera doesn't support capability enumeration.
    static const cricket::VideoFormatPod kVideoFormats[] = {
        {1920, 1080, FPS_TO_INTERVAL(30), cricket::FOURCC_ANY},
        {1280, 720, FPS_TO_INTERVAL(30), cricket::FOURCC_ANY},
        {960, 720, FPS_TO_INTERVAL(30), cricket::FOURCC_ANY},
        {640, 360, FPS_TO_INTERVAL(30), cricket::FOURCC_ANY},
        {640, 480, FPS_TO_INTERVAL(30), cricket::FOURCC_ANY},
        {320, 240, FPS_TO_INTERVAL(30), cricket::FOURCC_ANY},
        {320, 180, FPS_TO_INTERVAL(30), cricket::FOURCC_ANY} };

    webrtc::MediaSourceInterface::SourceState GetReadyState(cricket::CaptureState state) {
        switch (state) {
        case cricket::CS_STARTING:
            return webrtc::MediaSourceInterface::kInitializing;
        case cricket::CS_RUNNING:
            return webrtc::MediaSourceInterface::kLive;
        case cricket::CS_FAILED:
        case cricket::CS_STOPPED:
            return webrtc::MediaSourceInterface::kEnded;
        default:
            RTC_NOTREACHED() << "GetReadyState unknown state";
        }
        return webrtc::MediaSourceInterface::kEnded;
    }

    void SetUpperLimit(int new_limit, int* original_limit) {
        if (*original_limit < 0 || new_limit < *original_limit)
            *original_limit = new_limit;
    }

}  // anonymous namespace

namespace webrtc
{

    rtc::scoped_refptr<webrtc::VideoTrackSourceInterface> VideoCapturerTrackSource::Create(
        rtc::Thread* worker_thread,
        std::unique_ptr<cricket::VideoCapturer> capturer,
        bool remote) {
        RTC_DCHECK(worker_thread != NULL);
        RTC_DCHECK(capturer != nullptr);
        rtc::scoped_refptr<VideoCapturerTrackSource> source(
            new rtc::RefCountedObject<VideoCapturerTrackSource>(
                worker_thread, std::move(capturer), remote));
        source->Initialize(nullptr);
        return source;
    }

    VideoCapturerTrackSource::VideoCapturerTrackSource(
        rtc::Thread* worker_thread,
        std::unique_ptr<cricket::VideoCapturer> capturer,
        bool remote)
        : VideoTrackSource(remote),
        signaling_thread_(rtc::Thread::Current()),
        worker_thread_(worker_thread),
        video_capturer_(std::move(capturer)),
        started_(false) {
        video_capturer_->SignalStateChange.connect(
            this, &VideoCapturerTrackSource::OnStateChange);
    }

    VideoCapturerTrackSource::~VideoCapturerTrackSource() {
        video_capturer_->SignalStateChange.disconnect(this);
        Stop();
    }

    void VideoCapturerTrackSource::Initialize(
        const webrtc::MediaConstraintsInterface* constraints) {
        std::vector<cricket::VideoFormat> formats = *video_capturer_->GetSupportedFormats();
        if (formats.empty()) {
            if (video_capturer_->IsScreencast()) {
                // The screen capturer can accept any resolution and we will derive the
                // format from the constraints if any.
                // Note that this only affects tab capturing, not desktop capturing,
                // since the desktop capturer does not respect the VideoFormat passed in.
                formats.push_back(cricket::VideoFormat(kDefaultFormat));
            }
            else {
                // The VideoCapturer implementation doesn't support capability
                // enumeration. We need to guess what the camera supports.
                for (uint32_t i = 0; i < arraysize(kVideoFormats); ++i) {
                    formats.push_back(cricket::VideoFormat(kVideoFormats[i]));
                }
            }
        }

        if (formats.size() == 0) {
            RTC_LOG(LS_WARNING) << "Failed to find a suitable video format.";
            SetState(kEnded);
            return;
        }

        // Start the camera with our best guess.
        if (!worker_thread_->Invoke<bool>(
            RTC_FROM_HERE, rtc::Bind(&cricket::VideoCapturer::StartCapturing,
                video_capturer_.get(), format_))) {
            SetState(kEnded);
            return;
        }
        started_ = true;
        // Initialize hasn't succeeded until a successful state change has occurred.
    }

    bool VideoCapturerTrackSource::GetStats(Stats* stats) {
        return video_capturer_->GetInputSize(&stats->input_width,
            &stats->input_height);
    }

    void VideoCapturerTrackSource::Stop() {
        if (!started_) {
            return;
        }
        started_ = false;
        worker_thread_->Invoke<void>(
            RTC_FROM_HERE,
            rtc::Bind(&cricket::VideoCapturer::Stop, video_capturer_.get()));
    }

    // OnStateChange listens to the cricket::VideoCapturer::SignalStateChange.
    void VideoCapturerTrackSource::OnStateChange(
        cricket::VideoCapturer* capturer,
        cricket::CaptureState capture_state) {
        if (rtc::Thread::Current() != signaling_thread_) {
            // Use rtc::Unretained, because we don't want this to capture a reference
            // to ourselves. If our destructor is called while this task is executing,
            // that's fine; our AsyncInvoker destructor will wait for it to finish if
            // it isn't simply canceled.
            invoker_.AsyncInvoke<void>(
                RTC_FROM_HERE, signaling_thread_,
                rtc::Bind(&VideoCapturerTrackSource::OnStateChange,
                    rtc::Unretained(this), capturer, capture_state));
            return;
        }

        if (capturer == video_capturer_.get()) {
            SetState(GetReadyState(capture_state));
        }
    }
}  // namespace webrtc
