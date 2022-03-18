#pragma once
#pragma region webRTC related
#include "api/media_stream_interface.h"
#include "api/peer_connection_interface.h"
#include "api/create_peerconnection_factory.h"
#include "api/video_codecs/video_decoder_factory.h"
#include "api/video_codecs/builtin_video_decoder_factory.h"
#include "api/video_codecs/video_decoder.h"
#include "api/video_codecs/video_encoder_factory.h"
#include "api/video_codecs/builtin_video_encoder_factory.h"
#include "api/video_codecs/video_encoder.h"
#include "api/video_codecs/sdp_video_format.h"
#include "api/video/video_frame.h"
#include "api/video/video_frame_buffer.h"
#include "api/video/video_sink_interface.h"
#include "api/video/i420_buffer.h"
#include "api/video_track_source_proxy.h"

#include "rtc_base/thread.h"
#include "rtc_base/ref_counted_object.h"
#include "rtc_base/strings/json.h"
#include "rtc_base/logging.h"
#include "rtc_base/checks.h"
#include "rtc_base/ssl_adapter.h"
#include "rtc_base/arraysize.h"
#include "rtc_base/net_helpers.h"
#include "rtc_base/string_utils.h"
#include "rtc_base/physical_socket_server.h"
#include "rtc_base/third_party/sigslot/sigslot.h"
#include "rtc_base/atomic_ops.h"
#include "rtc_base/async_tcp_socket.h"

#ifdef _WIN32
#include "rtc_base/win32.h"
#include "rtc_base/win32_socket_server.h"
#include "rtc_base/win32_socket_init.h"
#include "rtc_base/win32_socket_server.h"
#endif

#include "media/engine/internal_encoder_factory.h"
#include "media/engine/internal_decoder_factory.h"
#include "media/base/h264_profile_level_id.h"
#include "media/base/adapted_video_track_source.h"
#include "media/base/media_channel.h"
#include "media/base/video_common.h"
#include "media/base/video_broadcaster.h"

#include "modules/video_capture/video_capture_impl.h"
#include "modules/video_capture/video_capture_factory.h"
#include "modules/video_coding/codecs/h264/include/h264.h"
#include "modules/video_coding/codecs/vp8/include/vp8.h"
#include "modules/video_coding/codecs/vp9/include/vp9.h"

#include "common_video/h264/h264_bitstream_parser.h"
#include "common_video/h264/h264_common.h"
#include "common_video/include/bitrate_adjuster.h"

#include "pc/media_stream_observer.h"
#include "pc/local_audio_source.h"

#pragma endregion

#include "PlatformBase.h"
#include "IUnityGraphics.h"
#include "IUnityRenderingExtensions.h"

#if SUPPORT_D3D11
#include <comdef.h>

#include "d3d11.h"
#include "IUnityGraphicsD3D11.h"
#endif

#if SUPPORT_D3D12
#include "d3d12.h"
#include "d3d11_4.h"
#include "IUnityGraphicsD3D12.h"
#endif

#if SUPPORT_OPENGL_CORE
#define GL_GLEXT_PROTOTYPES
#include <GL/gl.h>
#include <GL/glu.h>
#endif

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
#include "IUnityGraphicsVulkan.h"
#include "GraphicsDevice/Vulkan/LoadVulkanFunctions.h"

#endif

#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Wkeyword-macro"
#if _WIN32 && _DEBUG
#define _CRTDBG_MAP_ALLOC
#include <crtdbg.h>
#define new new(_NORMAL_BLOCK, __FILE__, __LINE__)
#endif
#pragma clang diagnostic pop

// audio codec isac
#define WEBRTC_USE_BUILTIN_ISAC_FLOAT 1

namespace unity
{
namespace webrtc
{

    void LogPrint(const char* fmt, ...);
    void LogPrint(const wchar_t* fmt, ...);
    void checkf(bool result, const char* msg);
#define DebugLog(...)       LogPrint("webrtc Log: " __VA_ARGS__)
#define DebugWarning(...)   LogPrint("webrtc Warning: " __VA_ARGS__)
#define DebugError(...)     LogPrint("webrtc Error: "  __VA_ARGS__)
#define DebugLogW(...)      LogPrint(L"webrtc Log: " __VA_ARGS__)
#define DebugWarningW(...)  LogPrint(L"webrtc Warning: " __VA_ARGS__)
#define DebugErrorW(...)    LogPrint(L"webrtc Error: "  __VA_ARGS__)
#define NV_RESULT(NvFunction) NvFunction == NV_ENC_SUCCESS

#if !UNITY_WIN
#define CoTaskMemAlloc(p) malloc(p)
#define CoTaskMemFree(p) free(p)
#endif

#if SUPPORT_OPENGL_CORE || SUPPORT_OPENGL_ES
    void OnOpenGLDebugMessage( GLenum source, GLenum type, GLuint id, GLenum severity, GLsizei length, const GLchar* message, const void* userParam);
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
