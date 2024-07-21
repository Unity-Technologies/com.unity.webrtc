cmake_minimum_required(VERSION 3.22)
include(CMakePrintHelpers)

set(CMAKE_XCODE_ATTRIBUTE_IPHONEOS_DEPLOYMENT_TARGET
    "15.0"
    CACHE STRING "Minimum iOS deployment version")
set(CMAKE_OSX_DEPLOYMENT_TARGET
    "10.13"
    CACHE STRING "Minimum OS X deployment version")

project(
  webrtc
  VERSION 2.4.0
  LANGUAGES C CXX ASM)

cmake_policy(SET CMP0076 NEW)

# Enable unit testing
enable_testing()

# Define flags to determine the build target platform Windows, Linux, macOS,
# iOS, Android
if(CMAKE_SYSTEM_NAME STREQUAL "Windows")
  set(Windows TRUE)
endif()
if(CMAKE_SYSTEM_NAME STREQUAL "iOS")
  set(iOS TRUE)
endif()
if(CMAKE_SYSTEM_NAME STREQUAL "Linux")
  set(Linux TRUE)
endif()
if(CMAKE_SYSTEM_NAME STREQUAL "Darwin")
  set(macOS TRUE)
endif()
if(CMAKE_SYSTEM_NAME STREQUAL "Android")
  set(Android TRUE)
endif()

set(CMAKE_C_STANDARD 17)
set(CMAKE_CXX_STANDARD 17)
set(CMAKE_CXX_EXTENSIONS OFF)
set(CMAKE_C_STANDARD_REQUIRED ON)
set(CMAKE_CXX_STANDARD_REQUIRED ON)

if(XCODE)
  cmake_print_variables(XCODE_VERSION)
  if(${XCODE_VERSION} VERSION_GREATER_EQUAL 12.0.0 AND ${XCODE_VERSION}
                                                       VERSION_LESS 11.0.0)
    message(
      FATAL_ERROR
        "The required Xcode version is 11.0 or higher and 12.0 or higher is not supported."
    )
  endif()
endif()

# Use lld linker instead of GNU ld. Link glibc++ statically because of
# compatibility of lower version of GLIBCXX. This package must support Ubuntu
# version 16.04, 18.04, 20.04.
# disable for ubuntu 22.04
#if(Linux)
#  set(CMAKE_CXX_FLAG "${CMAKE_CXX_FLAG} -static-libstdc++")
#  add_link_options(-static-libstdc++ -fuse-ld=lld-11)
#endif()

# enable debug output
set(CMAKE_VERBOSE_MAKEFILE ON)

# set iterator debug level and compiler flags.
if(Windows AND CMAKE_CXX_COMPILER_ID STREQUAL "MSVC")
  set(CMAKE_CXX_FLAGS_DEBUG
      "${CMAKE_CXX_FLAGS_DEBUG} -D_ITERATOR_DEBUG_LEVEL=0 /Zi /Od")
else()
  set(CMAKE_CXX_FLAGS_DEBUG
      "${CMAKE_CXX_FLAGS_DEBUG} -D_ITERATOR_DEBUG_LEVEL=0")
endif()

set(CMAKE_THREAD_PREFER_PTHREAD TRUE)
find_package(Threads REQUIRED)

list(APPEND CMAKE_MODULE_PATH "${CMAKE_SOURCE_DIR}/cmake")

find_package(WebRTC REQUIRED)

if(iOS OR macOS)
  find_package(Framework REQUIRED)
endif()

if(Windows OR Linux)
  find_package(Vulkan REQUIRED)

  # FindCUDA is removed in Cmake 3.27
  cmake_policy(SET CMP0146 OLD)
  find_package(CUDA REQUIRED)
  find_package(CUDAToolkit REQUIRED)
  set(CMAKE_CUDA_ARCHITECTURES "native")
  set(CMAKE_CUDA_COMPILER ${CUDAToolkit_NVCC_EXECUTABLE})

  # Building CUDA kernel is not supported with Clang compiler on Windows.
  # https://gitlab.kitware.com/cmake/cmake/-/issues/20776
  if((Windows AND CMAKE_CXX_COMPILER_ID STREQUAL "MSVC") OR Linux)
    enable_language(CUDA)
  endif()
  add_subdirectory(NvCodec)
endif()

add_subdirectory(WebRTCPlugin)
add_subdirectory(WebRTCPluginTest)
