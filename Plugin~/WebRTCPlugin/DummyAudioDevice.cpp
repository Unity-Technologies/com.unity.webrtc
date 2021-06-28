#include "pch.h"
#include "DummyAudioDevice.h"
#include "system_wrappers/include/sleep.h"

namespace unity
{
namespace webrtc
{

    bool DummyAudioDevice::RecThreadProcess()
    {
        if (!isRecording) {
            return false;
        }

        int64_t currentTime = rtc::TimeMillis();
        //mutex_.Lock();

        if (_lastCallRecordMillis == 0 || currentTime - _lastCallRecordMillis >= 10) {
            _lastCallRecordMillis = currentTime;
            //mutex_.Unlock();

            const int kBytesPerSample = 2;
            const int kChannels = 2;

            const int kSamplingRate = 48000;

            // Webrtc uses 10ms frames.
            const int kFrameLengthMs = 10;
            const int kSamplesPerFrame = kSamplingRate * kFrameLengthMs / 1000;

            int64_t elapsed_time_ms = -1;
            int64_t ntp_time_ms = -1;
            char data[kBytesPerSample * kChannels * kSamplesPerFrame];

            audio_transport_->PullRenderData(kBytesPerSample * 8, kSamplingRate,
                kChannels, kSamplesPerFrame, data,
                &elapsed_time_ms, &ntp_time_ms);
            //mutex_.Lock();
        }

        //mutex_.Unlock();

        int64_t deltaTimeMillis = rtc::TimeMillis() - currentTime;
        if (deltaTimeMillis < 10) {
            SleepMs(10 - deltaTimeMillis);
        }
        return true;
    }

} // end namespace webrtc
} // end namespace unity
