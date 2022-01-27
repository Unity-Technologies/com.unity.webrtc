#pragma once

extern "C" {
#include "common_audio/ring_buffer.h"
}

#include <mutex>
#include "api/media_stream_interface.h"
#include "api/audio/audio_frame.h"
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
        AudioTrackSinkAdapter();
        ~AudioTrackSinkAdapter() override;

        void OnData(
            const void* audio_data,
            int bits_per_sample,
            int sample_rate,
            size_t number_of_channels,
            size_t number_of_frames) override;

        void ProcessAudio(
            float* data, size_t length, size_t channels, int32_t sampleRate);
    private:
        void ResizeBuffer(size_t channels, int32_t sampleRate, size_t length);

        const size_t kChannels = 2;
        const int32_t kSampleRate = 48000;
        const size_t kBufferSize = kChannels * kSampleRate;
        AudioFrame _frame;
        std::mutex _mutex;
        RingBuffer* _buffer;
        std::vector<int16_t> _bufferIn;

        PushResampler<int16_t> _resampler;
        int32_t _sampleRate = kSampleRate;
        size_t _channels = kChannels;

    };
} // end namespace webrtc
} // end namespace unity
