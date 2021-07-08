#pragma once

#include "api/audio_codecs/audio_encoder_factory.h"

namespace unity
{
namespace webrtc
{
    rtc::scoped_refptr<::webrtc::AudioEncoderFactory> CreateAudioEncoderFactory();

} // end namespace webrtc
} // end namespace unity
