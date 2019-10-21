cmake_minimum_required(VERSION 3.10)
enable_language(CXX)
set(CMAKE_CXX_STANDARD 14)
set(CMAKE_CXX_STANDARD_REQUIRED ON)
set(CMAKE_CXX_EXTENSIONS OFF)
find_package(Threads REQUIRED)

project(webrtc)

if(CMAKE_SYSTEM_NAME STREQUAL "Darwin")
  set(MACOS true)
endif()

if(MSVC)
  # Force to always compile with W4
  if(CMAKE_CXX_FLAGS MATCHES "/W[0-4]")
    string(REGEX REPLACE "/W[0-4]" "/W4" CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS}")
  else()
    set(CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS} /W4")
  endif()
elseif(APPLE OR UNIX)
  # Update if necessary
  set(CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS} -Wall -Wextra -Wno-long-long -pedantic")
endif()

if(MACOS)
  add_library(webrtc)
  set_target_properties(webrtc PROPERTIES BUNDLE ON)
  target_compile_definitions(webrtc PUBLIC WEBRTC_POSIX)
  target_compile_definitions(webrtc PUBLIC WEBRTC_MAC)
else()
  add_library(webrtc)
  set_target_properties(${name} PROPERTIES PREFIX "")
endif()

target_sources(webrtc PRIVATE
  Callback.cpp
  Context.cpp
  DataChannelObject.cpp
  DummyAudioDevice.cpp
  DummyVideoEncoder.cpp
  Logger.cpp
  NvEncoder.cpp
  PeerConnectionObject.cpp
  UnityEncoder.cpp
  UnityVideoCapturer.cpp
  Utils.cpp
  WebRTCPlugin.cpp
  pch.cpp)

find_library(libwebrtc
        NAMES libwebrtc jsoncpp audio_decoder_opus webrtc_opus
        PATH_SUFFIXES "a"
  )

target_include_directories(webrtc
        PRIVATE
        ../unity/include
        ../webrtc/include
        ../webrtc/include/third_party/abseil-cpp
        ../webrtc/include/third_party/jsoncpp/source/include
        ../webrtc/include/third_party/jsoncpp/generated
        )

target_link_libraries(webrtc
  PRIVATE libwebrtc
    ${JSONCPP_LIBRARY}
    ${AUDIO_DECODER_OPUS_LIBRARY}
    ${WEBRTC_OPS_LIBRARY}
)

install(TARGETS webrtc DESTINATION ${CMAKE_INSTALL_PREFIX}/com.unity.webrtc/Runtime/Plugins/x86_64)
