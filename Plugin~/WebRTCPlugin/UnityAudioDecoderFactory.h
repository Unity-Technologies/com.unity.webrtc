#pragma once

#include "api/audio_codecs/audio_decoder_factory.h"

namespace unity
{
namespace webrtc
{
    rtc::scoped_refptr<::webrtc::AudioDecoderFactory> CreateAudioDecoderFactory();
} // end namespace webrtc
} // end namespace unity
