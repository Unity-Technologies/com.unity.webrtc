#pragma once

#include <absl/types/optional.h>
#include <api/video_codecs/h264_profile_level_id.h>

namespace unity
{
namespace webrtc
{
    using namespace ::webrtc;

    // Returns the minumum level which can supports given parameters.
    // webrtc::H264SupportedLevel function is defined in libwebrtc, but that is for decoder.
    absl::optional<H264Level> H264SupportedLevel(int maxFrameWidthPixelCount, int maxFrameHeightPixelCount, int maxFramerate, int maxBitrate);

    // Returns the max framerate that calclated by maxFrameWidthPixelCount and maxFrameHeightPixelCount.
    int SupportedMaxFramerate(H264Level level, int maxFrameWidthPixelCount, int maxFrameHeightPixelCount);

} // end namespace webrtc
} // end namespace unity
