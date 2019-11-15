cmake_minimum_required(VERSION 3.10)
enable_language(CXX)

project(webrtc)

set(CMAKE_VERBOSE_MAKEFILE ON)
set(CMAKE_CXX_FLAGS_DEBUG "-g3 -O0 -pg -D_DEBUG")

set(CMAKE_THREAD_PREFER_PTHREAD TRUE)
find_package(Threads REQUIRED)

list(APPEND CMAKE_MODULE_PATH "${CMAKE_SOURCE_DIR}/cmake")
find_package(WebRTC REQUIRED)
find_package(GLEW2.1 REQUIRED)
if(APPLE)
  find_package(Framework REQUIRED)
elseif(UNIX)
  find_package(LibCXX REQUIRED)
endif()
include(AddPlugin)

add_subdirectory(WebRTCPlugin)