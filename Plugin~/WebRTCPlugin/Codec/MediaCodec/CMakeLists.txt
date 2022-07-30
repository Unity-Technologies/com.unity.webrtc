target_link_libraries(WebRTCLib
  PRIVATE
    mediandk
)

target_sources(WebRTCLib
  PRIVATE
    MediaCodec.cpp
    MediaCodec.h
    MediaCodecEncoderImpl.cpp
    MediaCodecEncoderImpl.h
    MediaCodecDecoderImpl.cpp
    MediaCodecDecoderImpl.h
)