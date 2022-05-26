#pragma once

#include <api/audio_codecs/audio_decoder_factory.h>
#include <api/scoped_refptr.h>

namespace unity
{
namespace webrtc
{
    using namespace ::webrtc;

    rtc::scoped_refptr<AudioDecoderFactory> CreateAudioDecoderFactory();
} // end namespace webrtc
} // end namespace unity
