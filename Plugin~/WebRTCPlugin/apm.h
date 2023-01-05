#ifndef AEC_APM_H
#define AEC_APM_H

#include <stdint.h>
#include <vector>
#include "IUnityInterface.h"

typedef void *HANDLE;

#ifdef __cplusplus
extern "C" {
#endif

namespace unity
{
namespace webrtc
{
    static std::vector<int16_t> _converted_data;
    static std::vector<int16_t> _processed_data;

    static std::vector<int16_t> _converted_data_reverse;
    static std::vector<int16_t> _processed_data_reverse;

    static HANDLE _webrtc_ap;
//    const int sample_rate= 44100;
//    const int channels = 2;

    UNITY_INTERFACE_EXPORT HANDLE WebRtc_AudioProcessing_Create();

    UNITY_INTERFACE_EXPORT void WebRtc_AudioProcessing_Destroy(HANDLE handle);

    UNITY_INTERFACE_EXPORT void WebRtc_AudioProcessing_ApplyConfig(HANDLE handle, bool echo_cancellation,
                                                                   bool use_mobile_software_aec,
                                                                   bool auto_gain_control,
                                                                   bool noise_suppression,
                                                                   bool highpass_filter,
                                                                   bool residual_echo_detector,
                                                                   bool typing_detection);

    UNITY_INTERFACE_EXPORT int WebRtc_test();


    UNITY_INTERFACE_EXPORT int
    WebRtc_AudioProcessing_ProcessStream(HANDLE handle, float *src, int sample_rate_hz, int num_channels,
                                         int frames);

    UNITY_INTERFACE_EXPORT int
    WebRtc_AudioProcessing_ProcessReverseStream(float *src, int sample_rate_hz, int channels, int samples);
}
}
#ifdef __cplusplus
}
#endif
#endif //AEC_APM_H
