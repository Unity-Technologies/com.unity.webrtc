cmake_minimum_required(VERSION 3.10)

project(NvCodec)

set(NVCODEC_DIR ${CMAKE_SOURCE_DIR}/NvCodec)
set(NVCODEC_UTIL_DIR ${NVCODEC_DIR}/Utils)
set(NVENC_DIR ${NVCODEC_DIR}/NvCodec/NvEncoder)
set(NVDEC_DIR ${NVCODEC_DIR}/NvCodec/NvDecoder)

set(NVCODEC_INCLUDE_DIR
  ${NVCODEC_DIR}/include
  ${NVCODEC_DIR}/NvCodec
  CACHE PATH
  "NVCODEC INCLUDE PATH"
  )

file(GLOB sources
  ${NVCODEC_UTIL_DIR}/*
  ${NVENC_DIR}/*
  ${NVDEC_DIR}/*
)

add_library(NvCodec STATIC ${sources})

target_include_directories(NvCodec
  PRIVATE
    ${CUDA_INCLUDE_DIRS}
    ${Vulkan_INCLUDE_DIR}
    ${GLEW_INCLUDE_DIRS}
    ${NVCODEC_INCLUDE_DIR}
)

if(WIN32)
  if(CMAKE_SIZEOF_VOID_P EQUAL 8)
    set(CUVID_LIB ${CMAKE_CURRENT_SOURCE_DIR}/lib/x64/nvcuvid.lib)
    set(NVENCODEAPI_LIB ${CMAKE_CURRENT_SOURCE_DIR}/lib/x64/nvencodeapi.lib)
  else()
    set(CUVID_LIB ${CMAKE_CURRENT_SOURCE_DIR}/lib/Win32/nvcuvid.lib)
    set(NVENCODEAPI_LIB ${CMAKE_CURRENT_SOURCE_DIR}/lib/Win32/nvencodeapi.lib)
  endif()
else ()
  find_library(CUVID_LIB nvcuvid)
  find_library(NVENCODEAPI_LIB nvidia-encode)
endif()

set(NVCODEC_LIBRARIES
  NvCodec
  ${CUVID_LIB}
  ${NVENCODEAPI_LIB}
  CACHE PATH 
  "NVCODEC LIBRARIES PATH"
)