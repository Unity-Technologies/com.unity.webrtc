#include "pch.h"
#include "AudioTrackSinkAdapter.h"
#include "common_audio/include/audio_util.h"
#include "audio/remix_resample.h"

namespace unity
{
namespace webrtc
{
    AudioTrackSinkAdapter::AudioTrackSinkAdapter(
        DelegateAudioReceive callback,
        size_t sampleRate,
        size_t channels)
        : _callback(callback)
        , _sampleRate(sampleRate)
        , _channels(channels)
    {
    }

    void AudioTrackSinkAdapter::OnData(
        const void* audio_data, int bits_per_sample, int sample_rate,
        size_t number_of_channels, size_t number_of_frames)
    {
        if (_callback == nullptr) {
            return;
        }

        // note: AudioTrackSinkInterface::OnData method is passed audio data from
        // audio decoder directly, so we need to resample for expected format.
        // For example, when we use encoder/decoder which has monoural channel,
        // the `number_of_channels` argument is passed as `1`. However, Unity expects
        // to receive audio which is stereo channel usually, in this case we need to
        // resample audio monoural to stereo.

        std::unique_ptr<AudioFrame> frame = Resample(
            audio_data,
            number_of_frames,
            number_of_channels,
            _channels,
            sample_rate,
            _sampleRate);

        const size_t size = number_of_channels * number_of_frames;
        const int16_t* data = static_cast<const int16_t*>(audio_data);

        std::vector<float_t> _converted_data(size);
        for (size_t i = 0; i < size; i++)
        {
            _converted_data[i] = webrtc::S16ToFloat(data[i]);
        }

        _callback(
            this,
            _converted_data.data(),
            size,
            _sampleRate,
            _channels,
            number_of_frames);
    }

    std::unique_ptr<AudioFrame> AudioTrackSinkAdapter::Resample(
        const void* audio_data,
        const size_t number_of_frames,
        const size_t channels,
        const size_t dst_channels,
        const uint32_t sample_rate,
        const uint32_t dst_sample_rate
        )
    {
        std::unique_ptr<AudioFrame> audio_frame = std::make_unique<AudioFrame>();
        audio_frame->num_channels_ = dst_channels;
        audio_frame->sample_rate_hz_  = dst_sample_rate;
        voe::RemixAndResample(static_cast<const int16_t*>(audio_data),
            number_of_frames, channels, sample_rate,
            &_resampler, audio_frame.get());
        return std::move(audio_frame);
    }
} // end namespace webrtc
} // end namespace unity
