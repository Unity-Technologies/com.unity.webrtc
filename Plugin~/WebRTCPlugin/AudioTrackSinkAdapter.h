#pragma once

#include <mutex>
#include "WebRTCPlugin.h"

namespace unity
{
namespace webrtc
{
    using namespace ::webrtc;

    class AudioTrackSinkAdapter : public webrtc::AudioTrackSinkInterface
    {
    public:
        AudioTrackSinkAdapter(AudioTrackInterface* track, DelegateAudioReceive callback);
        ~AudioTrackSinkAdapter() override {};

        void OnData(
            const void* audio_data,
            int bits_per_sample,
            int sample_rate,
            size_t number_of_channels,
            size_t number_of_frames) override;
    private:
        rtc::scoped_refptr<webrtc::VideoFrameBuffer> m_frameBuffer;
        AudioTrackInterface* _track;
        DelegateAudioReceive _callback;
    };
} // end namespace webrtc
} // end namespace unity
