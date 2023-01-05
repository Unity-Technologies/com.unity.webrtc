#include "apm.h"
#include "Debug.h"
#include <iostream>
#include <modules/audio_processing/include/audio_processing.h>
#include <common_audio/include/audio_util.h>
#include "IUnityInterface.h"

using namespace webrtc;

namespace unity
{
namespace webrtc
{
    extern "C" UNITY_INTERFACE_EXPORT HANDLE WebRtc_AudioProcessing_Create() {
//        _buffer = WebRtc_CreateBuffer(sample_rate * channels * 0.2f, sizeof(int16_t));
        _webrtc_ap = AudioProcessingBuilder().Create();
        return _webrtc_ap;
    }

    extern "C" UNITY_INTERFACE_EXPORT int WebRtc_test() {
        return 2;
    }

    extern "C" UNITY_INTERFACE_EXPORT void WebRtc_AudioProcessing_Destroy(HANDLE handle) {
        auto *audio_processing = static_cast<AudioProcessing *>(handle);
        audio_processing->Release();
    }

    extern "C"  UNITY_INTERFACE_EXPORT void WebRtc_AudioProcessing_ApplyConfig(HANDLE handle, bool echo_cancellation,
                                                       bool use_mobile_software_aec,
                                                       bool auto_gain_control,
                                                       bool noise_suppression,
                                                       bool highpass_filter,
                                                       bool residual_echo_detector,
                                                       bool typing_detection) {

        auto *ap = static_cast<AudioProcessing *>(handle);
        AudioProcessing::Config apm_config = ap->GetConfig();
        apm_config.echo_canceller.enabled = echo_cancellation;
        apm_config.echo_canceller.mobile_mode = use_mobile_software_aec;
        {
            const bool enabled = auto_gain_control;
            apm_config.gain_controller1.enabled = enabled;
#if defined(WEBRTC_IOS) || defined(WEBRTC_ANDROID)
            apm_config.gain_controller1.mode =
                apm_config.gain_controller1.kFixedDigital;
#else
            apm_config.gain_controller1.mode =
                    apm_config.gain_controller1.kAdaptiveAnalog;
#endif
            constexpr int kMinVolumeLevel = 0;
            constexpr int kMaxVolumeLevel = 255;
            apm_config.gain_controller1.analog_level_minimum = kMinVolumeLevel;
            apm_config.gain_controller1.analog_level_maximum = kMaxVolumeLevel;
        }
        apm_config.high_pass_filter.enabled = highpass_filter;
        apm_config.residual_echo_detector.enabled = residual_echo_detector;
        {
            const bool enabled = noise_suppression;
            apm_config.noise_suppression.enabled = enabled;
            apm_config.noise_suppression.level =
                    AudioProcessing::Config::NoiseSuppression::Level::kHigh;
        }
        apm_config.voice_detection.enabled = typing_detection;
        ap->ApplyConfig(apm_config);
    }

   extern "C" UNITY_INTERFACE_EXPORT  int 
    WebRtc_AudioProcessing_ProcessStream(HANDLE handle, float *src, int sample_rate_hz, int num_channels,
                                         int samples) {

        for (int i = 0; i < samples; i++) {
            _converted_data.push_back(FloatToS16(src[i]));
            src[i] = 0;
        }

        // eg.  80 for 8KHz and 160 for 16kHz
        int nNumFramesFor10ms = sample_rate_hz / 100;   // 441
        int nNumSamplesFor10ms = nNumFramesFor10ms * num_channels;  // 882
        int nNeedProcessedSegments = (samples + nNumSamplesFor10ms - 1) / nNumSamplesFor10ms;
        int nNeedProcessedSamples = nNeedProcessedSegments * nNumSamplesFor10ms;
        Debug::Log("[AEC]variables: nNumSamplesFor10ms" + std::to_string(nNumSamplesFor10ms)
                   + "nNumFramesFor10ms: " + std::to_string(nNumFramesFor10ms)
                   + "nNeedProcessedSegments: " + std::to_string(nNeedProcessedSegments)
                   + "nNeedProcessedSamples: " + std::to_string(nNeedProcessedSamples)
        );

        if (_converted_data.size() < (size_t)nNeedProcessedSamples) {
            Debug::Log("[AEC]no need process");
            return 1;
        }

        _processed_data.clear();
        _processed_data.resize(nNeedProcessedSamples);
        auto *ap = static_cast<AudioProcessing *>(handle);
        const int kSystemDelayMs = 66; // no idea why it's 66.
        ap->set_stream_delay_ms(kSystemDelayMs);

        Debug::Log("[AEC]start processing, data size: " + std::to_string(_converted_data.size()));
        for (int i = 0; i < nNeedProcessedSegments; ++i) {
            ap->ProcessStream(_converted_data.data(),
                              StreamConfig(sample_rate_hz, num_channels),
                              StreamConfig(sample_rate_hz, num_channels),
                              _processed_data.data() + i * nNumSamplesFor10ms);

            // clear processed data
            if (samples - i * nNumSamplesFor10ms >= nNumSamplesFor10ms) {
                _converted_data.erase(_converted_data.begin(), _converted_data.begin() + nNumSamplesFor10ms);
            } else {
                _converted_data.erase(_converted_data.begin(),
                                      _converted_data.begin() + samples - i * nNumSamplesFor10ms);
            }

            Debug::Log("[AEC] processing round " + std::to_string(i) + ", data size: " +
                       std::to_string(_converted_data.size()));
        }
        Debug::Log("[AEC]end processing, converted_data size: " + std::to_string(_converted_data.size()) +
                   " processed_data size: " +
                   std::to_string(_processed_data.size()));

        for (int i = 0; i < samples; ++i) {
            src[i] = S16ToFloat(_processed_data[i]);
        }
        Debug::Log("[AEC] end end");
        return 0;
    }

   extern "C" UNITY_INTERFACE_EXPORT int WebRtc_AudioProcessing_ProcessReverseStream(float *src, int sample_rate_hz, int num_channels, int samples) {
        if (_webrtc_ap == nullptr) return 1;
        Debug::Log("[AEC-R] process reverse stream");
        auto *ap = static_cast<AudioProcessing *>(_webrtc_ap);

        for (int i = 0; i < samples; i++) {
            _converted_data_reverse.push_back(FloatToS16(src[i]));
            src[i] = 0;
        }

        // eg.  80 for 8KHz and 160 for 16kHz
        int nNumFramesFor10ms = sample_rate_hz / 100;   // 441
        int nNumSamplesFor10ms = nNumFramesFor10ms * num_channels;  // 882
        int nNeedProcessedSegments = (samples + nNumSamplesFor10ms - 1) / nNumSamplesFor10ms;
        int nNeedProcessedSamples = nNeedProcessedSegments * nNumSamplesFor10ms;
        Debug::Log("[AEC-R]variables: nNumSamplesFor10ms" + std::to_string(nNumSamplesFor10ms)
                   + "nNumFramesFor10ms: " + std::to_string(nNumFramesFor10ms)
                   + "nNeedProcessedSegments: " + std::to_string(nNeedProcessedSegments)
                   + "nNeedProcessedSamples: " + std::to_string(nNeedProcessedSamples)
        );

        if (_converted_data_reverse.size() < (size_t)nNeedProcessedSamples) {
            Debug::Log("[AEC-R]no need process");
            return 1;
        }

        _processed_data_reverse.clear();
        _processed_data_reverse.resize(nNeedProcessedSamples);

        Debug::Log("[AEC-R]start processing, data size: " + std::to_string(_converted_data_reverse.size()));
        for (int i = 0; i < nNeedProcessedSegments; ++i) {
            ap->ProcessReverseStream(_converted_data_reverse.data(),
                                     StreamConfig(sample_rate_hz, num_channels),
                                     StreamConfig(sample_rate_hz, num_channels),
                                     _processed_data_reverse.data() + i * nNumSamplesFor10ms);

            // clear processed data
            if (samples - i * nNumSamplesFor10ms >= nNumSamplesFor10ms) {
                _converted_data_reverse.erase(_converted_data_reverse.begin(),
                                              _converted_data_reverse.begin() + nNumSamplesFor10ms);
            } else {
                _converted_data_reverse.erase(_converted_data_reverse.begin(),
                                              _converted_data_reverse.begin() + samples - i * nNumSamplesFor10ms);
            }

            Debug::Log("[AEC-R] processing round " + std::to_string(i) + ", data size: " +
                       std::to_string(_converted_data_reverse.size()));
        }
        Debug::Log("[AEC-R]end processing, converted_data size: " + std::to_string(_converted_data_reverse.size()) +
                   " processed_data size: " +
                   std::to_string(_processed_data_reverse.size()));

        for (int i = 0; i < samples; ++i) {
            src[i] = S16ToFloat(_processed_data_reverse[i]);
        }
        Debug::Log("[AEC-R] end end");
        return 0;
    }
}
}