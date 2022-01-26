#pragma once

#include <mutex>
#include "WebRTCPlugin.h"
#include "common_audio/ring_buffer.h"
#include "audio/remix_resample.h"

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
        void ProcessAudio(float* data, int32 size, int channels, int sampleRate);
    private:
        std::mutex m_mutex;
        PushResampler<int16_t> m_resampler;
        AudioFrame m_frame;
        RingBuffer* m_ringBuffer;
        int16_t *m_bufferIn;
        int m_bufferInLen;
    };
} // end namespace webrtc
} // end namespace unity
