target_sources(WebRTCLib
  PRIVATE
    H264ProfileLevelId.cpp
    H264ProfileLevelId.h
    VideoCodecImpl.h
)

if(Windows OR Linux)
  add_subdirectory(NvCodec)
endif()