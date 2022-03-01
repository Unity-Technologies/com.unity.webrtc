#pragma once

#include <mutex>

namespace unity
{
namespace webrtc
{
    using namespace ::webrtc;

    class UnityAudioTrackSource : public LocalAudioSource
    {
    public:
        static rtc::scoped_refptr<UnityAudioTrackSource> Create();
        static rtc::scoped_refptr<UnityAudioTrackSource> Create(const cricket::AudioOptions& audio_options);

        const cricket::AudioOptions options() const override { return _options; }
        void AddSink(AudioTrackSinkInterface* sink) override;
        void RemoveSink(AudioTrackSinkInterface* sink) override;

        void PushAudioData(
            const float* pAudioData, int nSampleRate,
            size_t nNumChannels, size_t nNumFrames);
        void SendAudioData(int nSampleRate, size_t nNumChannels);

    protected:
        UnityAudioTrackSource();
        UnityAudioTrackSource(const cricket::AudioOptions& audio_options);

        ~UnityAudioTrackSource() override;

    private:
        std::vector<int16_t> _convertedAudioData;
        std::vector<AudioTrackSinkInterface*> _arrSink;
        std::mutex _mutex;
        cricket::AudioOptions _options;
        bool _bufferInit = false;
    };
} // end namespace webrtc
} // end namespace unity
