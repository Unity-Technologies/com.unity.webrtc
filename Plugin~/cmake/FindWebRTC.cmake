# Find WebRTC include path

set(WEBRTC_DIR "${CMAKE_SOURCE_DIR}/webrtc")

set(WEBRTC_INCLUDE_DIR
    ${WEBRTC_DIR}/include
    ${WEBRTC_DIR}/include/third_party/abseil-cpp
    ${WEBRTC_DIR}/include/third_party/jsoncpp/source/include
    ${WEBRTC_DIR}/include/third_party/jsoncpp/generated
)

set(LIBRARY_PATHS
  ${WEBRTC_DIR}/lib
)

find_library(WEBRTC_LIBRARY
  NAMES webrtc
  PATHS ${LIBRARY_PATHS}
)

set(WEBRTC_LIBRARIES
  ${WEBRTC_LIBRARY}
)