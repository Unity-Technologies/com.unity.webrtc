#pragma once

#pragma region webRTC related

#include "rtc_base/logging.h"

#ifdef _WIN32
#include "rtc_base/win32.h"
#include "rtc_base/win32_socket_init.h"
#include "rtc_base/win32_socket_server.h"
#endif

#pragma endregion

#include "PlatformBase.h"

// Android platform
#if SUPPORT_OPENGL_ES
#include <GLES/gl.h>
#include <GLES/glext.h>
#include <GLES3/gl32.h>
#include <GLES3/gl3ext.h>
#endif

#if SUPPORT_METAL
#include "IUnityGraphicsMetal.h"
#endif

#if SUPPORT_VULKAN
#include "GraphicsDevice/Vulkan/LoadVulkanFunctions.h"

#endif

// #pragma clang diagnostic push
// #pragma clang diagnostic ignored "-Wkeyword-macro"
// #if _WIN32 && _DEBUG
// #define _CRTDBG_MAP_ALLOC
// #include <crtdbg.h>
// #define new new (_NORMAL_BLOCK, __FILE__, __LINE__)
// #endif
// #pragma clang diagnostic pop

// audio codec isac
#define WEBRTC_USE_BUILTIN_ISAC_FLOAT 1

namespace unity
{
namespace webrtc
{

    void LogPrint(const char* fmt, ...);
    void LogPrint(const wchar_t* fmt, ...);
    void checkf(bool result, const char* msg);
#define DebugLog(...) LogPrint("webrtc Log: " __VA_ARGS__)
#define DebugWarning(...) LogPrint("webrtc Warning: " __VA_ARGS__)
#define DebugError(...) LogPrint("webrtc Error: " __VA_ARGS__)
#define DebugLogW(...) LogPrint(L"webrtc Log: " __VA_ARGS__)
#define DebugWarningW(...) LogPrint(L"webrtc Warning: " __VA_ARGS__)
#define DebugErrorW(...) LogPrint(L"webrtc Error: " __VA_ARGS__)
#define NV_RESULT(NvFunction) NvFunction == NV_ENC_SUCCESS

#if !UNITY_WIN
#define CoTaskMemAlloc(p) malloc(p)
#define CoTaskMemFree(p) free(p)
#endif

    using byte = unsigned char;
    using uint8 = unsigned char;
    using uint16 = unsigned short int;
    using uint32 = unsigned int;
    using uint64 = unsigned long long;
    using int8 = signed char;
    using int16 = signed short int;
    using int32 = signed int;
    using int64 = signed long long;

    const uint32 bufferedFrameNum = 3;
} // end namespace webrtc
} // end namespace unity
