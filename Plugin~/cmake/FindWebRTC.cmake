# Find WebRTC include path

set(WEBRTC_DIR "${CMAKE_SOURCE_DIR}/webrtc")

set(WEBRTC_INCLUDE_DIR
  ${WEBRTC_DIR}/include
  ${WEBRTC_DIR}/include/third_party/abseil-cpp
  ${WEBRTC_DIR}/include/third_party/jsoncpp/source/include
  ${WEBRTC_DIR}/include/third_party/jsoncpp/generated
  ${WEBRTC_DIR}/include/third_party/libyuv/include
)

set(WEBRTC_OBJC_INCLUDE_DIR
  ${WEBRTC_DIR}/include/sdk/objc
  ${WEBRTC_DIR}/include/sdk/objc/base
)

set(WEBRTC_LIBRARY_DIR
  ${WEBRTC_DIR}/lib
)

# There is only `x64` on Windows, macOS, and Linux
# iOS and macOS use universal binary contains `x64` and `arm64`
if(Windows OR Linux)
  set(SYSTEM_PROCESSOR x64)
  set(WEBRTC_LIBRARY_DIR ${WEBRTC_LIBRARY_DIR}/${SYSTEM_PROCESSOR})
endif()
if(Android)
  if(CMAKE_ANDROID_ARCH_ABI STREQUAL "x86_64")
    set(CMAKE_ANDROID_ARCH "x64")
  endif()
  set(WEBRTC_LIBRARY_DIR ${WEBRTC_LIBRARY_DIR}/${CMAKE_ANDROID_ARCH})
endif()

find_library(WEBRTC_LIBRARY_DEBUG
  NAMES webrtcd
  PATHS ${WEBRTC_LIBRARY_DIR}
  NO_CMAKE_FIND_ROOT_PATH
)

find_library(WEBRTC_LIBRARY_RELEASE
  NAMES webrtc
  PATHS ${WEBRTC_LIBRARY_DIR}
  NO_CMAKE_FIND_ROOT_PATH
)

set(WEBRTC_LIBRARY
  debug ${WEBRTC_LIBRARY_DEBUG} 
  optimized ${WEBRTC_LIBRARY_RELEASE}
  CACHE STRING "WebRTC library"
)

include(FindPackageHandleStandardArgs)
find_package_handle_standard_args(WebRTC 
  DEFAULT_MSG 
  WEBRTC_LIBRARY
  WEBRTC_LIBRARY_DEBUG
  WEBRTC_LIBRARY_RELEASE 
  WEBRTC_INCLUDE_DIR
)
