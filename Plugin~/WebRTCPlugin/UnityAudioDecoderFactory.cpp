#include "pch.h"
#include "UnityAudioEncoderFactory.h"
#include <api/audio_codecs/opus/audio_decoder_multi_channel_opus.h>
#include <api/audio_codecs/L16/audio_decoder_L16.h>
#include <api/audio_codecs/g711/audio_decoder_g711.h>
#include <api/audio_codecs/g722/audio_decoder_g722.h>
#include <api/audio_codecs/ilbc/audio_decoder_ilbc.h>
#include <api/audio_codecs/isac/audio_decoder_isac.h>
#include <api/audio_codecs/opus/audio_decoder_opus.h>

using namespace ::webrtc;

namespace unity
{
namespace webrtc
{

rtc::scoped_refptr<AudioDecoderFactory>
    CreateAudioDecoderFactory() {
    return ::webrtc::CreateAudioDecoderFactory<
        AudioDecoderOpus,
        AudioDecoderMultiChannelOpus,
        AudioDecoderIlbc,
        AudioDecoderIsac,
        AudioDecoderG722,
        AudioDecoderG711,
        AudioDecoderL16
    >();
}

} // end namespace webrtc
} // end namespace unity
