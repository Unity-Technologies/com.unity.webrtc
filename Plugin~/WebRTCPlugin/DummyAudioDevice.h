#pragma once

#include <mutex>
#include <unordered_map>

#include <modules/audio_device/include/audio_device.h>
#include <rtc_base/platform_thread.h>
#include <rtc_base/task_queue.h>
#include <rtc_base/task_utils/repeating_task.h>

#include "WebRTCPlugin.h"

namespace unity
{
namespace webrtc
{
    using namespace ::webrtc;

    class UnityAudioTrackSource;

    class DummyAudioDevice : public webrtc::AudioDeviceModule
    {
    public:
        DummyAudioDevice(TaskQueueFactory* factory);
        ~DummyAudioDevice() override { Terminate(); }

        // webrtc::AudioDeviceModule
        // Retrieve the currently utilized audio layer
        int32_t ActiveAudioLayer(AudioLayer* audioLayer) const override
        {
            *audioLayer = AudioDeviceModule::kDummyAudio;
            return 0;
        }
        // Full-duplex transportation of PCM audio
        int32_t RegisterAudioCallback(webrtc::AudioTransport* transport) override
        {
            std::lock_guard<std::mutex> lock(mutex_);
            RTC_DCHECK_EQ(!audio_transport_, !!transport);
            audio_transport_ = transport;
            return 0;
        }

        // Main initialization and termination
        int32_t Init() override;
        int32_t Terminate() override;
        virtual bool Initialized() const override
        {
            std::lock_guard<std::mutex> lock(mutex_);
            return initialized_;
        }

        // Device enumeration
        virtual int16_t PlayoutDevices() override { return 0; }
        virtual int16_t RecordingDevices() override { return 0; }
        virtual int32_t PlayoutDeviceName(
            uint16_t index, char name[webrtc::kAdmMaxDeviceNameSize], char guid[webrtc::kAdmMaxGuidSize]) override
        {
            return 0;
        }
        virtual int32_t RecordingDeviceName(
            uint16_t index, char name[webrtc::kAdmMaxDeviceNameSize], char guid[webrtc::kAdmMaxGuidSize]) override
        {
            return 0;
        }

        // Device selection
        virtual int32_t SetPlayoutDevice(uint16_t index) override { return 0; }
        virtual int32_t SetPlayoutDevice(WindowsDeviceType device) override { return 0; }
        virtual int32_t SetRecordingDevice(uint16_t index) override { return 0; }
        virtual int32_t SetRecordingDevice(WindowsDeviceType device) override { return 0; }

        // Audio transport initialization
        virtual int32_t PlayoutIsAvailable(bool* available) override { return 0; }
        virtual int32_t InitPlayout() override { return 0; }
        virtual bool PlayoutIsInitialized() const override { return false; }
        virtual int32_t RecordingIsAvailable(bool* available) override { return 0; }
        virtual int32_t InitRecording() override { return 0; }
        virtual bool RecordingIsInitialized() const override { return false; }

        virtual int32_t StartPlayout() override
        {
            std::lock_guard<std::mutex> lock(mutex_);
            playing_ = true;
            return 0;
        }
        virtual int32_t StopPlayout() override
        {
            std::lock_guard<std::mutex> lock(mutex_);
            playing_ = false;
            return 0;
        }
        virtual bool Playing() const override
        {
            std::lock_guard<std::mutex> lock(mutex_);
            return playing_;
        }

        virtual int32_t StartRecording() override
        {
            std::lock_guard<std::mutex> lock(mutex_);
            recording_ = true;
            return 0;
        }

        virtual int32_t StopRecording() override
        {
            std::lock_guard<std::mutex> lock(mutex_);
            recording_ = false;
            return 0;
        }
        virtual bool Recording() const override
        {
            std::lock_guard<std::mutex> lock(mutex_);
            return recording_;
        }

        // Audio mixer initialization
        virtual int32_t InitSpeaker() override { return 0; }
        virtual bool SpeakerIsInitialized() const override { return false; }
        virtual int32_t InitMicrophone() override { return 0; }
        virtual bool MicrophoneIsInitialized() const override { return false; }

        // Speaker volume controls
        virtual int32_t SpeakerVolumeIsAvailable(bool* available) override { return 0; }
        virtual int32_t SetSpeakerVolume(uint32_t volume) override { return 0; }
        virtual int32_t SpeakerVolume(uint32_t* volume) const override { return 0; }
        virtual int32_t MaxSpeakerVolume(uint32_t* maxVolume) const override { return 0; }
        virtual int32_t MinSpeakerVolume(uint32_t* minVolume) const override { return 0; }

        // Microphone volume controls
        virtual int32_t MicrophoneVolumeIsAvailable(bool* available) override { return 0; }
        virtual int32_t SetMicrophoneVolume(uint32_t volume) override { return 0; }
        virtual int32_t MicrophoneVolume(uint32_t* volume) const override { return 0; }
        virtual int32_t MaxMicrophoneVolume(uint32_t* maxVolume) const override { return 0; }
        virtual int32_t MinMicrophoneVolume(uint32_t* minVolume) const override { return 0; }

        // Speaker mute control
        virtual int32_t SpeakerMuteIsAvailable(bool* available) override { return 0; }
        virtual int32_t SetSpeakerMute(bool enable) override { return 0; }
        virtual int32_t SpeakerMute(bool* enabled) const override { return 0; }

        // Microphone mute control
        virtual int32_t MicrophoneMuteIsAvailable(bool* available) override { return 0; }
        virtual int32_t SetMicrophoneMute(bool enable) override { return 0; }
        virtual int32_t MicrophoneMute(bool* enabled) const override { return 0; }

        // Stereo support
        virtual int32_t StereoPlayoutIsAvailable(bool* available) const override { return 0; }
        virtual int32_t SetStereoPlayout(bool enable) override { return 0; }
        virtual int32_t StereoPlayout(bool* enabled) const override { return 0; }
        virtual int32_t StereoRecordingIsAvailable(bool* available) const override
        {
            *available = true;
            return 0;
        }
        virtual int32_t SetStereoRecording(bool enable) override { return 0; }
        virtual int32_t StereoRecording(bool* enabled) const override
        {
            *enabled = true;
            return 0;
        }

        // Playout delay
        virtual int32_t PlayoutDelay(uint16_t* delayMS) const override { return 0; }

        // Only supported on Android.
        virtual bool BuiltInAECIsAvailable() const override { return false; }
        virtual bool BuiltInAGCIsAvailable() const override { return false; }
        virtual bool BuiltInNSIsAvailable() const override { return false; }

        // Enables the built-in audio effects. Only supported on Android.
        virtual int32_t EnableBuiltInAEC(bool enable) override { return 0; }
        virtual int32_t EnableBuiltInAGC(bool enable) override { return 0; }
        virtual int32_t EnableBuiltInNS(bool enable) override { return 0; }
#if defined(WEBRTC_IOS)
        virtual int GetPlayoutAudioParameters(webrtc::AudioParameters* params) const override { return 0; }
        virtual int GetRecordAudioParameters(webrtc::AudioParameters* params) const override { return 0; }
#endif

    private:
        void ProcessAudio();
        bool PlayoutThreadProcess();

        const int32_t kFrameLengthMs = 10;
        const int32_t kBytesPerSample = 2;
        const size_t kChannels = 2;
        const int32_t kSamplingRate = 48000;
        const size_t kSamplesPerFrame = static_cast<size_t>(kSamplingRate * kFrameLengthMs / 1000);
        std::vector<int16_t> audio_data;
        std::unique_ptr<rtc::TaskQueue> taskQueue_;
        RepeatingTaskHandle task_;
        std::atomic<bool> initialized_ { false };
        std::atomic<bool> playing_ { false };
        std::atomic<bool> recording_ { false };
        mutable std::mutex mutex_;
        webrtc::AudioTransport* audio_transport_ { nullptr };
        TaskQueueFactory* tackQueueFactory_;
    };

} // end namespace webrtc
} // end namespace unity
