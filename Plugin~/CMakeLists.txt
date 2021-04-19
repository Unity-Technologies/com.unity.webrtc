cmake_minimum_required(VERSION 3.18)

# set default DEPLOYMENT_TARGET version for iOS platform
if(NOT CMAKE_OSX_DEPLOYMENT_TARGET)
  set(CMAKE_XCODE_ATTRIBUTE_IPHONEOS_DEPLOYMENT_TARGET 10.0)
  set(CMAKE_OSX_DEPLOYMENT_TARGET 10.10)
endif()

project(webrtc
  VERSION 2.4.0
  LANGUAGES C CXX ASM
)

cmake_policy(SET CMP0076 NEW)

# Enable unit testing
enable_testing()

# Define flags to determine the build target platform
# Windows, Linux, macOS, iOS
if(MSVC)
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

set(CMAKE_CXX_STANDARD 14)
set(CMAKE_CXX_STANDARD_REQUIRED ON)

if(XCODE)
 if(${XCODE_VERSION} VERSION_GREATER_EQUAL 12.0.0 AND
    ${XCODE_VERSION} VERSION_LESS 11.0.0
  )
    message(FATAL_ERROR 
      "The required Xcode version is 11.0 or higher and 12.0 or higher is not supported.")
  endif()
endif()

# enable debug output 
set(CMAKE_VERBOSE_MAKEFILE ON)
set(CMAKE_CXX_FLAGS_DEBUG "-g3 -O0 -D_DEBUG")

# set iterator debug level
if(Windows)
  set(CMAKE_CXX_FLAGS_DEBUG "${CMAKE_CXX_FLAGS_DEBUG} -D_ITERATOR_DEBUG_LEVEL=0 /Zi /Od")
endif()

set(CMAKE_THREAD_PREFER_PTHREAD TRUE)
find_package(Threads REQUIRED)

list(APPEND CMAKE_MODULE_PATH 
  "${CMAKE_SOURCE_DIR}/cmake"
)

find_package(WebRTC REQUIRED)

if(iOS OR macOS)
  find_package(Framework REQUIRED)
endif()

if(Windows OR Linux)
  find_package(CUDA REQUIRED)
  find_package(Vulkan REQUIRED)
  add_subdirectory(NvCodec)
endif()

add_subdirectory(WebRTCPlugin)
add_subdirectory(WebRTCPluginTest)