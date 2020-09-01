# Find WebRTC include path

set(WEBRTC_DIR "${CMAKE_SOURCE_DIR}/webrtc")

set(WEBRTC_INCLUDE_DIR
    ${WEBRTC_DIR}/include
    ${WEBRTC_DIR}/include/third_party/abseil-cpp
    ${WEBRTC_DIR}/include/third_party/jsoncpp/source/include
    ${WEBRTC_DIR}/include/third_party/jsoncpp/generated
)

set(WEBRTC_LIBRARY_DIR
  ${WEBRTC_DIR}/lib
)

find_library(WEBRTC_LIBRARY_DEBUG
  NAMES webrtcd
  PATHS ${WEBRTC_LIBRARY_DIR}
)

find_library(WEBRTC_LIBRARY_RELEASE
  NAMES webrtc
  PATHS ${WEBRTC_LIBRARY_DIR}
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