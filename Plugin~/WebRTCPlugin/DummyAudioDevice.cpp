#include "pch.h"
#include "DummyAudioDevice.h"
#include "system_wrappers/include/sleep.h"

namespace unity
{
namespace webrtc
{

    void DummyAudioDevice::ProcessAudioData(const float* data, int32 size, const size_t nChannels)
    {
        if (started && isRecording)
        {
            // set dummy data
            convertedAudioData.resize(_recordingFramesIn10MS * nChannels);
            std::memset(convertedAudioData.data(), 0, _recordingFramesIn10MS * nChannels);

            //opus supports up to 48khz sample rate, enforce 48khz here for quality
            //size_t chunkSize = 48000 * nChannels / 100;

            size_t numElements10ms = _recordingFramesIn10MS * nChannels;

            while (convertedAudioData.size() >= numElements10ms)
            {
                deviceBuffer->SetRecordedBuffer(convertedAudioData.data(), _recordingFramesIn10MS);
                deviceBuffer->DeliverRecordedData();
                convertedAudioData.erase(convertedAudioData.begin(), convertedAudioData.begin() + _recordingFramesIn10MS);
            }
        }
    }

    bool DummyAudioDevice::RecThreadProcess()
    {
        if (!isRecording) {
            return false;
        }

        int64_t currentTime = rtc::TimeMillis();
        //mutex_.Lock();

        if (_lastCallRecordMillis == 0 || currentTime - _lastCallRecordMillis >= 10) {
        //    if (_inputFile.is_open()) {
        //        if (_inputFile.Read(_recordingBuffer, kRecordingBufferSize) > 0) {
        //            _ptrAudioBuffer->SetRecordedBuffer(_recordingBuffer,
        //                _recordingFramesIn10MS);
        //        }
        //        else {
        //            _inputFile.Rewind();
        //        }
            _lastCallRecordMillis = currentTime;
            //mutex_.Unlock();
            //deviceBuffer->DeliverRecordedData();
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
