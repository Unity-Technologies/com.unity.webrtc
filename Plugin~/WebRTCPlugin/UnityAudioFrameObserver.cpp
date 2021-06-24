// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license
// information.

#include "pch.h"

#include <fstream>

#include "UnityAudioFrameObserver.h"

namespace unity {
    namespace webrtc {

// std::ofstream stream(LR"(D:\test_cpp.pcm)", std::ios_base::binary);

UnityAudioFrameObserver::UnityAudioFrameObserver(uint32_t id) : m_id(id)
{
    DebugLog("Create UnityAudioFrameObserver Id:%d", id);
}

UnityAudioFrameObserver::~UnityAudioFrameObserver()
{
    DebugLog("Destory UnityAudioFrameObserver Id:%d", m_id);
    {
        std::lock_guard<std::mutex> lock(m_mutex);
        // handled_frames = 0;
    }
}

void UnityAudioFrameObserver::RegisterOnFrameReady(
    DelegateAudioFrameObserverOnFrameReady callback) noexcept {
  std::lock_guard<std::mutex> lock(m_mutex);
  on_frame_ready_ = callback;
}

uint32_t UnityAudioFrameObserver::GetId() const
{
    return m_id;
}


void UnityAudioFrameObserver::OnData(const void* audio_data,
                                int bits_per_sample,
                                int sample_rate,
                                size_t number_of_channels,
                                size_t number_of_frames) {
  std::lock_guard<std::mutex> lock(m_mutex);
  if (!on_frame_ready_) {
    return;
  }
  AudioFrame frame;
  frame.data_ = audio_data;
  frame.bits_per_sample_ = static_cast<uint32_t>(bits_per_sample);
  frame.sampling_rate_hz_ = static_cast<uint32_t>(sample_rate);
  frame.channel_count_ = static_cast<uint32_t>(number_of_channels);
  frame.sample_count_ = static_cast<uint32_t>(number_of_frames);

  // const char* audio_data_char = reinterpret_cast<const char*>(audio_data);
  // size_t number_of_bytes = number_of_channels * number_of_frames * (bits_per_sample / 8);
  // stream.write(audio_data_char, number_of_bytes);
  // stream.flush();

  on_frame_ready_(this, frame);
}

}
} 

