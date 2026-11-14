#include "pch.h"

#include "UnityLogStream.h"

namespace unity
{
namespace webrtc
{
    namespace
    {
        int firstLogCount;
        int secondLogCount;

        void FirstLog(const char*, rtc::LoggingSeverity) { ++firstLogCount; }
        void SecondLog(const char*, rtc::LoggingSeverity) { ++secondLogCount; }
    }

    TEST(UnityLogStreamTest, ReRegistrationRemovesThePreviousSink)
    {
        firstLogCount = 0;
        secondLogCount = 0;
        UnityLogStream::AddLogStream(FirstLog, rtc::LS_INFO);
        RTC_LOG(LS_INFO) << "first registration";
        EXPECT_EQ(firstLogCount, 1);

        UnityLogStream::AddLogStream(SecondLog, rtc::LS_INFO);
        RTC_LOG(LS_INFO) << "second registration";
        EXPECT_EQ(firstLogCount, 1);
        EXPECT_EQ(secondLogCount, 1);

        UnityLogStream::RemoveLogStream();
        RTC_LOG(LS_INFO) << "after removal";
        EXPECT_EQ(secondLogCount, 1);
    }
} // namespace webrtc
} // namespace unity
