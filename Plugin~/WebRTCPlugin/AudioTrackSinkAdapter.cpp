#include "pch.h"
#include "AudioTrackSinkAdapter.h"

namespace unity
{
namespace webrtc
{
    AudioTrackSinkAdapter::AudioTrackSinkAdapter(
        webrtc::AudioTrackInterface* track, DelegateAudioReceive callback)
        : _track(track)
        , _callback(callback)
    {
    }

    void AudioTrackSinkAdapter::OnData(
        const void* audio_data, int bits_per_sample, int sample_rate,
        size_t number_of_channels, size_t number_of_frames)
    {
        if (_callback == nullptr) {
            return;
        }

        switch (sample_rate) {
        case 44100:
            break;
        case 48000:
            break;
        default:
            RTC_LOG(LS_WARNING) << "Unsupported sampling rate: " << sample_rate;
            return;
        }

        if (bits_per_sample != 16) {
            RTC_LOG(LS_WARNING) << "Unsupported bits/sample: " << bits_per_sample;
            return;
        }

        const size_t size = number_of_channels * number_of_frames;
        const int16_t* data = static_cast<const int16_t*>(audio_data);
        const float_t INVERSE = 1.0 / SHRT_MAX;

        std::vector<float_t> _converted_data(size);

        for (size_t i = 0; i < size; i++)
        {
            _converted_data[i] = data[i] * INVERSE;
        }

        _callback(
            _track,
            _converted_data.data(),
            size,
            sample_rate,
            number_of_channels,
            number_of_frames);
    }
} // end namespace webrtc
} // end namespace unity
