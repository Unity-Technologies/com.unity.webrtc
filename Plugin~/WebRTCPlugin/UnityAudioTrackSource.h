#pragma once

#include <mutex>

using namespace ::webrtc;

namespace unity
{
namespace webrtc
{
    class UnityAudioTrackSource : public LocalAudioSource
    {
    public:
        static rtc::scoped_refptr<UnityAudioTrackSource> Create();
        static rtc::scoped_refptr<UnityAudioTrackSource> Create(const cricket::AudioOptions& audio_options);

        const cricket::AudioOptions options() const override { return _options; }
        void AddSink(AudioTrackSinkInterface* sink) override;
        void RemoveSink(AudioTrackSinkInterface* sink) override;

    protected:
        UnityAudioTrackSource();
        UnityAudioTrackSource(const cricket::AudioOptions& audio_options);

        ~UnityAudioTrackSource() override;

    private:
        std::vector<int16_t> _convertedAudioData;
        std::vector <AudioTrackSinkInterface*> _arrSink;
        std::mutex _mutex;
        cricket::AudioOptions _options;
    };
} // end namespace webrtc
} // end namespace unity
