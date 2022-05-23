#include "pch.h"

#include "UnityLogStream.h"

namespace unity
{
namespace webrtc
{

    std::unique_ptr<UnityLogStream> UnityLogStream::log_stream;

    void UnityLogStream::OnLogMessage(const std::string& message)
    {
        if (on_log_message != nullptr)
        {
            on_log_message(message.c_str());
        }
    }

    void UnityLogStream::AddLogStream(DelegateDebugLog callback, rtc::LoggingSeverity loggingSeverity)
    {
        rtc::LogMessage::LogTimestamps(true);
        log_stream.reset(new UnityLogStream(callback));
        rtc::LogMessage::AddLogToStream(log_stream.get(), loggingSeverity);
    }

    void UnityLogStream::RemoveLogStream()
    {
        if (log_stream)
        {
            rtc::LogMessage::RemoveLogToStream(log_stream.get());
            log_stream.reset();
        }
    }

}
}
