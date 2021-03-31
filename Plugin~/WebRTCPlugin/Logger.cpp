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

#if SUPPORT_OPENGL_CORE || SUPPORT_OPENGL_ES
    void OnOpenGLDebugMessage(
            GLenum source,
            GLenum type,
            GLuint id,
            GLenum severity,
            GLsizei length,
            const GLchar* message,
            const void* userParam)
    {

        std::string strSource = "";
        switch (source) {
            case GL_DEBUG_SOURCE_API:
                strSource = "API";
                break;

            case GL_DEBUG_SOURCE_WINDOW_SYSTEM:
                strSource = "WINDOW SYSTEM";
                break;

            case GL_DEBUG_SOURCE_SHADER_COMPILER:
                strSource = "SHADER COMPILER";
                break;

            case GL_DEBUG_SOURCE_THIRD_PARTY:
                strSource = "THIRD PARTY";
                break;

            case GL_DEBUG_SOURCE_APPLICATION:
                strSource = "APPLICATION";
                break;

            case GL_DEBUG_SOURCE_OTHER:
                strSource = "UNKNOWN";
                break;
        }

        std::string strErrorType = "";
        switch (type)
        {
            case GL_DEBUG_TYPE_ERROR:
                strErrorType = "ERROR";
                break;
            case GL_DEBUG_TYPE_DEPRECATED_BEHAVIOR:
                strErrorType = "DEPRECATED_BEHAVIOR";
                break;
            case GL_DEBUG_TYPE_UNDEFINED_BEHAVIOR:
                strErrorType = "UNDEFINED_BEHAVIOR";
                break;
            case GL_DEBUG_TYPE_PORTABILITY:
                strErrorType = "PORTABILITY";
                break;
            case GL_DEBUG_TYPE_PERFORMANCE:
                strErrorType = "PERFORMANCE";
                break;
            case GL_DEBUG_TYPE_OTHER:
                strErrorType = "OTHER";
                break;
        }

        std::string strSeverity = "";
        switch (severity){
        case GL_DEBUG_SEVERITY_LOW:
            strSeverity = "LOW";
        break;
        case GL_DEBUG_SEVERITY_MEDIUM:
            strSeverity = "MEDIUM";
        break;
        case GL_DEBUG_SEVERITY_HIGH:
            strSeverity = "HIGH";
        default:
            strSeverity = "UNKNOWN";
        break;
        }

        LogPrint("%d: %s severity:%s source:%s\n%s",
                id,
                strErrorType.c_str(),
                strSeverity.c_str(),
                strSource.c_str(),
                message);
    }
#endif

} // end namespace webrtc
} // end namespace unity
