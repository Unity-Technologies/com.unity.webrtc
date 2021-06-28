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

    void AudioTrackSinkAdapter::GetData(float_t* audio_data, int32_t size)
    {
        std::lock_guard<std::mutex> lock(m_mutex);
        if (size > _data.size())
            return;

        std::memcpy(audio_data, _data.data(), size);
        _data.erase(
            _data.begin(),
            _data.begin() + size);
    }

    void AudioTrackSinkAdapter::OnData(
        const void* audio_data, int bits_per_sample, int sample_rate,
        size_t number_of_channels, size_t number_of_frames)
    {
        std::lock_guard<std::mutex> lock(m_mutex);

        const size_t size = number_of_channels * number_of_frames;
        const int16_t* data = static_cast<const int16_t*>(audio_data);
        const float_t INVERSE = 1.0 / SHRT_MAX;

        //_data.clear();

        //int16_t max = 0;
        for (int i = 0; i < size; i++)
        {
            _data.push_back(data[i] * INVERSE);

            //max = std::max(max, data[i]);
        }

        if (_callback != nullptr)
        {
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
} // end namespace webrtc
} // end namespace unity
