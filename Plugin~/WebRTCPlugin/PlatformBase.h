#pragma once

// Standard base includes, defines that indicate our current platform, etc.

#include <stddef.h>

#if defined(__APPLE__)
#include <TargetConditionals.h>
#endif

// Which platform we are on?
// UNITY_WIN - Windows (regular win32)
// UNITY_OSX - Mac OS X
// UNITY_LINUX - Linux
// UNITY_IOS - iOS
// UNITY_IOS_SIMULATOR - iOS Simulator
// UNITY_ANDROID - Android
// UNITY_METRO - WSA or UWP
// UNITY_WEBGL - WebGL
#if _MSC_VER
#define UNITY_WIN 1
#elif defined(__APPLE__)
#if TARGET_OS_IOS
#define UNITY_IOS 1
#elif TARGET_OS_SIMULATOR
#define UNITY_IOS_SIMULATOR 1
#elif TARGET_OS_OSX
#define UNITY_OSX 1
#endif
#elif defined(__ANDROID__)
#define UNITY_ANDROID 1
#elif defined(__linux__)
#define UNITY_LINUX 1
#elif defined(UNITY_METRO) || defined(UNITY_WEBGL)
    // these are defined externally
#elif defined(__EMSCRIPTEN__)
    // this is already defined in Unity 5.6
#define UNITY_WEBGL 1
#else
#error "Unknown platform!"
#endif



// Which graphics device APIs we possibly support?
#if UNITY_METRO
#define SUPPORT_D3D11 1
#if WINDOWS_UWP
#define SUPPORT_D3D12 1
#endif
#elif UNITY_WIN
#define SUPPORT_D3D11 1 // comment this out if you don't have D3D11 header/library files
#define SUPPORT_D3D12 1 // comment this out if you don't have D3D12 header/library files
#define SUPPORT_OPENGL_UNIFIED 1
#define SUPPORT_VULKAN 1 // Requires Vulkan SDK to be installed
#define SUPPORT_SOFTWARE_ENCODER 1
#elif UNITY_IOS || UNITY_ANDROID || UNITY_WEBGL
#ifndef SUPPORT_OPENGL_ES
#define SUPPORT_OPENGL_ES 1
#endif
#define SUPPORT_OPENGL_UNIFIED SUPPORT_OPENGL_ES
#elif UNITY_LINUX
#define SUPPORT_OPENGL_UNIFIED 1
#define SUPPORT_OPENGL_CORE 1
#define SUPPORT_VULKAN 1
#elif UNITY_OSX
#define SUPPORT_SOFTWARE_ENCODER 1
#endif

#if UNITY_IOS || UNITY_OSX  || UNITY_IOS_SIMULATOR
#define SUPPORT_METAL 1
#endif

// COM-like Release macro
#ifndef SAFE_RELEASE
#define SAFE_RELEASE(a) if (a) { a->Release(); a = NULL; }
#endif

#ifndef SAFE_CLOSE_HANDLE
#define SAFE_CLOSE_HANDLE(a) if (a) { CloseHandle(a); a = NULL; }
#endif
