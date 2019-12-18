cmake_minimum_required(VERSION 3.10)

project(webrtc-test)

# Common source files
file(GLOB sources
    *.cpp
    *.h
    ../WebRTCPlugin/*.h
    ../WebRTCPlugin/*.cpp
    ../WebRTCPlugin/GraphicsDevice/*.h
    ../WebRTCPlugin/GraphicsDevice/*.cpp
    ../WebRTCPlugin/Codec/*.h
    ../WebRTCPlugin/Codec/*.cpp
)

# Apple/Unix specific source files
if(APPLE)
    file(GLOB append_source
        ../WebRTCPlugin/GraphicsDevice/Metal/*.h
        ../WebRTCPlugin/GraphicsDevice/Metal/*.mm
        ../WebRTCPlugin/Codec/SoftwareCodec/*.h
        ../WebRTCPlugin/Codec/SoftwareCodec/*.cpp
        #Codec/VideoToolbox/VTEncoderMetal.mm
        #Codec/VideoToolbox/VTEncoderMetal.h
    )

    list (APPEND sources ${append_source})
elseif(UNIX)
    file(GLOB append_source
        NvCodec/*.h
        NvCodec/*.cpp
        ../WebRTCPlugin/GraphicsDevice/OpenGL/*.h
        ../WebRTCPlugin/GraphicsDevice/OpenGL/*.cpp
        ../WebRTCPlugin/GraphicsDevice/Vulkan/*.h
        ../WebRTCPlugin/GraphicsDevice/Vulkan/*.cpp
        ../WebRTCPlugin/GraphicsDevice/Vulkan/Cuda/*.h
        ../WebRTCPlugin/GraphicsDevice/Vulkan/Cuda/*.cpp
        ../WebRTCPlugin/Codec/NvCodec/NvEncoder.cpp
        ../WebRTCPlugin/Codec/NvCodec/NvEncoder.h
        ../WebRTCPlugin/Codec/NvCodec/NvEncoderGL.cpp
        ../WebRTCPlugin/Codec/NvCodec/NvEncoderGL.h
        ../WebRTCPlugin/Codec/NvCodec/NvEncoderCuda.cpp
        ../WebRTCPlugin/Codec/NvCodec/NvEncoderCuda.h
    )
    list (APPEND sources ${append_source})
endif()


add_executable(WebRTCPluginTest ${sources} ../unity/include/IUnityGraphicsMetal.h)

if(APPLE OR UNIX)
  message(STATUS "macos or linux")
  target_compile_options(WebRTCPluginTest PRIVATE -Wall -Wextra -Wno-unknown-pragmas -Wno-unused-parameter -Wno-missing-field-initializers -Wno-long-long)
  target_compile_features(WebRTCPluginTest PRIVATE cxx_std_14)
endif()

find_package(GTest REQUIRED)
include(GoogleTest)

if(APPLE)
    set(CMAKE_C_FLAGS "-x objective-c")
    set(CMAKE_CXX_FLAGS "-x objective-c++")

    target_compile_definitions(WebRTCPluginTest
        PRIVATE
        WEBRTC_MAC
        WEBRTC_POSIX
    )
    target_link_libraries(WebRTCPluginTest
        PRIVATE
        ${WEBRTC_LIBRARIES}
        ${OPENGL_LIBRARIES}
        ${FRAMEWORK_LIBS}
        ${GTEST_MAIN_LIBRARIES}
        ${GTEST_LIBRARIES}
    )

    target_include_directories(WebRTCPluginTest
        PRIVATE
        ..
    )
elseif(UNIX)
    find_package(OpenGL REQUIRED)
    find_package(GLUT REQUIRED)
    set(CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS} -fno-lto -fno-rtti -stdlib=libc++")

    target_compile_definitions(WebRTCPluginTest
        PRIVATE
        WEBRTC_LINUX
        WEBRTC_POSIX
    )

    target_link_libraries(WebRTCPluginTest
        PRIVATE
        ${WEBRTC_LIBRARIES}
        ${OPENGL_opengl_LIBRARY}
        ${GLUT_LIBRARY}
        ${CMAKE_DL_LIBS}
        ${GTEST_MAIN_LIBRARIES}
        ${GTEST_LIBRARIES}
        ${CMAKE_THREAD_LIBS_INIT}
        ${Vulkan_LIBRARY}
        cuda
    )
    target_include_directories(WebRTCPluginTest
        PRIVATE
        ..
        ${CUDA_INCLUDE_DIRS}
    )
endif()

gtest_add_tests(TARGET WebRTCPluginTest)

target_include_directories(WebRTCPluginTest
    PRIVATE
    .
    ../WebRTCPlugin
    ${CMAKE_SOURCE_DIR}/unity/include
    ${WEBRTC_INCLUDE_DIR}
    ${OPENGL_INCLUDE_DIR}
    ${GTEST_INCLUDE_DIRS}
)
