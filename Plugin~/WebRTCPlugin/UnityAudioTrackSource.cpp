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
