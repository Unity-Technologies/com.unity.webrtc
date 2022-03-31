add_library(CudaBicubic STATIC)

target_sources(CudaBicubic
  PRIVATE
    BicubicTexture.cpp
    BicubicTexture.h
    bicubicTexture_cuda.cu
    bicubicTexture_kernel.cuh
)

set_source_files_properties(bicubicTexture_cuda.cu
 PROPERTIES LANGUAGE CUDA
)

set_source_files_properties(bicubicTexture_kernel.cuh
 PROPERTIES LANGUAGE CUDA
)

#target_include_directories(CudaBicubic
#  PRIVATE
#    ${CUDA_INCLUDE_DIRS}
#)

target_link_libraries(CudaBicubic
  PUBLIC
    CUDA::cudart_static
    CUDA::cuda_driver
)

set(CUDA_BICUBIC_DIR 
  ${CMAKE_SOURCE_DIR}/Cuda/Bicubic
  CACHE PATH
  "CUDA_BICUBIC INCLUDE PATH"
)

if(Linux)
  # enable -fPIC option
  set_target_properties(CudaBicubic
    PROPERTIES
      POSITION_INDEPENDENT_CODE ON
  )
endif()