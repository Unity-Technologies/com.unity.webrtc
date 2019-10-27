#include "pch.h"
#include <cstdarg>
#include "WebRTCPlugin.h"

namespace WebRTC
{
    void LogPrint(const char* fmt, ...)
    {
#ifdef _DEBUG
        va_list vl;
        va_start(vl, fmt);
        char buf[2048];
#ifdef _WIN32
        vsprintf_s(buf, fmt, vl);
#else
        vsprintf(buf, fmt, vl);
#endif
        debugLog(buf);
        va_end(vl);
#endif
    }
    void checkf(bool result, const char* msg)
    {
        if (!result)
        {
            LogPrint(msg);
        }
    }
}
