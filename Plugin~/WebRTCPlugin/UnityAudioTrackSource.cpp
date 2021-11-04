#include "pch.h"
#include "UnityAudioTrackSource.h"

namespace unity
{
namespace webrtc
{

rtc::scoped_refptr<UnityAudioTrackSource> UnityAudioTrackSource::Create()
{
    rtc::scoped_refptr<UnityAudioTrackSource> source(
        new rtc::RefCountedObject<UnityAudioTrackSource>());
    return source;
}

rtc::scoped_refptr<UnityAudioTrackSource> UnityAudioTrackSource::Create(
    const cricket::AudioOptions& audio_options)
{
    rtc::scoped_refptr<UnityAudioTrackSource> source(
        new rtc::RefCountedObject<UnityAudioTrackSource>(audio_options));
    return source;
}

void UnityAudioTrackSource::AddSink(AudioTrackSinkInterface* sink)
{
    std::lock_guard<std::mutex> lock(_mutex);

    _arrSink.push_back(sink);
}

void UnityAudioTrackSource::RemoveSink(AudioTrackSinkInterface* sink)
{
    std::lock_guard<std::mutex> lock(_mutex);

    auto i= std::find(_arrSink.begin(), _arrSink.end(), sink);
    if (i != _arrSink.end())
        _arrSink.erase(i);
}

void UnityAudioTrackSource::PushAudioData(const float* pAudioData, int nSampleRate, size_t nNumChannels, size_t nNumFrames)
{
    RTC_DCHECK(pAudioData);
    RTC_DCHECK(nSampleRate);
    RTC_DCHECK(nNumChannels);
    RTC_DCHECK(nNumFrames);

    std::lock_guard<std::mutex> lock(_mutex);

    if (_arrSink.empty())
        return;

    _convertedAudioData.reserve(_convertedAudioData.size() + nNumFrames);
    for (size_t i = 0; i < nNumFrames; i++)
    {
        _convertedAudioData.push_back(pAudioData[i] >= 0 ? pAudioData[i] * SHRT_MAX : pAudioData[i] * -SHRT_MIN);
    }
}

void UnityAudioTrackSource::SendAudioData(int nSampleRate, size_t nNumChannels) {
    RTC_DCHECK(nSampleRate);
    RTC_DCHECK(nNumChannels);

    std::lock_guard<std::mutex> lock(_mutex);

    // eg.  80 for 8KHz and 160 for 16kHz
    size_t nNumFramesFor10ms = nSampleRate / 100;
    size_t nNumSamplesFor10ms = nNumFramesFor10ms * nNumChannels;
    size_t nNumSamplesForBuffering = nNumSamplesFor10ms * 10;
    constexpr size_t nBitPerSample = sizeof(int16_t) * 8;

    if (!_bufferInit && _convertedAudioData.size() > nNumSamplesForBuffering) {
        _convertedAudioData.erase(
            _convertedAudioData.begin(),
            _convertedAudioData.begin() + (_convertedAudioData.size() - nNumSamplesForBuffering));
        _bufferInit = true;
    }

    if (_bufferInit && _convertedAudioData.size() >= nNumSamplesFor10ms) {
        for (auto sink : _arrSink)
        {
            sink->OnData(_convertedAudioData.data(),
                nBitPerSample, nSampleRate, nNumChannels, nNumFramesFor10ms);
        }

        // pop processed buffer, remained buffer will be processed the next time.
        _convertedAudioData.erase(
            _convertedAudioData.begin(),
            _convertedAudioData.begin() + nNumSamplesFor10ms);
    }
}

UnityAudioTrackSource::UnityAudioTrackSource()
{
}
UnityAudioTrackSource::UnityAudioTrackSource(const cricket::AudioOptions& audio_options)
    : _options(audio_options) {
}

UnityAudioTrackSource::~UnityAudioTrackSource()
{
}

} // end namespace webrtc
} // end namespace unity
