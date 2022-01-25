#include "pch.h"
#include "DummyAudioDevice.h"
#include "UnityAudioTrackSource.h"
#include "system_wrappers/include/sleep.h"

namespace unity
{
namespace webrtc
{
    DummyAudioDevice::DummyAudioDevice(TaskQueueFactory* taskQueueFactory)
        : tackQueueFactory_(taskQueueFactory)
    {
    }

    int32_t DummyAudioDevice::Init()
    {
        taskQueue_ = std::make_unique<rtc::TaskQueue>(
            tackQueueFactory_->CreateTaskQueue(
                "AudioDevice", TaskQueueFactory::Priority::NORMAL));
        task_ = RepeatingTaskHandle::Start(taskQueue_->Get(), [this]() {
            ProcessAudio();
            return TimeDelta::Millis(kFrameLengthMs);
            });
        initialized_ = true;
        return 0;
    }

    int32_t DummyAudioDevice::Terminate()
    {
        if (!initialized_)
            return 0;

        StopRecording();
        StopPlayout();

        initialized_ = false;
        return 0;
    }

    void DummyAudioDevice::ProcessAudio()
    {
        std::lock_guard<std::mutex> lock(mutex_);

        if (playing_)
        {
            int64_t elapsed_time_ms = -1;
            int64_t ntp_time_ms = -1;

            const int kBytesPerSample = 2;
            const int kChannels = 2;
            const int kSamplingRate = 48000;
            const int kSamplesPerFrame = kSamplingRate * kFrameLengthMs / 1000;

            char data[kBytesPerSample * kChannels * kSamplesPerFrame];

            audio_transport_->PullRenderData(kBytesPerSample * 8, kSamplingRate,
                kChannels, kSamplesPerFrame, data,
                &elapsed_time_ms, &ntp_time_ms);
        }

        if (recording_)
        {
            for (const auto& pair : callbacks_) {
                pair.second();
            }
        }
    }

    void DummyAudioDevice::RegisterSendAudioCallback(
        UnityAudioTrackSource* source, int sampleRate, int channels) {
        std::lock_guard<std::mutex> lock(mutex_);
        if (callbacks_.find(source) == callbacks_.end()) {
            callbacks_.emplace(source, [source, sampleRate, channels]() {
                source->SendAudioData(sampleRate, channels); });
        }
    }

    void DummyAudioDevice::UnregisterSendAudioCallback(
        UnityAudioTrackSource* source) {
        std::lock_guard<std::mutex> lock(mutex_);
        if (callbacks_.find(source) != callbacks_.end()) {
            callbacks_.erase(source);
        }
    }

} // end namespace webrtc
} // end namespace unity
