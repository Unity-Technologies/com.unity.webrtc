#pragma once

#include <mutex>

#include "WebRTCPlugin.h"
#include "api/task_queue/default_task_queue_factory.h"
#include "rtc_base/platform_thread.h"

namespace unity
{
namespace webrtc
{
    using namespace ::webrtc;

    class DummyAudioDevice : public webrtc::AudioDeviceModule
    {
    public:
        //webrtc::AudioDeviceModule
        // Retrieve the currently utilized audio layer
        virtual int32 ActiveAudioLayer(AudioLayer* audioLayer) const override
        {
            *audioLayer = AudioDeviceModule::kPlatformDefaultAudio;
            return 0;
        }
        // Full-duplex transportation of PCM audio
        virtual int32 RegisterAudioCallback(webrtc::AudioTransport* transport) override
        {
            audio_transport_ = transport;
            return 0;
        }

        // Main initialization and termination
        virtual int32 Init() override
        {
            initialized_ = true;
            return 0;
        }
        virtual int32 Terminate() override
        {
            initialized_ = false;
            playing_ = false;
            recording_ = false;
            return 0;
        }
        virtual bool Initialized() const override
        {
            return initialized_;
        }

        // Device enumeration
        virtual int16 PlayoutDevices() override
        {
            return 0;
        }
        virtual int16 RecordingDevices() override
        {
            return 0;
        }
        virtual int32 PlayoutDeviceName(uint16 index,
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

        // Audio transport control
        virtual int32 StartPlayout() override
        {
            playing_ = true;
            playoutAudioThread_ = rtc::Thread::Create();
            playoutAudioThread_->Start();
            playoutAudioThread_->PostTask(RTC_FROM_HERE, [&] {
                while (PlayoutThreadProcess()) {}
                });
            return 0;
        }
        virtual int32 StopPlayout() override
        {
            if (playoutAudioThread_ && !playoutAudioThread_->empty())
                playoutAudioThread_->Stop();
            return 0;
        }
        virtual bool Playing() const override
        {
            return playing_;
        }

        virtual int32 StartRecording() override
        {
            recording_ = true;
            recordAudioThread_.reset(new rtc::PlatformThread(
                [](void *pThis) { static_cast<DummyAudioDevice*>(pThis)->RecodingThread(); },
                this, "webrtc_audio_module_recording_thread", rtc::kRealtimePriority));
            recordAudioThread_->Start();
            return 0;
        }

        virtual int32 StopRecording() override
        {
            if (recording_) {
                recording_ = false;
                recordAudioThread_->Stop();
            }
            return 0;
        }
        virtual bool Recording() const override
        {
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
        void InitLocalAudio(int sampleRate, int channels);
        void PushLocalAudio(const float* audioData, int sampleRate, int channels, int numFrames);

    private:
        bool PlayoutThreadProcess();
        void RecodingThread();

        std::atomic<bool> initialized_ {false};
        std::atomic<bool> playing_ {false};
        std::atomic<bool> recording_{ false };
        std::mutex mutex_;
        std::unique_ptr<rtc::Thread> playoutAudioThread_;
        std::unique_ptr<rtc::PlatformThread> recordAudioThread_;
        int64_t lastCallRecordMillis_ = 0;

        std::atomic<webrtc::AudioTransport*> audio_transport_{ nullptr };

        int sampleRate_ = 0;
        int channels_ = 0;
        std::vector<int16_t> audioBuffer_;
    };

} // end namespace webrtc
} // end namespace unity
