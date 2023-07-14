#pragma once

#include <string>

#include "WebRTCPlugin.h"

namespace unity
{
namespace webrtc
{

    class UnityLogStream : public rtc::LogSink
    {
    public:
        UnityLogStream(DelegateDebugLog callback)
            : on_log_message(callback)
        {
        }

        // log format can be defined in this interface
        void OnLogMessage(const std::string& message) override;
        void OnLogMessage(const std::string& message, rtc::LoggingSeverity severity) override;

        static void AddLogStream(DelegateDebugLog callback, rtc::LoggingSeverity minLoggingSeverity);
        static void RemoveLogStream();

    private:
        DelegateDebugLog on_log_message;
        void logMessage(const std::string& message, rtc::LoggingSeverity severity);

        static std::unique_ptr<UnityLogStream> log_stream;
    };

}
}
