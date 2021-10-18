#include "pch.h"
#include "DummyAudioDevice.h"
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

    void DummyAudioDevice::RecodingThread() {
        bool buffer_init = false;

        while (recording_) {
            const int64_t bgnTime = rtc::TimeMillis();
            {
                std::lock_guard<std::mutex> lock(mutex_);

                if (sampleRate_ > 0 && channels_ > 0) {
                    const size_t samplesFor10ms = sampleRate_ * channels_ / 100;

                    if (buffer_init == false && audioBuffer_.size() > samplesFor10ms * 10) {
                        audioBuffer_.erase(
                            audioBuffer_.begin(),
                            audioBuffer_.begin() + (audioBuffer_.size() - samplesFor10ms));
                        buffer_init = true;
                    }

                    if (buffer_init) {
                        if (audioBuffer_.size() >= samplesFor10ms) {
                            constexpr int bytePerSample = sizeof(decltype(audioBuffer_)::value_type);

                            uint32_t newMicLev = 0;
                            if (audio_transport_) {
                                (*audio_transport_).RecordedDataIsAvailable(audioBuffer_.data(),
                                    sampleRate_ / 100, bytePerSample, channels_, sampleRate_,
                                    0, 0, 0, false, newMicLev);
                            }

                            audioBuffer_.erase(audioBuffer_.begin(), audioBuffer_.begin() + samplesFor10ms);
                        }
                    }
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

    void DummyAudioDevice::InitLocalAudio(int sampleRate, int channels) {
        std::lock_guard<std::mutex> lock(mutex_);

        sampleRate_ = sampleRate;
        channels_ = channels;
    }
    
    void DummyAudioDevice::PushLocalAudio(const float* audioData, int sampleRate, int channels, int numFrames) {
        std::lock_guard<std::mutex> lock(mutex_);

        if (sampleRate_ == 0 || channels_ == 0)
            return;

        audioBuffer_.reserve(audioBuffer_.size() + numFrames);
        for (size_t i = 0; i < numFrames; i++)
        {
            audioBuffer_.push_back(audioData[i] >= 0 ? audioData[i] * SHRT_MAX : audioData[i] * -SHRT_MIN);
        }
    }

} // end namespace webrtc
} // end namespace unity
