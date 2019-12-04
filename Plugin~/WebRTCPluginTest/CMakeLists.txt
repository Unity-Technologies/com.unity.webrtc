cmake_minimum_required(VERSION 3.10)

project(webrtc-test)

file(GLOB sources
  *.cpp
  *.h
  ../WebRTCPlugin/*h
  ../WebRTCPlugin/*cpp
  ../WebRTCPlugin/GraphicsDevice/*.h
  ../WebRTCPlugin/GraphicsDevice/*.cpp
  ../WebRTCPlugin/GraphicsDevice/OpenGL/*.h
  ../WebRTCPlugin/GraphicsDevice/OpenGL/*.cpp
  ../WebRTCPlugin/GraphicsDevice/Vulkan/*.h
  ../WebRTCPlugin/GraphicsDevice/Vulkan/*.cpp
  ../WebRTCPlugin/GraphicsDevice/Vulkan/Cuda/*.h
  ../WebRTCPlugin/GraphicsDevice/Vulkan/Cuda/*.cpp
  ../WebRTCPlugin/Codec/*.h
  ../WebRTCPlugin/Codec/*.cpp
  ../WebRTCPlugin/Codec/NvCodec/NvEncoder.cpp
  ../WebRTCPlugin/Codec/NvCodec/NvEncoder.h
  ../WebRTCPlugin/Codec/NvCodec/NvEncoderGL.cpp
  ../WebRTCPlugin/Codec/NvCodec/NvEncoderGL.h
  ../WebRTCPlugin/Codec/NvCodec/NvEncoderCuda.cpp
  ../WebRTCPlugin/Codec/NvCodec/NvEncoderCuda.h
)

add_executable(WebRTCPluginTest ${sources})

if(APPLE OR UNIX)
  message(STATUS "macos or linux")
  target_compile_options(WebRTCPluginTest PRIVATE -Wall -Wextra -Wno-unknown-pragmas -Wno-unused-parameter -Wno-missing-field-initializers -Wno-long-long)
  target_compile_features(WebRTCPluginTest PRIVATE cxx_std_14)
endif()

find_package(OpenGL REQUIRED)
find_package(GLUT REQUIRED)
find_package(X11 REQUIRED)
find_package(GTest REQUIRED)
include(GoogleTest)

if(APPLE)
  target_compile_definitions(WebRTCPluginTest
    PRIVATE
    WEBRTC_MAC
    WEBRTC_POSIX
    )
  target_link_libraries(WebRTCPluginTest
    PRIVATE
    ${WEBRTC_LIBRARIES}
    ${GLEW_LIBRARIES}
    ${OPENGL_LIBRARIES}
    ${FRAMEWORK_LIBS}
    GTest::GTest
    GTest::Main
    )
elseif(UNIX)
  set(CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS} -fno-lto -fno-rtti -stdlib=libc++")

  target_compile_definitions(WebRTCPluginTest
    PRIVATE
    WEBRTC_LINUX
    WEBRTC_POSIX
    GLEW_STATIC
    )

  target_link_libraries(WebRTCPluginTest
    PRIVATE
    ${WEBRTC_LIBRARIES}
    ${GLEW_STATIC_LIBRARY}
    ${OPENGL_LIBRARIES}
    ${GLUT_LIBRARY}
    ${X11_LIBRARIES}
    ${CMAKE_DL_LIBS}
    ${GTEST_MAIN_LIBRARIES}
    ${GTEST_LIBRARIES}
    ${CMAKE_THREAD_LIBS_INIT}
#    ${CUDA_LIBRARY}
    ${Vulkan_LIBRARY}
    cuda
    )
  target_include_directories(WebRTCPluginTest
    PRIVATE
    ${CUDA_INCLUDE_DIRS}
    )
endif()

target_include_directories(WebRTCPluginTest
  PRIVATE
  .
  ../WebRTCPlugin
  ${CMAKE_SOURCE_DIR}/unity/include
  ${CMAKE_SOURCE_DIR}/gl3w
  ${WEBRTC_INCLUDE_DIR}
  ${OPENGL_INCLUDE_DIR}
  ${GLEW_INCLUDE_DIRS}
  ${GTEST_INCLUDE_DIRS}
)

gtest_add_tests(TARGET WebRTCPluginTest)