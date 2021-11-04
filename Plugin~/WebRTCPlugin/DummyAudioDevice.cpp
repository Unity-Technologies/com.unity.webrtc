#include "pch.h"
#include "DummyAudioDevice.h"
#include "UnityAudioTrackSource.h"
#include "system_wrappers/include/sleep.h"

namespace unity
{
namespace webrtc
{

    bool DummyAudioDevice::PlayoutThreadProcess()
    {
        int64_t currentTime = rtc::TimeMillis();

        {
            if (audio_transport_ == nullptr) {
                return false;
            }

            if (!playing_) {
                return false;
            }

            if (lastCallRecordMillis_ == 0 || currentTime - lastCallRecordMillis_ >= 10) {
                lastCallRecordMillis_ = currentTime;

                const int kBytesPerSample = 2;
                const int kChannels = 2;

                const int kSamplingRate = 48000;

                // Webrtc uses 10ms frames.
                const int kFrameLengthMs = 10;
                const int kSamplesPerFrame = kSamplingRate * kFrameLengthMs / 1000;

                int64_t elapsed_time_ms = -1;
                int64_t ntp_time_ms = -1;
                char data[kBytesPerSample * kChannels * kSamplesPerFrame];

                (*audio_transport_).PullRenderData(kBytesPerSample * 8, kSamplingRate,
                    kChannels, kSamplesPerFrame, data,
                    &elapsed_time_ms, &ntp_time_ms);
            }
        }

        int64_t deltaTimeMillis = rtc::TimeMillis() - currentTime;
        if (deltaTimeMillis < 10) {
            SleepMs(10 - (deltaTimeMillis + 1));
        }
        return true;
    }

    void DummyAudioDevice::RecordingThread() {
        bool buffer_init = false;

        while (recording_) {
            const int64_t bgnTime = rtc::TimeMillis();
            {
                std::lock_guard<std::mutex> lock(mutex_);
                for (const auto &pair : callbacks_) {
                    pair.second();
                }
            }
            if (recording_) {
                const int64_t elapsed = rtc::TimeMillis() - bgnTime;
                if (elapsed < 10) {
                    SleepMs(10 - (elapsed + 1));
                }
            }
        }
    }

    void DummyAudioDevice::RegisterSendAudioCallback(UnityAudioTrackSource* source, int sampleRate, int channels) {
        if (callbacks_.find(source) == callbacks_.end()) {
            std::lock_guard<std::mutex> lock(mutex_);
            callbacks_.emplace(source, [source, sampleRate, channels]() {
                source->SendAudioData(sampleRate, channels); });
        }
    }

} // end namespace webrtc
} // end namespace unity
