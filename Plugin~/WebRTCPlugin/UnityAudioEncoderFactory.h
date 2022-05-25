#pragma once

#include <api/audio_codecs/audio_encoder_factory.h>
#include <api/scoped_refptr.h>

namespace unity
{
namespace webrtc
{
    using namespace ::webrtc;

    rtc::scoped_refptr<AudioEncoderFactory> CreateAudioEncoderFactory();

} // end namespace webrtc
} // end namespace unity
