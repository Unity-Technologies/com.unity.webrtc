#pragma once

#include <mutex>
#include <unordered_map>

#include "WebRTCPlugin.h"
#include "rtc_base/platform_thread.h"
#include "rtc_base/task_utils/repeating_task.h"

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
        ~DummyAudioDevice() override {
            StopPlayout();
            StopRecording();
        }

        //webrtc::AudioDeviceModule
        // Retrieve the currently utilized audio layer
        int32_t ActiveAudioLayer(AudioLayer* audioLayer) const override
        {
            *audioLayer = AudioDeviceModule::kPlatformDefaultAudio;
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
        virtual int16_t PlayoutDevices() override
        {
            return 0;
        }
        virtual int16_t RecordingDevices() override
        {
            return 0;
        }
        virtual int32_t PlayoutDeviceName(uint16_t index,
            char name[webrtc::kAdmMaxDeviceNameSize],
            char guid[webrtc::kAdmMaxGuidSize]) override
        {
            return 0;
        }
        virtual int32 RecordingDeviceName(uint16 index,
            char name[webrtc::kAdmMaxDeviceNameSize],
            char guid[webrtc::kAdmMaxGuidSize]) override
        {
            return 0;
        }

        // Device selection
        virtual int32 SetPlayoutDevice(uint16 index) override
        {
            return 0;
        }
        virtual int32 SetPlayoutDevice(WindowsDeviceType device) override
        {
            return 0;
        }
        virtual int32 SetRecordingDevice(uint16 index) override
        {
            return 0;
        }
        virtual int32 SetRecordingDevice(WindowsDeviceType device) override
        {
            return 0;
        }

        // Audio transport initialization
        virtual int32 PlayoutIsAvailable(bool* available) override
        {
            return 0;
        }
        virtual int32 InitPlayout() override
        {
            return 0;
        }
        virtual bool PlayoutIsInitialized() const override
        {
            return false;
        }
        virtual int32 RecordingIsAvailable(bool* available) override
        {
            return 0;
        }
        virtual int32 InitRecording() override
        {
            return 0;
        }
        virtual bool RecordingIsInitialized() const override
        {
            return false;
        }

        virtual int32 StartPlayout() override
        {
            std::lock_guard<std::mutex> lock(mutex_);
            playing_ = true;
            return 0;
        }
        virtual int32 StopPlayout() override
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

        virtual int32 StartRecording() override
        {
            std::lock_guard<std::mutex> lock(mutex_);
            recording_ = true;
            return 0;
        }

        virtual int32 StopRecording() override
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
        virtual int32 InitSpeaker() override
        {
            return 0;
        }
        virtual bool SpeakerIsInitialized() const override
        {
            return false;
        }
        virtual int32 InitMicrophone() override
        {
            return 0;
        }
        virtual bool MicrophoneIsInitialized() const override
        {
            return false;
        }

        // Speaker volume controls
        virtual int32 SpeakerVolumeIsAvailable(bool* available) override
        {
            return 0;
        }
        virtual int32 SetSpeakerVolume(uint32 volume) override
        {
            return 0;
        }
        virtual int32 SpeakerVolume(uint32* volume) const override
        {
            return 0;
        }
        virtual int32 MaxSpeakerVolume(uint32* maxVolume) const override
        {
            return 0;
        }
        virtual int32 MinSpeakerVolume(uint32* minVolume) const override
        {
            return 0;
        }

        // Microphone volume controls
        virtual int32 MicrophoneVolumeIsAvailable(bool* available) override
        {
            return 0;
        }
        virtual int32 SetMicrophoneVolume(uint32 volume) override
        {
            return 0;
        }
        virtual int32 MicrophoneVolume(uint32* volume) const override
        {
            return 0;
        }
        virtual int32 MaxMicrophoneVolume(uint32* maxVolume) const override
        {
            return 0;
        }
        virtual int32 MinMicrophoneVolume(uint32* minVolume) const override
        {
            return 0;
        }

        // Speaker mute control
        virtual int32 SpeakerMuteIsAvailable(bool* available) override
        {
            return 0;
        }
        virtual int32 SetSpeakerMute(bool enable) override
        {
            return 0;
        }
        virtual int32 SpeakerMute(bool* enabled) const override
        {
            return 0;
        }

        // Microphone mute control
        virtual int32 MicrophoneMuteIsAvailable(bool* available) override
        {
            return 0;
        }
        virtual int32 SetMicrophoneMute(bool enable) override
        {
            return 0;
        }
        virtual int32 MicrophoneMute(bool* enabled) const override
        {
            return 0;
        }

        // Stereo support
        virtual int32 StereoPlayoutIsAvailable(bool* available) const override
        {
            return 0;
        }
        virtual int32 SetStereoPlayout(bool enable) override
        {
            return 0;
        }
        virtual int32 StereoPlayout(bool* enabled) const override
        {
            return 0;
        }
        virtual int32 StereoRecordingIsAvailable(bool* available) const override
        {
            *available = true;
            return 0;
        }
        virtual int32 SetStereoRecording(bool enable) override
        {
            return 0;
        }
        virtual int32 StereoRecording(bool* enabled) const override
        {
            *enabled = true;
            return 0;
        }

        // Playout delay
        virtual int32 PlayoutDelay(uint16* delayMS) const override
        {
            return 0;
        }

        // Only supported on Android.
        virtual bool BuiltInAECIsAvailable() const override
        {
            return false;
        }
        virtual bool BuiltInAGCIsAvailable() const override
        {
            return false;
        }
        virtual bool BuiltInNSIsAvailable() const override
        {
            return false;
        }

        // Enables the built-in audio effects. Only supported on Android.
        virtual int32 EnableBuiltInAEC(bool enable) override
        {
            return 0;
        }
        virtual int32 EnableBuiltInAGC(bool enable) override
        {
            return 0;
        }
        virtual int32 EnableBuiltInNS(bool enable) override
        {
            return 0;
        }
#if defined(WEBRTC_IOS)
        virtual int GetPlayoutAudioParameters(webrtc::AudioParameters* params) const override
        {
            return 0;
        }
        virtual int GetRecordAudioParameters(webrtc::AudioParameters* params) const override
        {
            return 0;
        }
#endif
        void RegisterSendAudioCallback(
            UnityAudioTrackSource* source, int sampleRate, int channels);
        void UnregisterSendAudioCallback(
            UnityAudioTrackSource* source);

    private:
        void ProcessAudio();
        bool PlayoutThreadProcess();

        const int32_t kFrameLengthMs = 10;

        std::unique_ptr<rtc::TaskQueue> taskQueue_;
        RepeatingTaskHandle task_;
        std::atomic<bool> initialized_ {false};
        std::atomic<bool> playing_ {false};
        std::atomic<bool> recording_{ false };
        mutable std::mutex mutex_;
        webrtc::AudioTransport* audio_transport_{ nullptr };
        using callback_t = std::function<void()>;
        std::unordered_map<UnityAudioTrackSource*, callback_t> callbacks_;
        TaskQueueFactory* tackQueueFactory_;
    };

} // end namespace webrtc
} // end namespace unity
