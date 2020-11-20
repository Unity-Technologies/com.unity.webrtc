target_sources(WebRTCLib
  PRIVATE
    NvEncoder.cpp
    NvEncoder.h
    NvEncoderCuda.cpp
    NvEncoderCuda.h
)

if(Windows)
  target_sources(WebRTCLib
    PRIVATE
      NvEncoderD3D11.cpp
      NvEncoderD3D11.h
      NvEncoderD3D12.cpp
      NvEncoderD3D12.h
  )
endif()

if(Linux)
  target_sources(WebRTCLib
    PRIVATE
      NvEncoderGL.cpp
      NvEncoderGL.h
  )
endif()
