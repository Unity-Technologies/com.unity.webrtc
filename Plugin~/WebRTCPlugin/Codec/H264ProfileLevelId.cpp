#include "pch.h"
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
        const H264Level level;
    };

    static constexpr LevelConstraint kLevelConstraints[] =
    {
        { 1485, 99, H264Level::kLevel1 },
        { 1485, 99, H264Level::kLevel1_b },
        { 3000, 396, H264Level::kLevel1_1 },
        { 6000, 396, H264Level::kLevel1_2 },
        { 11880, 396, H264Level::kLevel1_3 },
        { 11880, 396, H264Level::kLevel2 },
        { 19800, 792, H264Level::kLevel2_1 },
        { 20250, 1620, H264Level::kLevel2_2 },
        { 40500, 1620, H264Level::kLevel3 },
        { 108000, 3600, H264Level::kLevel3_1 },
        { 216000, 5120, H264Level::kLevel3_2 },
        { 245760, 8192, H264Level::kLevel4 },
        { 245760, 8192, H264Level::kLevel4_1 },
        { 522240, 8704, H264Level::kLevel4_2 },
        { 589824, 22080, H264Level::kLevel5 },
        { 983040, 36864, H264Level::kLevel5_1 },
        { 2073600, 36864, H264Level::kLevel5_2 },
    };

    absl::optional<webrtc::H264Level> H264SupportedLevel(int maxFramePixelCount, int maxFramerate)
    {
        static const int kPixelsPerMacroblock = 16 * 16;

        for (size_t i = 0; i < arraysize(kLevelConstraints); i++)
        {
            const LevelConstraint& level_constraint = kLevelConstraints[i];
            if (level_constraint.max_macroblock_frame_size * kPixelsPerMacroblock >= maxFramePixelCount &&
                level_constraint.max_macroblocks_per_second >=
                    maxFramerate * level_constraint.max_macroblock_frame_size)
            {
                return level_constraint.level;
            }
        }

        // No level supported.
        return absl::nullopt;
    }
}
}
