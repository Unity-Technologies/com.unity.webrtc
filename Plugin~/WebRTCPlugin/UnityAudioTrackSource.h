#pragma once

using namespace ::webrtc;

namespace unity
{
namespace webrtc
{
    class UnityAudioTrackSource : public LocalAudioSource
    {
    public:
        static rtc::scoped_refptr<UnityAudioTrackSource> Create(const std::string& sTrackName);
        static rtc::scoped_refptr<UnityAudioTrackSource> Create(const std::string& sTrackName, const cricket::AudioOptions& audio_options);

        void AddSink(AudioTrackSinkInterface* sink) override;
        void RemoveSink(AudioTrackSinkInterface* sink) override;

        void OnData(const float* pAudioData, int nSampleRate, size_t nNumChannels, size_t nNumFrames);

    protected:
        UnityAudioTrackSource(const std::string& sTrackName);
        UnityAudioTrackSource(const std::string& sTrackName, const cricket::AudioOptions& audio_options);

        ~UnityAudioTrackSource() override;

    private:
        std::string _sTrackName;
        std::vector<int16_t> _convertedAudioData;
        std::vector <AudioTrackSinkInterface*> _arrSink;
    };
} // end namespace webrtc
} // end namespace unity
