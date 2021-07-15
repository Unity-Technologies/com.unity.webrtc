// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license
// information.

#include "pch.h"

#include "UnityLogStream.h"

namespace unity {
    namespace webrtc {

    std::unique_ptr<UnityLogStream> UnityLogStream::log_stream;

    void UnityLogStream::OnLogMessage(const std::string& message) {
        if (on_log_message != nullptr)
        {
            on_log_message(message.c_str());
        }
    }

    void UnityLogStream::AddLogStream(DelegateDebugLog callback)
    {
        rtc::LogMessage::LogTimestamps(true);
        log_stream.reset(new UnityLogStream(callback));
        rtc::LogMessage::AddLogToStream(log_stream.get(), rtc::INFO);
    }

    void UnityLogStream::RemoveLogStream()
    {
        if (log_stream) {
            rtc::LogMessage::RemoveLogToStream(log_stream.get());
            log_stream.reset();
        }
    }


}
} 

