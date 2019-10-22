# Find WebRTC include path

set(WEBRTC_DIR "webrtc")

find_path(WEBRTC_INCLUDE_DIR
  NAMES
    include
    include/third_party/abseil-cpp
    include/third_party/jsoncpp/source/include
    include/third_party/jsoncpp/generated
  PATHS
    ${WEBRTC_DIR}
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