add_library(WebRTCLib STATIC)

target_sources(WebRTCLib
  PRIVATE
    Context.cpp
    Context.h
    DataChannelObject.cpp
    DataChannelObject.h
    DummyAudioDevice.cpp
    DummyAudioDevice.h
    DummyVideoEncoder.cpp
    DummyVideoEncoder.h
    AudioTrackSinkAdapter.h
    AudioTrackSinkAdapter.cpp
    Logger.cpp
    MediaStreamObserver.cpp
    MediaStreamObserver.h
    pch.cpp
    pch.h
    PeerConnectionObject.cpp
    PeerConnectionObject.h
    PeerConnectionStatsCollectorCallback.cpp
    PeerConnectionStatsCollectorCallback.h
    PlatformBase.h
    SetSessionDescriptionObserver.cpp
    SetSessionDescriptionObserver.h
    ScopedProfiler.h
    ScopedProfiler.cpp
    targetver.h
    UnityVideoDecoderFactory.cpp
    UnityVideoDecoderFactory.h
    UnityVideoEncoderFactory.cpp
    UnityVideoEncoderFactory.h
    UnityVideoRenderer.cpp
    UnityVideoRenderer.h
    UnityVideoTrackSource.cpp
    UnityVideoTrackSource.h
    UnityAudioDecoderFactory.cpp
    UnityAudioDecoderFactory.h
    UnityAudioEncoderFactory.cpp
    UnityAudioEncoderFactory.h
    UnityAudioTrackSource.cpp
    UnityAudioTrackSource.h
    WebRTCConstants.h
    WebRTCMacros.h
    WebRTCPlugin.h
)

if(iOS OR macOS)
  target_sources(WebRTCLib
    PRIVATE
      ../libcxx/debug.cpp
  )
endif()

add_subdirectory(Codec)
add_subdirectory(GraphicsDevice)

if(Linux)
  add_subdirectory(Dynlink)
endif()

if(Windows)
  # Force to always compile with W4
  if(CMAKE_CXX_FLAGS MATCHES "/W[0-4]")
    string(REGEX REPLACE "/W[0-4]" "/W4" CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS}")
  else()
    set(CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS} /W4")
  endif()
  # Replace /GR option to disable RTTI
  foreach(flag_var CMAKE_CXX_FLAGS CMAKE_CXX_FLAGS_DEBUG CMAKE_CXX_FLAGS_RELEASE
                   CMAKE_CXX_FLAGS_MINSIZEREL CMAKE_CXX_FLAGS_RELWITHDEBINFO)
    string(REPLACE /GR /GR- "${flag_var}" "${${flag_var}}")
  endforeach()
endif()

if(UNIX)
  target_compile_options(WebRTCLib 
    PUBLIC
      -Wall -Wextra -Wno-unknown-pragmas -Wno-unused-parameter -Wno-missing-field-initializers -Wno-long-long
  )
endif()

if(Windows)
  # Use precompiled header
  set(CMAKE_CXX_FLAGS 
    "${CMAKE_CXX_FLAGS} /Yu /Yupch.h")
  set(PROJECT_BINARY_DIR
    "${CMAKE_SOURCE_DIR}/../Runtime/Plugins/x86_64")

  target_compile_definitions(WebRTCLib
    PUBLIC
      WEBRTC_WIN
      NOMINMAX
      VK_NO_PROTOTYPES
  )
  target_link_libraries(WebRTCLib
    PRIVATE
      ${WEBRTC_LIBRARY}
      ${Vulkan_LIBRARY}
      ${CUDA_CUDA_LIBRARY}
      ${NVCODEC_LIBRARIES}
      d3d11
      winmm
      Secur32
      Msdmo
      Dmoguids
      wmcodecdspuuid
      Strmiids
  )
  target_include_directories(WebRTCLib
    PUBLIC
      ${CUDA_INCLUDE_DIRS}
      ${Vulkan_INCLUDE_DIR}
      ${NVCODEC_INCLUDE_DIR}
  )

  # Select runtime library (MT, MTD) on windows platform
  set_target_properties(WebRTCLib
    PROPERTIES MSVC_RUNTIME_LIBRARY "MultiThreaded$<$<CONFIG:Debug>:Debug>"
  )

  # Creare precompiled header
  set_source_files_properties(pch.cpp 
    PROPERTIES 
      COMPILE_FLAGS "/Yc /Ycpch.h"
  )
elseif(macOS)
  set(CMAKE_C_FLAGS "${CMAKE_C_FLAGS} -x objective-c")
  set(CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS} -fno-lto -fno-rtti")
  set(CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS} -x objective-c++")
  set(PROJECT_BINARY_DIR
    "${CMAKE_SOURCE_DIR}/../Runtime/Plugins/macOS")

  target_compile_definitions(WebRTCLib
    PUBLIC
      WEBRTC_MAC
      WEBRTC_POSIX
  )
  target_link_libraries(WebRTCLib
    PRIVATE
      ${WEBRTC_LIBRARY}
      ${FRAMEWORK_LIBS}
  )
  target_include_directories(WebRTCLib
    PUBLIC
      ${WEBRTC_OBJC_INCLUDE_DIR}
  )
  set_target_properties(WebRTCLib
    PROPERTIES 
      LINK_FLAGS "-ObjC"
  )  
elseif(Linux)
  set(PROJECT_BINARY_DIR
    "${CMAKE_SOURCE_DIR}/../Runtime/Plugins/x86_64")

  find_package(PkgConfig REQUIRED)
  pkg_check_modules(GLIB REQUIRED glib-2.0)

  target_compile_definitions(WebRTCLib
    PUBLIC
      WEBRTC_LINUX
      WEBRTC_POSIX
      VK_NO_PROTOTYPES
  )
  target_link_libraries(WebRTCLib
    PRIVATE
      ${WEBRTC_LIBRARY}
      ${OPENGL_opengl_LIBRARY}
      ${Vulkan_LIBRARY}
      ${GLIB_LIBRARIES}
  )
  target_include_directories(WebRTCLib
    PUBLIC
      ${CUDA_INCLUDE_DIRS}
      ${Vulkan_INCLUDE_DIR}
      ${OPENGL_INCLUDE_DIR}
      ${NVCODEC_INCLUDE_DIR}
  )

  # enable -fPIC option
  set_target_properties(WebRTCLib
    PROPERTIES
      POSITION_INDEPENDENT_CODE ON
  )
elseif(iOS)
  set(CMAKE_C_FLAGS "${CMAKE_C_FLAGS} -x objective-c -fembed-bitcode")
  set(CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS} -fno-lto -fno-rtti -fembed-bitcode")
  set(CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS} -x objective-c++")
  set(PROJECT_BINARY_DIR
    "${CMAKE_SOURCE_DIR}/../Runtime/Plugins/iOS"
  )

  target_compile_definitions(WebRTCLib
    PUBLIC
      WEBRTC_IOS
      WEBRTC_MAC
      WEBRTC_POSIX
  )
  target_include_directories(WebRTCLib
    PUBLIC
      ${WEBRTC_OBJC_INCLUDE_DIR}
  )
  target_link_libraries(WebRTCLib
    PRIVATE
      ${WEBRTC_LIBRARY}
      ${FRAMEWORK_LIBS}
  )
  set_target_properties(WebRTCLib 
    PROPERTIES 
      LINK_FLAGS "-ObjC"
  )
elseif(Android)
  target_compile_definitions(WebRTCLib
    PUBLIC
      WEBRTC_ANDROID
      WEBRTC_POSIX
      VK_NO_PROTOTYPES
  )
  target_link_libraries(WebRTCLib
    PRIVATE
      ${WEBRTC_LIBRARY}
      log
      OpenSLES
      GLESv3
      EGL
  )
  target_compile_options(WebRTCLib
    PUBLIC 
      -fno-lto -fno-rtti
  )

  # Linker excludes unused symbols from the shared library 
  # when linking the archive file (libwebrtc.a).
  file(READ "${CMAKE_SOURCE_DIR}/tools/android/jni_symbols.txt" JNI_SYMBOLS)
  string(REPLACE "\n" ";" JNI_SYMBOLS ${JNI_SYMBOLS})
  list(TRANSFORM JNI_SYMBOLS PREPEND "-Wl,--require-defined=")

  target_link_options(WebRTCLib
    PUBLIC
      ${JNI_SYMBOLS}
  )

  set(PROJECT_BINARY_DIR
    "${CMAKE_SOURCE_DIR}/../Runtime/Plugins/Android"
  )
endif()

target_include_directories(WebRTCLib
  PUBLIC
    .
    ${CMAKE_SOURCE_DIR}/unity/include
    ${WEBRTC_INCLUDE_DIR}
)

#
# Configure WebRTC Unity plugin
#

if(iOS)
  add_library(WebRTCPlugin SHARED)
  set_target_properties(WebRTCPlugin 
    PROPERTIES 
      FRAMEWORK TRUE
  )
elseif(macOS)
  add_library(WebRTCPlugin MODULE)
  set_target_properties(WebRTCPlugin 
    PROPERTIES
      BUNDLE TRUE
      BUNDLE_EXTENSION bundle
  )
else()
  add_library(WebRTCPlugin SHARED)
endif()

target_link_libraries(WebRTCPlugin
  PRIVATE
    WebRTCLib
)
target_sources(WebRTCPlugin 
  PRIVATE
    pch.cpp
    pch.h
    WebRTCPlugin.cpp
    UnityRenderEvent.cpp
)

if(Android)
  target_sources(WebRTCPlugin
    PRIVATE
      Jni.cpp
  )
endif()

# rename dll/framework filename
set_target_properties(WebRTCPlugin
  PROPERTIES
    OUTPUT_NAME webrtc
    RUNTIME_OUTPUT_DIRECTORY_RELEASE ${PROJECT_BINARY_DIR}
    RUNTIME_OUTPUT_DIRECTORY_DEBUG ${PROJECT_BINARY_DIR}
    LIBRARY_OUTPUT_DIRECTORY_RELEASE ${PROJECT_BINARY_DIR}
    LIBRARY_OUTPUT_DIRECTORY_DEBUG ${PROJECT_BINARY_DIR}
)

if(Windows)
  # Set linker option
  set_target_properties(WebRTCPlugin
    PROPERTIES 
      LINK_FLAGS "/SUBSYSTEM:WINDOWS /DELAYLOAD:nvcuda.dll /DELAYLOAD:vulkan-1.dll"
      MSVC_RUNTIME_LIBRARY "MultiThreaded$<$<CONFIG:Debug>:Debug>"
  )
endif()

if(macOS)
  set_target_properties(WebRTCPlugin
  PROPERTIES 
    LINK_FLAGS "-ObjC"
  )
endif()

if(iOS)
  #
  # Build Setting Reference
  # https://developer.apple.com/library/archive/documentation/DeveloperTools/Reference/XcodeBuildSettingRef/1-Build_Setting_Reference/build_setting_ref.html
  #
  set_target_properties(WebRTCPlugin
  PROPERTIES 
    LINK_FLAGS "-ObjC"
    MACOSX_FRAMEWORK_IDENTIFIER "com.unity.webrtc"
    MACOSX_FRAMEWORK_BUNDLE_VERSION ${CMAKE_PROJECT_VERSION}
    MACOSX_FRAMEWORK_SHORT_VERSION_STRING ${CMAKE_PROJECT_VERSION}
    XCODE_ATTRIBUTE_SKIP_INSTALL "NO"
    XCODE_ATTRIBUTE_INSTALL_PATH "@rpath"
    XCODE_ATTRIBUTE_STRIP_STYLE non-global
    XCODE_ATTRIBUTE_STRIP_INSTALLED_PRODUCT $<IF:$<CONFIG:Debug>,NO,YES>
    XCODE_ATTRIBUTE_GCC_SYMBOLS_PRIVATE_EXTERN $<IF:$<CONFIG:Debug>,NO,YES>
  )
endif()