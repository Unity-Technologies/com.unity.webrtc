set(NVCODEC_DIR ${CMAKE_SOURCE_DIR}/NvCodec)
set(NVCODEC_UTIL_DIR ${NVCODEC_DIR}/Utils)
set(NVENC_DIR ${NVCODEC_DIR}/NvCodec/NvEncoder)
set(NVDEC_DIR ${NVCODEC_DIR}/NvCodec/NvDecoder)

set(NVCODEC_INCLUDE_DIR
    ${NVCODEC_DIR}/include ${NVCODEC_DIR}/NvCodec ${NVCODEC_DIR}/Utils
    CACHE PATH "NVCODEC INCLUDE PATH")

file(GLOB sources ${NVCODEC_UTIL_DIR}/* ${NVENC_DIR}/* ${NVDEC_DIR}/*)

add_library(NvCodec STATIC ${sources})

# Delayload on linux
if(Linux)
  add_subdirectory(implib)
endif()

target_include_directories(
  NvCodec PRIVATE ${CUDA_INCLUDE_DIRS} ${Vulkan_INCLUDE_DIR}
                  ${GLEW_INCLUDE_DIRS} ${NVCODEC_INCLUDE_DIR})

if(Windows)
  if(CMAKE_SIZEOF_VOID_P EQUAL 8)
    set(CUVID_LIB ${CMAKE_CURRENT_SOURCE_DIR}/lib/x64/nvcuvid.lib)
    set(NVENCODEAPI_LIB ${CMAKE_CURRENT_SOURCE_DIR}/lib/x64/nvencodeapi.lib)
  else()
    set(CUVID_LIB ${CMAKE_CURRENT_SOURCE_DIR}/lib/Win32/nvcuvid.lib)
    set(NVENCODEAPI_LIB ${CMAKE_CURRENT_SOURCE_DIR}/lib/Win32/nvencodeapi.lib)
  endif()
else()
  find_library(CUVID_LIB nvcuvid)
  find_library(NVENCODEAPI_LIB nvidia-encode)
endif()

if(Windows)
  # Select runtime library (MT, MTD) on windows platform
  set_target_properties(
    NvCodec PROPERTIES MSVC_RUNTIME_LIBRARY
                       "MultiThreaded$<$<CONFIG:Debug>:Debug>")
endif()

if(Linux)
  # enable -fPIC option
  set_target_properties(NvCodec PROPERTIES POSITION_INDEPENDENT_CODE ON)
endif()

if(Windows)
  # Use delayload linker option for nvEncodeAPI64.dll and nvcuvid.dll
  set(NVCODEC_LIBRARIES
      NvCodec ${CUVID_LIB} ${NVENCODEAPI_LIB}
      CACHE PATH "NVCODEC LIBRARIES PATH")
elseif(Linux)
  # Use implib.so for delayload on Linux If you don't use implib, need to link
  # libnvcuvid.so and libnvidia-encode.so
  set(NVCODEC_LIBRARIES
      NvCodec
      CACHE PATH "NVCODEC LIBRARIES PATH")
endif()
