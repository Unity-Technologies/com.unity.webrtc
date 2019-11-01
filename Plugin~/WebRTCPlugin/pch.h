﻿#pragma once
#pragma region webRTC related
#include "api/mediastreaminterface.h"
#include "api/peerconnectioninterface.h"
#include "api/create_peerconnection_factory.h"
#include "api/audio_codecs/audio_decoder_factory_template.h"
#include "api/audio_codecs/audio_encoder_factory_template.h"
#include "api/audio_codecs/opus/audio_decoder_opus.h"
#include "api/audio_codecs/opus/audio_encoder_opus.h"
#include "api/test/fakeconstraints.h"
#include "api/video_codecs/video_decoder_factory.h"
#include "api/video_codecs/builtin_video_decoder_factory.h"
#include "api/video_codecs/video_encoder_factory.h"
#include "api/video_codecs/builtin_video_encoder_factory.h"
#include "api/video_codecs/video_encoder.h"
#include "api/video_codecs/sdp_video_format.h"
#include "api/video/video_frame.h"
#include "api/video/video_frame_buffer.h"
#include "api/video/i420_buffer.h"

#include "rtc_base/thread.h"
#include "rtc_base/refcountedobject.h"
#include "rtc_base/strings/json.h"
#include "rtc_base/logging.h"
#include "rtc_base/flags.h"
#include "rtc_base/checks.h"
#include "rtc_base/ssladapter.h"
#include "rtc_base/arraysize.h"
#include "rtc_base/nethelpers.h"
#include "rtc_base/stringutils.h"
#include "rtc_base/physicalsocketserver.h"
#include "rtc_base/signalthread.h"
#include "rtc_base/third_party/sigslot/sigslot.h"
#include "rtc_base/atomicops.h"
#include "rtc_base/asynctcpsocket.h"

#ifdef _WIN32
#include "rtc_base/win32.h"
#include "rtc_base/win32socketserver.h"
#include "rtc_base/win32socketinit.h"
#include "rtc_base/win32socketserver.h"
#endif

#include "media/base/videocapturer.h"
#include "media/engine/webrtcvideocapturerfactory.h"
#include "media/engine/internaldecoderfactory.h"
#include "media/base/h264_profile_level_id.h"
#include "media/engine/webrtcvideoencoderfactory.h"
#include "media/base/adaptedvideotracksource.h"
#include "media/base/mediachannel.h"
#include "media/base/videocommon.h"

#include "modules/video_capture/video_capture_factory.h"
#include "modules/audio_device/include/audio_device.h"
#include "modules/audio_device/audio_device_buffer.h"
#include "modules/audio_processing/include/audio_processing.h"
#include "modules/video_coding/codecs/h264/include/h264.h"

#include "common_video/h264/h264_bitstream_parser.h"
#include "common_video/h264/h264_common.h"

#include "media/base/videobroadcaster.h"
#pragma endregion

#include "PlatformBase.h"

#if SUPPORT_D3D11
#include "d3d11.h"
#endif // if SUPPORT_D3D11

#if SUPPORT_OPENGL_CORE
#include <GL/glew.h>
#endif

namespace WebRTC
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

#ifndef _WIN32
#define CoTaskMemAlloc(p) malloc(p)
#define CoTaskMemFree(p) free(p)
#endif

#if SUPPORT_OPENGL_CORE
    void OnOpenGLDebugMessage( GLenum source, GLenum type, GLuint id, GLenum severity, GLsizei length, const GLchar* message, const void* userParam);
#endif
    template<class ... Args>
    std::string StringFormat(const std::string& format, Args ... args)
    {
        size_t size = snprintf(nullptr, 0, format.c_str(), args ...) + 1;
        std::unique_ptr<char[]> buf(new char[size]);
        snprintf(buf.get(), size, format.c_str(), args ...);
        return std::string(buf.get(), buf.get() + size - 1);
    }

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

#if SUPPORT_D3D11
    using UnityFrameBuffer = ID3D11Texture2D;
    extern ID3D11DeviceContext* context;
    extern ID3D11Device* g_D3D11Device;
    extern UnityFrameBuffer* renderTextures[bufferedFrameNum];
#elif SUPPORT_OPENGL_CORE
    using UnityFrameBuffer = void;
    extern void* renderTextures[bufferedFrameNum];
#endif //if SUPPORT_D3D11
}
