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

if(MSVC)
  set(SYSTEM_PROCESSOR ${CMAKE_GENERATOR_PLATFORM})
elseif(APPLE)
  set(SYSTEM_PROCESSOR x64)
elseif(UNIX)
  set(SYSTEM_PROCESSOR x64)
endif()

find_library(WEBRTC_LIBRARY_DEBUG
  NAMES webrtcd
  PATHS ${WEBRTC_LIBRARY_DIR}/${SYSTEM_PROCESSOR}
)

find_library(WEBRTC_LIBRARY_RELEASE
  NAMES webrtc
  PATHS ${WEBRTC_LIBRARY_DIR}/${SYSTEM_PROCESSOR}
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