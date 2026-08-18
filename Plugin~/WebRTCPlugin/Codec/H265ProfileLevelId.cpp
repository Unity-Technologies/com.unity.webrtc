#include "pch.h"
#include "H265ProfileLevelId.h"
#include "rtc_base/checks.h"
#include <rtc_base/arraysize.h>

namespace unity
{
namespace webrtc
{

    // H.265 Level constraints based on ITU-T H.265 Table A-6
    // max_luma_ps: Max number of luma samples per second (maxLumaSr)
    // max_luma_ps: Max number of luma samples per picture (maxLumaPs)
    // max_br: Max bitrate (in kbps)
    struct H265LevelConstraint {
        uint32_t max_luma_sr;        // Max luma samples per second
        uint32_t max_luma_ps;        // Max luma samples per picture
        uint32_t max_br_main;        // Max bitrate for Main tier (in kbps)
        uint32_t max_br_high;        // Max bitrate for High tier (in kbps)
        H265Level level;
    };

    // H.265 level constraints from ITU-T H.265 Table A-6 and A-7
    // Values are based on the H.265 standard specifications
    static constexpr H265LevelConstraint kH265LevelConstraints[] = {
        // Level 1.0
        { 552960, 36864, 128, 64, H265Level::kLevel1 },
        // Level 2.0
        { 3686400, 122880, 1500, 750, H265Level::kLevel2 },
        // Level 2.1
        { 7372800, 245760, 3000, 1500, H265Level::kLevel2_1 },
        // Level 3.0
        { 16588800, 552960, 6000, 3000, H265Level::kLevel3 },
        // Level 3.1
        { 33177600, 1228800, 10000, 5000, H265Level::kLevel3_1 },
        // Level 4.0
        { 62914560, 2097152, 12000, 6000, H265Level::kLevel4 },
        // Level 4.1
        { 62914560, 2097152, 20000, 10000, H265Level::kLevel4_1 },
        // Level 5.0
        { 116640000, 4194304, 25000, 12500, H265Level::kLevel5 },
        // Level 5.1
        { 233280000, 8388608, 40000, 20000, H265Level::kLevel5_1 },
        // Level 5.2
        { 466560000, 16777216, 60000, 30000, H265Level::kLevel5_2 },
        // Level 6.0
        { 833972800, 33554432, 60000, 30000, H265Level::kLevel6 },
        // Level 6.1
        { 1667942400, 67108864, 120000, 60000, H265Level::kLevel6_1 },
        // Level 6.2
        { 3349708800, 134217728, 240000, 120000, H265Level::kLevel6_2 }
    };

    absl::optional<H265Level> H265SupportedLevel(int maxFramePixelCount, int maxFramerate, int maxBitrate)
    {
        if (maxFramePixelCount <= 0 || maxFramerate <= 0 || maxBitrate <= 0)
            return absl::nullopt;

        // Calculate samples per second required
        uint32_t requiredSamplesPerSec = static_cast<uint32_t>(maxFramePixelCount) * static_cast<uint32_t>(maxFramerate);

        for (size_t i = 0; i < arraysize(kH265LevelConstraints); i++)
        {
            const H265LevelConstraint& constraint = kH265LevelConstraints[i];

            // Check if this level can support the required parameters
            // Use Main tier constraints by default (more restrictive)
            if (constraint.max_luma_ps >= static_cast<uint32_t>(maxFramePixelCount) &&
                constraint.max_luma_sr >= requiredSamplesPerSec &&
                constraint.max_br_main >= static_cast<uint32_t>(maxBitrate))
            {
                return constraint.level;
            }
        }

        // No level supported
        return absl::nullopt;
    }

    int SupportedMaxFramerate(H265Level level, int maxFramePixelCount)
    {
        if (maxFramePixelCount <= 0)
            return 0;

        for (size_t i = 0; i < arraysize(kH265LevelConstraints); i++)
        {
            const H265LevelConstraint& constraint = kH265LevelConstraints[i];
            if (constraint.level == level)
            {
                // Calculate max framerate based on max luma samples per second
                // max_framerate = max_luma_sr / maxFramePixelCount
                return static_cast<int>(constraint.max_luma_sr / maxFramePixelCount);
            }
        }

        // Level not found
        return 0;
    }

} // end namespace webrtc
} // end namespace unity
