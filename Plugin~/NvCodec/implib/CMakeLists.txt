target_sources(
  NvCodec
  PRIVATE libcuda.so.init.c libcuda.so.tramp.S libnvcuvid.so.init.c
          libnvcuvid.so.tramp.S libnvidia-encode.so.init.c
          libnvidia-encode.so.tramp.S)
