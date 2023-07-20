#pragma once

#include "api/video_codecs/video_encoder_factory.h"
#include <memory>

namespace unity
{

namespace webrtc
{

    ::webrtc::VideoEncoderFactory*
    CreateSimulcastEncoderFactory(std::unique_ptr<::webrtc::VideoEncoderFactory> factory);

} // namespace webrtc
} // namespace unity
