#pragma once

#include "WebRTCPlugin.h"
#include "api/task_queue/default_task_queue_factory.h"

namespace unity
{
namespace webrtc
{

    using namespace ::webrtc;

    class AudioTrackSinkAdapter : public webrtc::AudioTrackSinkInterface
    {
    public:
        AudioTrackSinkAdapter(AudioTrackInterface* track, DelegateAudioReceive callback)
            : _track(track), _callback(callback)
        {
        }
        ~AudioTrackSinkAdapter() override
        {
        }

        void OnData(const void* audio_data, int bits_per_sample, int sample_rate, size_t number_of_channels, size_t number_of_frames) override
        {
            if(_callback != nullptr)
            {
                size_t size = number_of_frames * number_of_channels;
                //RTC_LOG(LS_INFO) << "size:" << size << "number_of_frames:" << number_of_frames;

                _callback(
                    _track,
                    audio_data,
                    size,
                    bits_per_sample,
                    sample_rate,
                    number_of_channels,
                    number_of_frames);
            }
        }
    private:
        AudioTrackInterface* _track;
        DelegateAudioReceive _callback;
    };

    class UnityAudioTrackSource : public webrtc::LocalAudioSource, public webrtc::AudioTrackSinkInterface
    {
    public:
        static rtc::scoped_refptr<UnityAudioTrackSource> Create(const std::string& sTrackName)
        {
            rtc::scoped_refptr<UnityAudioTrackSource> source(
                new rtc::RefCountedObject<UnityAudioTrackSource>(sTrackName));
            return source;
        }

        static rtc::scoped_refptr<UnityAudioTrackSource> Create(const std::string& sTrackName, const cricket::AudioOptions& audio_options)
        {
            rtc::scoped_refptr<UnityAudioTrackSource> source(
                new rtc::RefCountedObject<UnityAudioTrackSource>(sTrackName, audio_options));
            return source;
        }

        //void AddSink(webrtc::AudioTrackSinkInterface* sink) override
        //{
        //    m_pAudioTrackSinkInterface = sink;
        //}
        //void RemoveSink(webrtc::AudioTrackSinkInterface* sink) override
        //{
        //    m_pAudioTrackSinkInterface = 0;
        //}

        void OnData(const void* pAudioData, int nBitPerSample, int nSampleRate, size_t nNumChannels, size_t nNumFrames) override
        {
            RTC_LOG(LS_INFO) << "bitPerSample:" << nBitPerSample << "\n"
                << "sampleRate" << nSampleRate << "\n"
                << "channels" << nNumChannels << "\n"
                << "numFrames" << nNumFrames;

            //if (m_pAudioTrackSinkInterface)
            //{

            //    //todo(kazuki):: buffering audio
            //    size_t nNumFrames_ = nSampleRate / 100;

            //    m_pAudioTrackSinkInterface->OnData(pAudioData, nBitPerSample, nSampleRate, nNumChannels, nNumFrames_);
            //}
        }
    protected:
        UnityAudioTrackSource(const std::string& sTrackName) : m_sTrackName(sTrackName), m_pAudioTrackSinkInterface(0)
        {
            RTC_LOG(LS_INFO) << "UnityAudioTrackSource:Constructor";
            AddSink(this);
            //CopyConstraintsIntoAudioOptions(constraints, &m_Options);
        }
        UnityAudioTrackSource(const std::string& sTrackName, const cricket::AudioOptions& audio_options)
        : m_sTrackName(sTrackName)
        , m_pAudioTrackSinkInterface(0)
        {
            RTC_LOG(LS_INFO) << "UnityAudioTrackSource:Constructor";
            AddSink(this);
        }
        ~UnityAudioTrackSource() override
        {
            RemoveSink(this);
        }

    private:
        std::string m_sTrackName;
        std::unique_ptr<AudioTrackSinkAdapter> _sink;
        //cricket::AudioOptions m_Options;
        webrtc::AudioTrackSinkInterface* m_pAudioTrackSinkInterface;
    };

    //void WebRTCConductor::AddStreams()
    //{
    //    if (m_ActiveStream.find(g_StreamLabel) != m_ActiveStream.end())
    //    {
    //        return;  // Already added.
    //    }

    //    // Create stream
    //    rtc::scoped_refptr<webrtc::MediaStreamInterface> stream =
    //        m_pPeerConnectionFactory->CreateLocalMediaStream(g_StreamLabel);

    //    // Create Audio Track 1 
    //    m_ActiveAudioSources[g_AudioLabel1] = MyLocalAudioSource::Create(g_AudioLabel1, m_WebRTCHandler, NULL);
    //    rtc::scoped_refptr<webrtc::AudioTrackInterface> audio_track1(
    //        m_pPeerConnectionFactory->CreateAudioTrack(g_AudioLabel1, m_ActiveAudioSources[g_AudioLabel1]));
    //    // Add audio track to stream
    //    stream->AddTrack(audio_track1);

    //    // Create Audio Track 2
    //    m_ActiveAudioSources[g_AudioLabel2] = MyLocalAudioSource::Create(g_AudioLabel2, m_WebRTCHandler, NULL);
    //    rtc::scoped_refptr<webrtc::AudioTrackInterface> audio_track2(
    //        m_pPeerConnectionFactory->CreateAudioTrack(g_AudioLabel2, m_ActiveAudioSources[g_AudioLabel2]));
    //    // Add audio track to stream
    //    stream->AddTrack(audio_track2);

    //    // TODO - switch to using AddTrack API on m_pPeerConnection - since AddStream will eventually be deprecated

    //    if (!m_pPeerConnection->AddStream(stream))
    //    {
    //        ss << __FUNCTION__ << ": Adding stream to PeerConnection failed";
    //        m_WebRTCHandler.onLog(IWebRTCHandler::Error, ss.str().c_str());
    //    }
    //    typedef std::pair<std::string, rtc::scoped_refptr<webrtc::MediaStreamInterface> > MediaStreamPair;
    //    m_ActiveStream.insert(MediaStreamPair(stream->label(), stream));
    //}

    //void WebRTCConductor::SendAudioTrackToWebRTCClient(int nTrackNum, short* pLinearAudio, int nSamples, int nSamplingFreqHz, int nChannels)
    //{
    //    auto it = m_ActiveAudioSources.find(nTrackNum == 2 ? g_AudioLabel2 : g_AudioLabel1);
    //    if (it != m_ActiveAudioSources.end())
    //    {
    //        int NumberSamplesFor10ms = nSamplingFreqHz / 100; // eg.  80 for 8KHz and 160 for 16kHz
    //        assert(nSamples % NumberSamplesFor10ms == 0);

    //        for (int i = 0; i < nSamples / NumberSamplesFor10ms; i++)
    //        {
    //            it->second->OnData(&pLinearAudio[i * NumberSamplesFor10ms * nChannels],
    //                sizeof(pLinearAudio[0]) * 8 * nChannels,  // BitsPerSample
    //                nSamplingFreqHz,  // SampleRate
    //                nChannels,
    //                NumberSamplesFor10ms);   // NumFrames
    //        }
    //    }
    //}



    class DummyAudioDevice : public webrtc::AudioDeviceModule
    {
    public:
        void ProcessAudioData(const float* data, int32 size);
        void PullAudioData(const size_t nBitsPerSamples,
            const size_t nSampleRate,
            const size_t nChannels,
            const uint32_t samplesPerSec,
            void* audioSamples,
//            size_t& nSamplesOut,
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
            RTC_LOG(LS_INFO) << "RegisterAudioCallback";
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
            isRecording = true;
            deviceBuffer->SetRecordingSampleRate(48000);
            deviceBuffer->SetRecordingChannels(2);
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
            return 0;
        }
        virtual int32 StopRecording() override
        {
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

        webrtc::AudioTransport* audio_transport_ = nullptr;
    };

} // end namespace webrtc
} // end namespace unity
