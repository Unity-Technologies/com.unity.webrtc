#pragma once

#include <absl/types/optional.h>
#include <api/video_codecs/h265_profile_tier_level.h>
#include <api/video/resolution.h>

namespace unity
{
namespace webrtc
{
    using namespace ::webrtc;

    // Returns the minimum level which can support given parameters.
    absl::optional<H265Level> H265SupportedLevel(int maxFramePixelCount, int maxFramerate, int maxBitrate);

    // Returns the max framerate that calculated by maxFramePixelCount for H.265.
    int SupportedMaxFramerate(H265Level level, int maxFramePixelCount);

} // end namespace webrtc
} // end namespace unity