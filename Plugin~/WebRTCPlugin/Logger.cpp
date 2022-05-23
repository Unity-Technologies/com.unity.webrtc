#include "pch.h"

#include "WebRTCPlugin.h"

#if _DEBUG
#include <cstdarg>
#endif

namespace unity
{
namespace webrtc
{
    DelegateDebugLog delegateDebugLog = nullptr;

    void debugLog(const char* buf)
    {
        if (delegateDebugLog != nullptr)
        {
            delegateDebugLog(buf);
        }
    }

    void LogPrint(const char* fmt, ...)
    {
#if _DEBUG
        va_list vl;
        va_start(vl, fmt);
        char buf[2048];
#if _WIN32
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
            throw std::runtime_error(msg);
        }
    }

} // end namespace webrtc
} // end namespace unity
