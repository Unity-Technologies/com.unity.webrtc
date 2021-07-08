#include "pch.h"
#include "UnityAudioEncoderFactory.h"
#include <api/audio_codecs/opus/audio_encoder_multi_channel_opus.h>
#include <api/audio_codecs/L16/audio_encoder_L16.h>
#include <api/audio_codecs/g711/audio_encoder_g711.h>
#include <api/audio_codecs/g722/audio_encoder_g722.h>
#include <api/audio_codecs/ilbc/audio_encoder_ilbc.h>
#include <api/audio_codecs/isac/audio_encoder_isac.h>
#include <api/audio_codecs/opus/audio_encoder_opus.h>

using namespace ::webrtc;

namespace unity
{
namespace webrtc
{

rtc::scoped_refptr<AudioEncoderFactory>
    CreateAudioEncoderFactory() {
    return ::webrtc::CreateAudioEncoderFactory<
        AudioEncoderOpus,
        AudioEncoderMultiChannelOpus,
        AudioEncoderIlbc,
        AudioEncoderIsac,
        AudioEncoderG722,
        AudioEncoderG711,
        AudioEncoderL16
    >();
}

} // end namespace webrtc
} // end namespace unity
