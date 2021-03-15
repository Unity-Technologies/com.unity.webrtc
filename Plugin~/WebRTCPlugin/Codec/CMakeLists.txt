target_sources(WebRTCLib
  PRIVATE
    EncoderFactory.cpp
    EncoderFactory.h
    IEncoder.h
)

add_subdirectory(SoftwareCodec)

if(Windows OR Linux)
  add_subdirectory(NvCodec)
endif()

if(Android)
  add_subdirectory(AndroidCodec)
endif()