#include "pch.h"
#include "DummyAudioDevice.h"

namespace unity
{
namespace webrtc
{

    void DummyAudioDevice::ProcessAudioData(const float* data, int32 size)
    {
        if (started && isRecording)
        {
            for (int i = 0; i < size; i++)
            {
#pragma warning (suppress: 4244)
                convertedAudioData.push_back(data[i] >= 0 ? data[i] * SHRT_MAX : data[i] * -SHRT_MIN);
            }
            //opus supports up to 48khz sample rate, enforce 48khz here for quality
            size_t chunkSize = 48000 * 2 / 100;
            while (convertedAudioData.size() > chunkSize)
            {
                deviceBuffer->SetRecordedBuffer(convertedAudioData.data(), chunkSize / 2);
                deviceBuffer->DeliverRecordedData();
                convertedAudioData.erase(convertedAudioData.begin(), convertedAudioData.begin() + chunkSize);
            }
        }
    }

    void DummyAudioDevice::pollAudioData()
    {
        auto pollingTime = std::chrono::high_resolution_clock::now();
        while (isPlaying)
        {
            pollFromSource();

            auto now = std::chrono::high_resolution_clock::now();
            auto delayUntilNextPolling = pollingTime + std::chrono::milliseconds(pollInterval) - now;
            if (delayUntilNextPolling < std::chrono::seconds(0)) {
                delayUntilNextPolling = std::chrono::seconds(0);
            }
            pollingTime = now + delayUntilNextPolling;
            std::this_thread::sleep_for(delayUntilNextPolling);
        }
    }

    void DummyAudioDevice::pollFromSource()
    {
        if (!audio_transport_)
            return;

        int64_t elapsedTime = -1;
        int64_t ntpTime = -1;
        char* audio_data = new char[bytesPerSample * samplesPerFrame * channels];
        audio_transport_->PullRenderData(bytesPerSample * 8, samplingRate, channels, samplesPerFrame, audio_data, &elapsedTime, &ntpTime);
        
    }

} // end namespace webrtc
} // end namespace unity
