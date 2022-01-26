#include "pch.h"
#include "AudioTrackSinkAdapter.h"
#include "common_audio/include/audio_util.h"

namespace unity
{
namespace webrtc
{
    AudioTrackSinkAdapter::AudioTrackSinkAdapter()
        : m_ringBuffer(nullptr), m_bufferIn(nullptr), m_bufferInLen(0)
    {
    }

    AudioTrackSinkAdapter::~AudioTrackSinkAdapter()
    {
        if (m_ringBuffer)
        {
            WebRtc_FreeBuffer(m_ringBuffer);
            m_ringBuffer = nullptr;
        }
        if (m_bufferIn)
        {
            free(m_bufferIn);
            m_bufferIn = nullptr;
            m_bufferInLen = 0;
        }
    }

    void AudioTrackSinkAdapter::OnData(
        const void* audio_data, int bits_per_sample, int sample_rate,
        size_t number_of_channels, size_t number_of_frames)
    {
        if (m_ringBuffer) {
            std::lock_guard<std::mutex> lock(m_mutex);
            webrtc::voe::RemixAndResample((const int16_t*) audio_data, number_of_frames, number_of_channels, sample_rate, &m_resampler, &m_frame);
            WebRtc_WriteBuffer(m_ringBuffer, m_frame.data(), m_frame.num_channels() * m_frame.samples_per_channel());
        }
    }

    void AudioTrackSinkAdapter::ProcessAudio(float* data, int32 size, int channels, int sampleRate)
    {
        int samplesLeft = size / channels;
        int sampleIdx = 0;

        if (sampleRate == 0)
            return;

        memset(data, 0, sizeof(float) * samplesLeft * channels);
        bool update = size > m_bufferInLen || m_frame.num_channels() != (size_t)channels || m_frame.sample_rate_hz() != sampleRate;

        if (update || !m_ringBuffer)
        {
            std::lock_guard<std::mutex> lock(m_mutex);

            // A change in configuration during runtime is very unlikely
            m_frame.num_channels_ = channels;
            m_frame.sample_rate_hz_ = sampleRate;
            m_frame.mutable_data(); // unmute

            if (m_ringBuffer)
                WebRtc_FreeBuffer(m_ringBuffer);
            m_ringBuffer = WebRtc_CreateBuffer((int)(0.2f * m_frame.sample_rate_hz()) * m_frame.num_channels(), sizeof(int16_t));
            if (m_bufferIn)
                free(m_bufferIn);
            m_bufferInLen = (samplesLeft + 1024) * m_frame.num_channels();
            m_bufferIn = (int16_t *) calloc(m_bufferInLen, sizeof(int16_t));
        }

        while (samplesLeft > 0)
        {
            int samplesGot = WebRtc_ReadBuffer(m_ringBuffer, NULL, m_bufferIn, samplesLeft * channels);
            samplesGot = samplesGot / channels;
            if (samplesGot == 0)
                break;
            for (int i = 0; i < samplesGot * channels; i++)
                data[sampleIdx + i] = webrtc::S16ToFloat(m_bufferIn[i]);
            samplesLeft -= samplesGot;
            sampleIdx += samplesGot * channels;
        }
    }
} // end namespace webrtc
} // end namespace unity
