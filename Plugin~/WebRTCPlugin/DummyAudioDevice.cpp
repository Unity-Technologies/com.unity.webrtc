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

} // end namespace webrtc
} // end namespace unity
