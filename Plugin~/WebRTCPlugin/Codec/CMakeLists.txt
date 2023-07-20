target_sources(
  WebRTCLib
  PRIVATE CreateVideoCodecFactory.cpp CreateVideoCodecFactory.h
          H264ProfileLevelId.cpp H264ProfileLevelId.h
          SimulcastEncoderFactory.cpp SimulcastEncoderFactory.h)

if(Windows OR Linux)
  add_subdirectory(NvCodec)
endif()
