cmake_minimum_required(VERSION 3.10)

file(GLOB sources
  *.cpp
  *.h
  ../WebRTCPlugin/*h
  ../WebRTCPlugin/*cpp
  ../WebRTCPlugin/GraphicsDevice/*.h
  ../WebRTCPlugin/GraphicsDevice/*.cpp
  ../WebRTCPlugin/GraphicsDevice/OpenGL/*.h
  ../WebRTCPlugin/GraphicsDevice/OpenGL/*.cpp
  ../WebRTCPlugin/Codec/*.h
  ../WebRTCPlugin/Codec/*.cpp
  ../WebRTCPlugin/Codec/NvCodec/NvEncoder.cpp
  ../WebRTCPlugin/Codec/NvCodec/NvEncoder.h
  ../WebRTCPlugin/Codec/NvCodec/NvEncoderGL.cpp
  ../WebRTCPlugin/Codec/NvCodec/NvEncoderGL.h
)

add_executable(WebRTCPluginTest ${sources})

if(APPLE OR UNIX)
  message(STATUS "macos or linux")
  target_compile_options(WebRTCPluginTest PRIVATE -Wall -Wextra -Wno-unknown-pragmas -Wno-unused-parameter -Wno-missing-field-initializers -Wno-long-long)
  target_compile_features(WebRTCPluginTest PRIVATE cxx_std_14)
endif()

find_package(OpenGL REQUIRED)
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
  set(CMAKE_C_COMPILER_ID Clang)
  set(CMAKE_CXX_COMPILER_ID Clang)
  set(CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS} -fno-lto -fno-rtti")
  set(CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS} -nostdinc++")

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
    ${X11_LIBRARIES}
    ${CMAKE_DL_LIBS}
    ${LIBCXX_LIBRARIES}
    ${GTEST_MAIN_LIBRARIES}
    ${GTEST_LIBRARIES}
    ${CMAKE_THREAD_LIBS_INIT}
    )
endif()

target_include_directories(WebRTCPluginTest
  PRIVATE
  .
  ../WebRTCPlugin
  ${CMAKE_SOURCE_DIR}/unity/include
  ${WEBRTC_INCLUDE_DIR}
  ${OPENGL_INCLUDE_DIR}
  ${GLEW_INCLUDE_DIRS}
  ${GTEST_INCLUDE_DIRS}
  ${LIBCXX_INCLUDE_DIRS}
)

gtest_add_tests(TARGET WebRTCPluginTest)