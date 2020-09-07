cmake_minimum_required(VERSION 3.10)

project(webrtc-plugin)

# Common source files
file(GLOB sources
  ../../.editorconfig
  *.cpp
  *.h
  GraphicsDevice/*.h
  GraphicsDevice/*.cpp
  Codec/*.h
  Codec/*.cpp
)

if(MSVC)
  file(GLOB append_source
    GraphicsDevice/Vulkan/*.h
    GraphicsDevice/Vulkan/*.cpp
    GraphicsDevice/Vulkan/Cuda/*.h
    GraphicsDevice/Vulkan/Cuda/*.cpp
    GraphicsDevice/D3D11/*.h
    GraphicsDevice/D3D11/*.cpp
    GraphicsDevice/D3D12/*.h
    GraphicsDevice/D3D12/*.cpp
    Codec/SoftwareCodec/*.h
    Codec/SoftwareCodec/*.cpp
    Codec/NvCodec/NvEncoderD3D11.cpp
    Codec/NvCodec/NvEncoderD3D11.h
    Codec/NvCodec/NvEncoderD3D12.cpp
    Codec/NvCodec/NvEncoderD3D12.h
    Codec/NvCodec/NvEncoder.cpp
    Codec/NvCodec/NvEncoder.h
    Codec/NvCodec/NvEncoderCuda.cpp
    Codec/NvCodec/NvEncoderCuda.h
  )
  list (APPEND sources ${append_source})
elseif(APPLE)
  file(GLOB append_source
    ../libcxx/debug.cpp
    *.mm
    GraphicsDevice/Metal/*.h
    GraphicsDevice/Metal/*.mm
    Codec/SoftwareCodec/*.h
    Codec/SoftwareCodec/*.cpp
  )
  list (APPEND sources ${append_source})
elseif(UNIX)
  file(GLOB append_source
    GraphicsDevice/OpenGL/*.h
    GraphicsDevice/OpenGL/*.cpp
    GraphicsDevice/Vulkan/*.h
    GraphicsDevice/Vulkan/*.cpp
    GraphicsDevice/Vulkan/Cuda/*.h
    GraphicsDevice/Vulkan/Cuda/*.cpp
    Codec/SoftwareCodec/*.h
    Codec/SoftwareCodec/*.cpp
    Codec/NvCodec/NvEncoder.cpp
    Codec/NvCodec/NvEncoder.h
    Codec/NvCodec/NvEncoderGL.cpp
    Codec/NvCodec/NvEncoderGL.h
    Codec/NvCodec/NvEncoderCuda.cpp
    Codec/NvCodec/NvEncoderCuda.h
  )
  list (APPEND sources ${append_source})
endif()

add_plugin(webrtc
  SOURCES ${sources}
  PLUGINS_DIR ${CMAKE_SOURCE_DIR}/../Runtime/Plugins/x86_64
)

if(MSVC)
  # Force to always compile with W4
  if(CMAKE_CXX_FLAGS MATCHES "/W[0-4]")
    string(REGEX REPLACE "/W[0-4]" "/W4" CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS}")
  else()
    set(CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS} /W4")
  endif()
  # Replace /GR option to disable RTTI
  foreach(flag_var CMAKE_CXX_FLAGS CMAKE_CXX_FLAGS_DEBUG CMAKE_CXX_FLAGS_RELEASE
                   CMAKE_CXX_FLAGS_MINSIZEREL CMAKE_CXX_FLAGS_RELWITHDEBINFO)
    string(REPLACE /GR /GR- "${flag_var}" "${${flag_var}}")
  endforeach()
elseif(APPLE OR UNIX)
  message(STATUS "macos or linux")
  target_compile_options(webrtc PRIVATE -Wall -Wextra -Wno-unknown-pragmas -Wno-unused-parameter -Wno-missing-field-initializers -Wno-long-long)
  target_compile_features(webrtc PRIVATE cxx_std_14)
endif()

if(MSVC)
  # Use precompiled header
  set(CMAKE_CXX_FLAGS 
    "${CMAKE_CXX_FLAGS} /Yu /Yupch.h")
  set(PROJECT_BINARY_DIR 
    "${CMAKE_SOURCE_DIR}/../Runtime/Plugins/x86_64")

  target_compile_definitions(webrtc
    PRIVATE
      WEBRTC_WIN
      NOMINMAX
  )
  target_link_libraries(webrtc
    PRIVATE
      ${WEBRTC_LIBRARY}
      ${Vulkan_LIBRARY}
      ${CUDA_CUDA_LIBRARY}
      ${NVCODEC_LIBRARIES}
      d3d11
      winmm
      Secur32
      Msdmo
      Dmoguids
      wmcodecdspuuid
  )
  target_include_directories(webrtc
    PRIVATE
      ${CUDA_INCLUDE_DIRS}
      ${Vulkan_INCLUDE_DIR}
      ${NVCODEC_INCLUDE_DIR}
  )

  # Set linker option
  set_target_properties( webrtc 
    PROPERTIES LINK_FLAGS 
      "/SUBSYSTEM:WINDOWS /DELAYLOAD:nvcuda.dll /DELAYLOAD:vulkan-1.dll"
  )

  # Change output directory for shared library
  set_target_properties( webrtc 
    PROPERTIES RUNTIME_OUTPUT_DIRECTORY_RELEASE
      ${PROJECT_BINARY_DIR}
  )
  set_target_properties( webrtc 
    PROPERTIES RUNTIME_OUTPUT_DIRECTORY_DEBUG
      ${PROJECT_BINARY_DIR}
  )

  # Creare precompiled header
  set_source_files_properties(pch.cpp 
    PROPERTIES COMPILE_FLAGS 
      "/Yc /Ycpch.h"
  )
elseif(APPLE)
  EXECUTE_PROCESS( COMMAND uname -m COMMAND tr -d '\n' OUTPUT_VARIABLE ARCHITECTURE )
  message( STATUS "Architecture: ${ARCHITECTURE}" )
  set(CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS} -fno-lto -fno-rtti")
  set(CMAKE_C_FLAGS "-x objective-c")
  set(CMAKE_CXX_FLAGS "-x objective-c++")
  set(PROJECT_BINARY_DIR "${CMAKE_SOURCE_DIR}/../Runtime/Plugins/${ARCHITECTURE}")

  target_compile_definitions(webrtc
    PRIVATE
      WEBRTC_MAC
      WEBRTC_POSIX
  )
  target_link_libraries(webrtc
    PRIVATE
      ${WEBRTC_LIBRARY}
      ${OPENGL_LIBRARIES}
      ${FRAMEWORK_LIBS}
  )
  set_target_properties( webrtc 
    PROPERTIES LIBRARY_OUTPUT_DIRECTORY_DEBUG 
      ${PROJECT_BINARY_DIR}
  )
  set_target_properties( webrtc 
    PROPERTIES LIBRARY_OUTPUT_DIRECTORY_RELEASE 
      ${PROJECT_BINARY_DIR}
  )
  target_include_directories(webrtc
    PRIVATE
      ${WEBRTC_OBJC_INCLUDE_DIR}
  )

  set_target_properties( webrtc 
    PROPERTIES LINK_FLAGS 
      "-ObjC"
  )  
elseif(UNIX)
  set(CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS} -fno-lto -fno-rtti -stdlib=libc++")

  target_compile_definitions(webrtc
    PRIVATE
      WEBRTC_LINUX
      WEBRTC_POSIX
  )
  target_link_libraries(webrtc
    PRIVATE
      ${WEBRTC_LIBRARY}
      ${OPENGL_opengl_LIBRARY}
      ${Vulkan_LIBRARY}
      ${NVCODEC_LIBRARIES}
      cuda
  )
  target_include_directories(webrtc
    PRIVATE
      ${CUDA_INCLUDE_DIRS}
      ${Vulkan_INCLUDE_DIR}
      ${NVCODEC_INCLUDE_DIR}
  )
endif()

target_include_directories(webrtc
  PRIVATE
    .
    ${CMAKE_SOURCE_DIR}/unity/include
    ${WEBRTC_INCLUDE_DIR}
    ${OPENGL_INCLUDE_DIR}
)
