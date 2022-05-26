#include "pch.h"

#include <audio/remix_resample.h>
#include <common_audio/include/audio_util.h>

#include "AudioTrackSinkAdapter.h"

namespace unity
{
namespace webrtc
{
    AudioTrackSinkAdapter::AudioTrackSinkAdapter()
        : _buffer(nullptr)
    {
    }

    AudioTrackSinkAdapter::~AudioTrackSinkAdapter() { WebRtc_FreeBuffer(_buffer); }

    void AudioTrackSinkAdapter::OnData(
        const void* audio_data,
        int bits_per_sample,
        int sample_rate,
        size_t number_of_channels,
        size_t number_of_frames)
    {
        std::lock_guard<std::mutex> lock(_mutex);

        if (_buffer == nullptr)
            return;

        // note: AudioTrackSinkInterface::OnData method is passed audio data from
        // audio decoder directly, so we need to resample for expected format.
        // For example, when we use encoder/decoder which has monoural channel,
        // the `number_of_channels` argument is passed as `1`. However, Unity expects
        // to receive audio which is stereo channel usually, in this case we need to
        // resample audio monoural to stereo.
        webrtc::voe::RemixAndResample(
            static_cast<const int16_t*>(audio_data),
            number_of_frames,
            number_of_channels,
            sample_rate,
            &_resampler,
            &_frame);

        size_t length = _frame.num_channels() * _frame.samples_per_channel();

        WebRtc_WriteBuffer(_buffer, _frame.data(), length);
    }

    void AudioTrackSinkAdapter::ResizeBuffer(size_t channels, int32_t sampleRate, size_t length)
    {
        RTC_DCHECK(channels);
        RTC_DCHECK(sampleRate);

        // reallocate ring buffer, keep it relatively short at 0.2s
        size_t bufferSize = static_cast<size_t>(static_cast<float>(channels) * static_cast<float>(sampleRate) * 0.2f);
        if (_buffer != nullptr)
            WebRtc_FreeBuffer(_buffer);
        _buffer = WebRtc_CreateBuffer(bufferSize, sizeof(int16_t));

        // reset audio frame.
        _frame.num_channels_ = channels;
        _frame.sample_rate_hz_ = sampleRate;

        // reallocate temporary buffer.
        _bufferIn.resize(length);
    }

    void AudioTrackSinkAdapter::ProcessAudio(float* data, size_t length, size_t channels, int32_t sampleRate)
    {
        RTC_DCHECK(data);
        RTC_DCHECK(length);
        RTC_DCHECK(channels);
        RTC_DCHECK(sampleRate);

        std::memset(data, 0, sizeof(float) * length);

        std::lock_guard<std::mutex> lock(_mutex);

        // Reallocate audio buffer when Unity changes channel count, sample rate,
        // or data length.
        if (_buffer == nullptr || _frame.num_channels_ != channels || _frame.sample_rate_hz_ != sampleRate ||
            _bufferIn.size() != length)
        {
            ResizeBuffer(channels, sampleRate, length);
        }

        size_t readLength = WebRtc_ReadBuffer(_buffer, nullptr, _bufferIn.data(), length);

        for (size_t i = 0; i < readLength; i++)
            data[i] = webrtc::S16ToFloat(_bufferIn[i]);
    }
} // end namespace webrtc
} // end namespace unity
