#pragma once

namespace unity
{
namespace webrtc
{
    class UnityAudioTrackSource : public ::webrtc::LocalAudioSource
    {
    public:
        static rtc::scoped_refptr<UnityAudioTrackSource> Create(const std::string& sTrackName);
        static rtc::scoped_refptr<UnityAudioTrackSource> Create(const std::string& sTrackName, const cricket::AudioOptions& audio_options);

        void AddSink(::webrtc::AudioTrackSinkInterface* sink) override;
        void RemoveSink(::webrtc::AudioTrackSinkInterface* sink) override;

        void OnData(const float* pAudioData, int nSampleRate, size_t nNumChannels, size_t nNumFrames);

    protected:
        UnityAudioTrackSource(const std::string& sTrackName);
        UnityAudioTrackSource(const std::string& sTrackName, const cricket::AudioOptions& audio_options);

        ~UnityAudioTrackSource() override;

    private:
        std::string m_sTrackName;
        std::vector<int16_t> convertedAudioData;
        ::webrtc::AudioTrackSinkInterface* m_pAudioTrackSinkInterface;
    };
} // end namespace webrtc
} // end namespace unity
