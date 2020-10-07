cmake_minimum_required(VERSION 3.10)
enable_language(CXX)

project(webrtc)

set(CMAKE_VERBOSE_MAKEFILE ON)
set(CMAKE_CXX_FLAGS_DEBUG "-g3 -O0 -D_DEBUG")

if(MSVC)
  set(CMAKE_CXX_FLAGS_RELEASE "${CMAKE_CXX_FLAGS_RELEASE} /MT /Zi")
  set(CMAKE_CXX_FLAGS_DEBUG "${CMAKE_CXX_FLAGS_DEBUG} /MTd /Zi -D_ITERATOR_DEBUG_LEVEL=0")
endif()

set(CMAKE_THREAD_PREFER_PTHREAD TRUE)
find_package(Threads REQUIRED)

list(APPEND CMAKE_MODULE_PATH 
  "${CMAKE_SOURCE_DIR}/cmake"
  "${CMAKE_SOURCE_DIR}/NvCodec"
)

find_package(WebRTC REQUIRED)

if(APPLE)
  find_package(Framework REQUIRED)
else()
  find_package(CUDA REQUIRED)
  find_package(Vulkan REQUIRED)
  add_subdirectory(NvCodec)
endif()

include(AddPlugin)

enable_testing()
add_subdirectory(WebRTCPlugin)
add_subdirectory(WebRTCPluginTest)
