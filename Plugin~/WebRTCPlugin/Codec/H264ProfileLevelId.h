#pragma once

#include "absl/types/optional.h"
#include "api/video_codecs/h264_profile_level_id.h"

namespace unity
{
namespace webrtc
{
    using namespace ::webrtc;

    // Returns the minumum level which can supports given parameters.
    // webrtc::H264SupportedLevel function is defined in libwebrtc, but that is for decoder.
    absl::optional<H264Level> H264SupportedLevel(int maxFramePixelCount, int maxFramerate);

} // end namespace webrtc
} // end namespace unity
