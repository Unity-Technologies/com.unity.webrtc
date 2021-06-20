#include "pch.h"
#include "UnityAudioTrackSource.h"

#include <mutex>

namespace unity
{
namespace webrtc
{

rtc::scoped_refptr<UnityAudioTrackSource> UnityAudioTrackSource::Create(const std::string& sTrackName)
{
    rtc::scoped_refptr<UnityAudioTrackSource> source(
        new rtc::RefCountedObject<UnityAudioTrackSource>(sTrackName));
    return source;
}

rtc::scoped_refptr<UnityAudioTrackSource> UnityAudioTrackSource::Create(
    const std::string& sTrackName, const cricket::AudioOptions& audio_options)
{
    rtc::scoped_refptr<UnityAudioTrackSource> source(
        new rtc::RefCountedObject<UnityAudioTrackSource>(sTrackName, audio_options));
    return source;
}

void UnityAudioTrackSource::AddSink(::webrtc::AudioTrackSinkInterface* sink)
{
    m_pAudioTrackSinkInterface = sink;
}
void UnityAudioTrackSource::RemoveSink(::webrtc::AudioTrackSinkInterface* sink)
{
    m_pAudioTrackSinkInterface = nullptr;
}

void UnityAudioTrackSource::OnData(const float* pAudioData, int nSampleRate, size_t nNumChannels, size_t nNumFrames)
{
    if (!m_pAudioTrackSinkInterface)
        return;

    for (size_t i = 0; i < nNumFrames; i++)
    {
        convertedAudioData.push_back(pAudioData[i] >= 0 ? pAudioData[i] * SHRT_MAX : pAudioData[i] * -SHRT_MIN);
    }

    // eg.  80 for 8KHz and 160 for 16kHz
    size_t nNumFramesFor10ms = nSampleRate / 100;
    size_t size = convertedAudioData.size() / (nNumFramesFor10ms * nNumChannels);
    size_t nBitPerSample = sizeof(int16_t) * 8;
    for (size_t i = 0; i < size; i++)
    {
        m_pAudioTrackSinkInterface->OnData(
            &convertedAudioData.data()[i * nNumFramesFor10ms * nNumChannels],
            nBitPerSample, nSampleRate, nNumChannels, nNumFramesFor10ms);
    }

    // pop processed buffer, remained buffer will be processed the next time.
    convertedAudioData.erase(
        convertedAudioData.begin(),
        convertedAudioData.begin() + nNumFramesFor10ms * nNumChannels * size);
}

UnityAudioTrackSource::UnityAudioTrackSource(const std::string& sTrackName)
    : m_sTrackName(sTrackName)
    , m_pAudioTrackSinkInterface(nullptr)
{
}
UnityAudioTrackSource::UnityAudioTrackSource(const std::string& sTrackName, const cricket::AudioOptions& audio_options)
    : m_sTrackName(sTrackName)
    , m_pAudioTrackSinkInterface(nullptr)
{
}

UnityAudioTrackSource::~UnityAudioTrackSource()
{
}

} // end namespace webrtc
} // end namespace unity
