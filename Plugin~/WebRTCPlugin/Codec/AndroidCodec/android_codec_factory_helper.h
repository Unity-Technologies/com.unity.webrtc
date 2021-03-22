#pragma once
#include <memory>

#include "api/video_codecs/video_decoder_factory.h"
#include "api/video_codecs/video_encoder_factory.h"

using namespace ::webrtc;

namespace unity {
namespace webrtc {

std::unique_ptr<VideoEncoderFactory> CreateAndroidEncoderFactory();
std::unique_ptr<VideoDecoderFactory> CreateAndroidDecoderFactory();

}  // namespace test
}  // namespace webrtc