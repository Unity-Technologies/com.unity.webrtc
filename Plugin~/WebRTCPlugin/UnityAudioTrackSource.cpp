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

void UnityAudioTrackSource::OnData(const float* pAudioData, int nSampleRate, size_t nNumChannels, size_t nNumFrames)
{
    std::lock_guard<std::mutex> lock(_mutex);

    if (_arrSink.empty())
        return;

    for (size_t i = 0; i < nNumFrames; i++)
    {
        _convertedAudioData.push_back(pAudioData[i] >= 0 ? pAudioData[i] * SHRT_MAX : pAudioData[i] * -SHRT_MIN);
    }

    // eg.  80 for 8KHz and 160 for 16kHz
    size_t nNumFramesFor10ms = nSampleRate / 100;
    size_t size = _convertedAudioData.size() / (nNumFramesFor10ms * nNumChannels);
    size_t nBitPerSample = sizeof(int16_t) * 8;

    for(auto sink : _arrSink)
    {
        for (size_t i = 0; i < size; i++)
        {
            sink->OnData(
                &_convertedAudioData.data()[i * nNumFramesFor10ms * nNumChannels],
                nBitPerSample, nSampleRate, nNumChannels, nNumFramesFor10ms);
        }
    }

    // pop processed buffer, remained buffer will be processed the next time.
    _convertedAudioData.erase(
        _convertedAudioData.begin(),
        _convertedAudioData.begin() + nNumFramesFor10ms * nNumChannels * size);
}

UnityAudioTrackSource::UnityAudioTrackSource()
{
}
UnityAudioTrackSource::UnityAudioTrackSource(const cricket::AudioOptions& audio_options)
{
}

UnityAudioTrackSource::~UnityAudioTrackSource()
{
}

} // end namespace webrtc
} // end namespace unity
