#include "pch.h"

#include "Codec/H264ProfileLevelId.h"

namespace unity
{
namespace webrtc
{

    TEST(H264ProfileLevelId, TestSupportedLevel)
    {
        EXPECT_EQ(H264Level::kLevel2_1, *H264SupportedLevel(320 * 240, 25, 4000 * 1200));
        EXPECT_EQ(H264Level::kLevel3_1, *H264SupportedLevel(1280 * 720, 30, 14000 * 1200));
        EXPECT_EQ(H264Level::kLevel4_2, *H264SupportedLevel(1920 * 1080, 60, 50000 * 1200));
        EXPECT_EQ(H264Level::kLevel5_2, *H264SupportedLevel(3840 * 2160, 60, 50000 * 1200));
    }

    TEST(H264ProfileLevelId, TestSupportedLevelInvalid)
    {
        EXPECT_FALSE(H264SupportedLevel(0, 0, 0));
        EXPECT_FALSE(H264SupportedLevel(3840 * 2160, 90, 50000 * 1200));
    }

    TEST(H264ProfileLevelId, TestSupportedFramerate)
    {
        EXPECT_GE(SupportedMaxFramerate(H264Level::kLevel2_1, 320 * 240), 25);
        EXPECT_GE(SupportedMaxFramerate(H264Level::kLevel3_1, 1280 * 720), 30);
        EXPECT_GE(SupportedMaxFramerate(H264Level::kLevel4_2, 1920 * 1080), 60);
        EXPECT_GE(SupportedMaxFramerate(H264Level::kLevel5_2, 2560 * 1440), 90);
        EXPECT_GE(SupportedMaxFramerate(H264Level::kLevel5_2, 3840 * 2160), 60);
    }

    const char kProfileLevelId[] = "profile-level-id";

    TEST(H264ProfileLevelId, H264IsSameProfile)
    {
        SdpVideoFormat format1("H264");
        H264ProfileLevelId id1(H264Profile::kProfileBaseline, H264Level::kLevel4_1);
        format1.parameters.emplace(kProfileLevelId, H264ProfileLevelIdToString(id1).value());

        SdpVideoFormat format2("H264");
        H264ProfileLevelId id2(H264Profile::kProfileBaseline, H264Level::kLevel5_1);
        format2.parameters.emplace(kProfileLevelId, H264ProfileLevelIdToString(id2).value());

        // ignore level
        EXPECT_TRUE(H264IsSameProfile(format1.parameters, format2.parameters));

        SdpVideoFormat format3("H264");
        H264ProfileLevelId id3(H264Profile::kProfileHigh, H264Level::kLevel4_1);
        format3.parameters.emplace(kProfileLevelId, H264ProfileLevelIdToString(id3).value());

        // not ignore profile
        EXPECT_FALSE(H264IsSameProfile(format1.parameters, format3.parameters));
    }
}
}
