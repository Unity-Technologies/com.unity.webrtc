#pragma once

#include <mutex>
#include "WebRTCPlugin.h"
#include "common_audio/resampler/include/push_resampler.h"

namespace unity
{
namespace webrtc
{
    using namespace ::webrtc;

    class AudioTrackSinkAdapter
        : public webrtc::AudioTrackSinkInterface
    {
    public:
        AudioTrackSinkAdapter(
            DelegateAudioReceive callback,
            size_t sampleRate,
            size_t channels);
        ~AudioTrackSinkAdapter() override {};

        void OnData(
            const void* audio_data,
            int bits_per_sample,
            int sample_rate,
            size_t number_of_channels,
            size_t number_of_frames) override;

        std::unique_ptr<AudioFrame>  Resample(
            const void* audio_data,
            const size_t number_of_frames,
            const size_t channels,
            const size_t dst_channels,
            const uint32_t sample_rate,
            const uint32_t dst_sample_rate);
    private:
        DelegateAudioReceive _callback;
        PushResampler<int16_t> _resampler;
        size_t _sampleRate;
        size_t _channels;

    };
} // end namespace webrtc
} // end namespace unity
