#pragma once
#include <memory>

#include "api/video_codecs/video_decoder_factory.h"
#include "api/video_codecs/video_encoder_factory.h"

namespace unity
{
namespace webrtc
{
    using namespace ::webrtc;

    std::unique_ptr<VideoEncoderFactory> CreateAndroidEncoderFactory();
    std::unique_ptr<VideoDecoderFactory> CreateAndroidDecoderFactory();

} // namespace test
} // namespace webrtc