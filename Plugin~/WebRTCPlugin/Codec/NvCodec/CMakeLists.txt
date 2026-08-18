target_sources(
  WebRTCLib
  PRIVATE NvCodec.cpp
          NvCodec.h
          NvDecoderImplH264.cpp
          NvDecoderImplH265.cpp
          NvDecoderImpl.h
          NvEncoderCudaWithCUarray.cpp
          NvEncoderCudaWithCUarray.h
          NvEncoderImplH264.cpp
          NvEncoderImplH265.cpp
          NvEncoderImpl.h)
