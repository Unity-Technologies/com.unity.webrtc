if(Windows OR Linux)
  add_subdirectory(NvCodec)
endif()

if(Android)
  add_subdirectory(AndroidCodec)
endif()