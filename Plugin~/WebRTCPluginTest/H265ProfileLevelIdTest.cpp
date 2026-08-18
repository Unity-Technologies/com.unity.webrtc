#include "pch.h"
#include <gtest/gtest.h>
#include "Codec/H265ProfileLevelId.h"
#include "api/video_codecs/h265_profile_tier_level.h"

namespace unity::webrtc::test
{

    TEST(H265ProfileLevelIdTest, SupportedMaxFramerate)
    {
        // Test basic functionality
        // 1920x1080 = 2073600 pixels
        // Level 4.0 should support up to ~30fps for 1920x1080
        int maxFps = SupportedMaxFramerate(H265Level::kLevel4, 1920 * 1080);
        EXPECT_GE(maxFps, 30);
        EXPECT_LE(maxFps, 60);

        // Level 5.1 should support higher framerate for same resolution
        int maxFps5_1 = SupportedMaxFramerate(H265Level::kLevel5_1, 1920 * 1080);
        EXPECT_GT(maxFps5_1, maxFps);

        // 4K resolution: 3840x2160 = 8294400 pixels
        int maxFps4k = SupportedMaxFramerate(H265Level::kLevel5_1, 3840 * 2160);
        EXPECT_GE(maxFps4k, 25);
        EXPECT_LE(maxFps4k, 35);

        // Level 6.2 should support 4K at higher framerate
        int maxFps4k6_2 = SupportedMaxFramerate(H265Level::kLevel6_2, 3840 * 2160);
        EXPECT_GT(maxFps4k6_2, maxFps4k);
    }

    TEST(H265ProfileLevelIdTest, H265SupportedLevel)
    {
        // Test 1920x1080@30fps@10Mbps - should be supported by Level 4.0 or higher
        auto level = H265SupportedLevel(1920 * 1080, 30, 10000);
        EXPECT_TRUE(level.has_value());
        EXPECT_GE(level.value(), H265Level::kLevel4);

        // Test 3840x2160@30fps@20Mbps - should be supported by Level 5.1 or higher
        auto level4k = H265SupportedLevel(3840 * 2160, 30, 20000);
        EXPECT_TRUE(level4k.has_value());
        EXPECT_GE(level4k.value(), H265Level::kLevel5_1);

        // Test very high resolution/framerate - should be supported by Level 6.2
        auto levelHigh = H265SupportedLevel(3840 * 2160, 60, 50000);
        EXPECT_TRUE(levelHigh.has_value());
        EXPECT_GE(levelHigh.value(), H265Level::kLevel6_1);

        // Test invalid inputs
        auto levelInvalid = H265SupportedLevel(0, 30, 10000);
        EXPECT_FALSE(levelInvalid.has_value());

        auto levelInvalid2 = H265SupportedLevel(1920 * 1080, 0, 10000);
        EXPECT_FALSE(levelInvalid2.has_value());

        auto levelInvalid3 = H265SupportedLevel(1920 * 1080, 30, 0);
        EXPECT_FALSE(levelInvalid3.has_value());
    }

} // namespace unity::webrtc::test