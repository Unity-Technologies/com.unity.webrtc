#pragma once

#include <mutex>

#include <api/audio/audio_frame.h>
#include <api/media_stream_interface.h>
#include <common_audio/resampler/include/push_resampler.h>
#include <common_audio/ring_buffer.h>

namespace unity
{
namespace webrtc
{
    using namespace ::webrtc;

    class AudioTrackSinkAdapter : public webrtc::AudioTrackSinkInterface
    {
    public:
        AudioTrackSinkAdapter();
        ~AudioTrackSinkAdapter() override;

        void OnData(
            const void* audio_data,
            int bits_per_sample,
            int sample_rate,
            size_t number_of_channels,
            size_t number_of_frames) override;

        void ProcessAudio(float* data, size_t length, size_t channels, int32_t sampleRate);

    private:
        void ResizeBuffer(size_t channels, int32_t sampleRate, size_t length);

        AudioFrame _frame;
        std::mutex _mutex;
        RingBuffer* _buffer;
        std::vector<int16_t> _bufferIn;

        PushResampler<int16_t> _resampler;
    };
} // end namespace webrtc
} // end namespace unity
