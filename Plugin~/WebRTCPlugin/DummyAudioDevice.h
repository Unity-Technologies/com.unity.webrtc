#pragma once

#include <mutex>

#include "WebRTCPlugin.h"
#include "api/task_queue/default_task_queue_factory.h"

namespace unity
{
namespace webrtc
{
    using namespace ::webrtc;

    const int kRecordingFixedSampleRate = 44100;
    const size_t kRecordingNumChannels = 2;

    class DummyAudioDevice : public webrtc::AudioDeviceModule
    {
    public:
        void ProcessAudioData(const float* data, int32 size, const size_t nChannels);
        void PullAudioData(const size_t nBitsPerSamples,
            const size_t nSampleRate,
            const size_t nChannels,
            const uint32_t samplesPerSec,
            void* audioSamples,
            int64_t* elapsed_time_ms,
            int64_t* ntp_time_ms)
        {
            // Get 10ms audio and copy result to temporary byte buffer.
            //render_buffer_.resize(audio_bus->frames() * audio_bus->channels());
            //constexpr int kBytesPerSample = 2;
            //static_assert(sizeof(render_buffer_[0]) == kBytesPerSample,
            //    "kBytesPerSample and FromInterleaved expect 2 bytes.");
            //int64_t elapsed_time_ms = -1;
            //int64_t ntp_time_ms = -1;
            //int16_t* audio_data = render_buffer_.data();

            audio_transport_->PullRenderData(
                nBitsPerSamples,
                nSampleRate,
                nChannels,
                samplesPerSec,
                audioSamples,
//                nSamplesOut,
                elapsed_time_ms,
                ntp_time_ms);
        }

        bool RecThreadProcess();

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
            deviceBuffer->RegisterAudioCallback(transport);

            audio_transport_ = transport;
            return 0;
        }

        // Main initialization and termination
        virtual int32 Init() override
        {
            deviceBuffer = std::make_unique<webrtc::AudioDeviceBuffer>(webrtc::CreateDefaultTaskQueueFactory().get());
            started = true;
            return 0;
        }
        virtual int32 Terminate() override
        {
            deviceBuffer.reset();
            started = false;
            isRecording = false;
            return 0;
        }
        virtual bool Initialized() const override
        {
            return started;
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
            RTC_LOG(LS_INFO) << "InitRecording";

            isRecording = true;
            _recordingFramesIn10MS = static_cast<size_t>(kRecordingFixedSampleRate / 100);
            deviceBuffer->SetRecordingSampleRate(kRecordingFixedSampleRate);
            deviceBuffer->SetRecordingChannels(kRecordingNumChannels);


            //RTC_LOG(LS_INFO) << "StartRecording";
            _ptrThreadRec = rtc::Thread::Create();
            _ptrThreadRec->Start();
            _ptrThreadRec->PostTask(RTC_FROM_HERE, [&] {
                while (RecThreadProcess()) {
                }
            });

            return 0;
        }
        virtual bool RecordingIsInitialized() const override
        {
            return isRecording;
        }

        // Audio transport control
        virtual int32 StartPlayout() override
        {
            return 0;
        }
        virtual int32 StopPlayout() override
        {
            return 0;
        }
        virtual bool Playing() const override
        {
            return false;
        }

        virtual int32 StartRecording() override
        {
            //RTC_LOG(LS_INFO) << "StartRecording";
            //_ptrThreadRec = rtc::Thread::Create();
            //_ptrThreadRec->Invoke<void>(RTC_FROM_HERE, [&]{
            //    while (RecThreadProcess()) {
            //    }
            //});

            //_ptrThreadRec = new rtc::PlatformThread((rtc::ThreadRunFunction)ThreadRun,
            //    nullptr,
            //    "webrtc_audio_module_capture_thread",
            //        rtc::ThreadPriority::kRealtimePriority);

            return 0;
        }
        virtual int32 StopRecording() override
        {
            if (_ptrThreadRec && !_ptrThreadRec->empty())
                _ptrThreadRec->Stop();
            return 0;
        }
        virtual bool Recording() const override
        {
            return isRecording;
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
    private:
        std::unique_ptr<webrtc::AudioDeviceBuffer> deviceBuffer;
        std::atomic<bool> started {false};
        std::atomic<bool> isRecording {false};
        std::vector<int16> convertedAudioData;

        size_t _recordingFramesIn10MS;
        std::unique_ptr<rtc::Thread> _ptrThreadRec;
        int64_t _lastCallRecordMillis = 0;

        webrtc::AudioTransport* audio_transport_ = nullptr;
    };

} // end namespace webrtc
} // end namespace unity
