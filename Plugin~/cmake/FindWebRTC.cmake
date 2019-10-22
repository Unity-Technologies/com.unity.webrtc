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

find_library(AUDIO_DECODER_OPUS_LIBRARY
  NAMES audio_decoder_opus
  PATHS ${LIBRARY_PATHS}
)

find_library(WEBRTC_OPUS_LIBRARY
  NAMES webrtc_opus
  PATHS ${LIBRARY_PATHS}
)

set(WEBRTC_LIBRARIES
  ${WEBRTC_LIBRARY}
  ${AUDIO_DECODER_OPUS_LIBRARY}
  ${WEBRTC_OPUS_LIBRARY}
)