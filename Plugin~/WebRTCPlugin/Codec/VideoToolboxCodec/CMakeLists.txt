if(iOS OR macOS)
  target_sources(WebRTCLib
    PRIVATE
      VideoToolboxEncoder.cpp
      VideoToolboxEncoder.h
  )
endif()
