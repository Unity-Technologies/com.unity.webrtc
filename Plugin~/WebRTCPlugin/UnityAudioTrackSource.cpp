#include "pch.h"

#include <common_audio/include/audio_util.h>
#include <modules/audio_processing/include/audio_processing.h>
#include <rtc_base/ref_counted_object.h>

#include "UnityAudioTrackSource.h"

namespace unity
{
namespace webrtc
{

    rtc::scoped_refptr<UnityAudioTrackSource> UnityAudioTrackSource::Create()
    {
        rtc::scoped_refptr<UnityAudioTrackSource> source(new rtc::RefCountedObject<UnityAudioTrackSource>());
        return source;
    }

    rtc::scoped_refptr<UnityAudioTrackSource> UnityAudioTrackSource::Create(const cricket::AudioOptions& audio_options)
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

        auto i = std::find(_arrSink.begin(), _arrSink.end(), sink);
        if (i != _arrSink.end())
            _arrSink.erase(i);
    }

    void UnityAudioTrackSource::PushAudioData(
        const float* pAudioData, int nSampleRate, size_t nNumChannels, size_t nNumFrames)
    {
        RTC_DCHECK(pAudioData);
        RTC_DCHECK(nSampleRate);
        RTC_DCHECK(nNumChannels);
        RTC_DCHECK(nNumFrames);

        std::lock_guard<std::mutex> lock(_mutex);

        // eg.  80 for 8KHz and 160 for 16kHz
        size_t nNumFramesFor10ms = static_cast<size_t>(nSampleRate / 100);
        size_t nNumSamplesFor10ms = nNumFramesFor10ms * nNumChannels;
        constexpr size_t nBitPerSample = sizeof(int16_t) * 8;

        if (_sampleRate != nSampleRate || _numChannels != nNumChannels || _numFrames != nNumFrames)
        {
            _sampleRate = nSampleRate;
            _numChannels = nNumChannels;
            _numFrames = nNumFrames;
            _convertedAudioData.clear();
            _convertedAudioData.reserve(nNumSamplesFor10ms * 20);
        }

        for (size_t i = 0; i < nNumFrames; i++)
            _convertedAudioData.push_back(::webrtc::FloatToS16(pAudioData[i]));

        while (_convertedAudioData.size() >= nNumSamplesFor10ms)
        {
            if (_audioProcessing)
            {
                StreamConfig streamConfig(nSampleRate, nNumChannels);
                _audioProcessing->ProcessStream(
                    _convertedAudioData.data(),
                    streamConfig,
                    streamConfig,
                    _convertedAudioData.data());
            }

            for (auto sink : _arrSink)
                sink->OnData(_convertedAudioData.data(), nBitPerSample, nSampleRate, nNumChannels, nNumFramesFor10ms);
            _convertedAudioData.erase(_convertedAudioData.begin(), _convertedAudioData.begin() + nNumSamplesFor10ms);
        }
    }

    UnityAudioTrackSource::UnityAudioTrackSource() { }
    UnityAudioTrackSource::UnityAudioTrackSource(const cricket::AudioOptions& audio_options)
        : _options(audio_options)
    {
        // Build and configure the APM so that the options are actually applied.
        // cricket::AudioOptions are stored as metadata by LocalAudioSource but are
        // never wired into any real processing — PushAudioData() bypasses the WebRTC
        // voice engine entirely, so we must drive the APM ourselves here.
        AudioProcessing::Config apmConfig;

        apmConfig.noise_suppression.enabled =
            audio_options.noise_suppression.value_or(true);
        apmConfig.noise_suppression.level =
            AudioProcessing::Config::NoiseSuppression::kHigh;

        apmConfig.gain_controller1.enabled =
            audio_options.auto_gain_control.value_or(true);
        apmConfig.gain_controller1.mode =
            AudioProcessing::Config::GainController1::kAdaptiveDigital;

        apmConfig.high_pass_filter.enabled =
            audio_options.highpass_filter.value_or(false);

        apmConfig.echo_canceller.enabled =
            audio_options.echo_cancellation.value_or(false);

        _audioProcessing = AudioProcessingBuilder().Create();
        _audioProcessing->ApplyConfig(apmConfig);
    }

    UnityAudioTrackSource::~UnityAudioTrackSource() { }

} // end namespace webrtc
} // end namespace unity
