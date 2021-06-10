// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

#pragma once

#include <mutex>

#include "WebRTCPlugin.h"


namespace unity {
namespace webrtc {
    namespace webrtc = ::webrtc;

/// Audio frame observer to get notified of newly available audio frames.
class UnityAudioFrameObserver : public webrtc::AudioTrackSinkInterface {
 public:
  UnityAudioFrameObserver(uint32_t id);
  ~UnityAudioFrameObserver() override;
  void RegisterOnFrameReady(DelegateAudioFrameObserverOnFrameReady callback) noexcept;
  uint32_t GetId() const;

 protected:
  // AudioTrackSinkInterface interface
  void OnData(const void* audio_data,
              int bits_per_sample,
              int sample_rate,
              size_t number_of_channels,
              size_t number_of_frames) noexcept override;

 private:
  uint32_t m_id;
  std::mutex m_mutex;
  DelegateAudioFrameObserverOnFrameReady on_frame_ready_ RTC_GUARDED_BY(m_mutex) = nullptr;
  
};

}
} 

