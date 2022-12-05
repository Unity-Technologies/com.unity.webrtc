#include "pch.h"

#include <rtc_base/arraysize.h>

#include "H264ProfileLevelId.h"

using namespace ::webrtc;

namespace unity
{
namespace webrtc
{
    struct LevelConstraint
    {
        const int max_macroblocks_per_second;
        const int max_macroblock_frame_size;
        const int max_video_bitrate;
        const H264Level level;
    };

    static constexpr LevelConstraint kLevelConstraints[] = {
        { 1485, 99, 64, H264Level::kLevel1 },
        { 1485, 99, 128, H264Level::kLevel1_b },
        { 3000, 396, 192, H264Level::kLevel1_1 },
        { 6000, 396, 384, H264Level::kLevel1_2 },
        { 11880, 396, 768, H264Level::kLevel1_3 },
        { 11880, 396, 2000, H264Level::kLevel2 },
        { 19800, 792, 4000, H264Level::kLevel2_1 },
        { 20250, 1620, 4000, H264Level::kLevel2_2 },
        { 40500, 1620, 10000, H264Level::kLevel3 },
        { 108000, 3600, 14000, H264Level::kLevel3_1 },
        { 216000, 5120, 20000, H264Level::kLevel3_2 },
        { 245760, 8192, 20000, H264Level::kLevel4 },
        { 245760, 8192, 50000, H264Level::kLevel4_1 },
        { 522240, 8704, 50000, H264Level::kLevel4_2 },
        { 589824, 22080, 135000, H264Level::kLevel5 },
        { 983040, 36864, 240000, H264Level::kLevel5_1 },
        { 2073600, 36864, 240000, H264Level::kLevel5_2 },
    };

    static const int kPixelsPerMacroblock = 16 * 16;
    static const int kUnitMaxBRWithNAL = 1200;

    absl::optional<webrtc::H264Level> H264SupportedLevel(int maxFramePixelCount, int maxFramerate, int maxBitrate)
    {
        if (maxFramePixelCount <= 0 || maxFramerate <= 0 || maxBitrate <= 0)
            return absl::nullopt;

        for (size_t i = 0; i < arraysize(kLevelConstraints); i++)
        {
            const LevelConstraint& level_constraint = kLevelConstraints[i];
            if (level_constraint.max_macroblock_frame_size * kPixelsPerMacroblock >= maxFramePixelCount &&
                level_constraint.max_macroblocks_per_second >=
                    maxFramerate * maxFramePixelCount / kPixelsPerMacroblock &&
                level_constraint.max_video_bitrate * kUnitMaxBRWithNAL >= maxBitrate)
            {
                return level_constraint.level;
            }
        }

        // No level supported.
        return absl::nullopt;
    }

    int SupportedMaxFramerate(H264Level level, int maxFramePixelCount)
    {
        for (size_t i = 0; i < arraysize(kLevelConstraints); i++)
        {
            const LevelConstraint& level_constraint = kLevelConstraints[i];
            if (level_constraint.level == level)
            {
                return level_constraint.max_macroblocks_per_second * kPixelsPerMacroblock / maxFramePixelCount;
            }
        }

        // target level not found.
        return 0;
    }
}
}
