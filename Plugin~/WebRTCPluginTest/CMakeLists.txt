enable_testing()

add_executable(WebRTCLibTest)

target_sources(WebRTCLibTest
  PRIVATE
    pch.cpp
    pch.h
    ContextTest.cpp
    GraphicsDeviceTest.cpp
    GraphicsDeviceTestBase.cpp
    GraphicsDeviceTestBase.h
)

# todo(kazuki):: fix linker error on macOS
if(NOT macOS)
target_sources(WebRTCLibTest
  PRIVATE
    VideoRendererTest.cpp
    VideoTrackSourceTest.cpp
)    
endif()

if(Windows OR Linux)
  add_subdirectory(NvCodec)
endif()

# specify gtest root directory
if(Windows)
  set(GTEST_ROOT "${CMAKE_SOURCE_DIR}/googletest")
  set(GMOCK_ROOT "${CMAKE_SOURCE_DIR}/googletest")
endif()

#find_package(GMock REQUIRED PATHS "/usr/local" NO_CMAKE_FIND_ROOT_PATH)
#enable_testing()
#include(FetchContent)
#FetchContent_Declare(googletest
#  GIT_REPOSITORY https://github.com/google/googletest.git
#  GIT_TAG ec44c6c1675c25b9827aacd08c02433cccde7780
#)
#FetchContent_GetProperties(googletest)
#if(NOT googletest_POPULATED)
#  FetchContent_Populate(googletest)
#  add_subdirectory(
#    ${googletest_SOURCE_DIR}
#    ${googletest_BINARY_DIR}
#  )
#endif()

find_package(GTest)
find_package(GMock)
include(GoogleTest)

if (NOT GTest_FOUND AND UNIX)
  message("FindGTest could not find GTest module, so set variables directly.")
  set(GTEST_INCLUDE_DIRS
    "/usr/local/include"
  )

  set(GTEST_LIBRARIES
    debug "/usr/local/lib/libgtestd.a"
    optimized "/usr/local/lib/libgtest.a"
    CACHE STRING "GTEST library"
  )

  set(GMOCK_LIBRARIES
    debug "/usr/local/lib/libgmockd.a"
    optimized "/usr/local/lib/libgmock.a"
    CACHE STRING "GMOCK library"
  )

  set(GTEST_MAIN_LIBRARIES
    debug "/usr/local/lib/libgtest_maind.a"
    optimized "/usr/local/lib/libgtest_main.a"
    CACHE STRING "GTEST Main library"
  )
endif()  

if(Windows)
  # Use precompiled header
  set(CMAKE_CXX_FLAGS 
    "${CMAKE_CXX_FLAGS} /Yu /Yupch.h"
  )
  target_link_libraries(WebRTCLibTest
      PRIVATE
      ${WEBRTC_LIBRARY}
      ${Vulkan_LIBRARY}
      ${CUDA_CUDA_LIBRARY}
      ${GTEST_MAIN_LIBRARIES}
      ${GTEST_LIBRARIES}
      ${GMOCK_LIBRARIES}
      ${NVCODEC_LIBRARIES}
      d3d11
      d3d12
      dxgi
      winmm
      Secur32
      Msdmo
      Dmoguids
      wmcodecdspuuid
      WebRTCLib
  )
  target_include_directories(WebRTCLibTest
    PRIVATE
      ${CUDA_INCLUDE_DIRS}
      ${Vulkan_INCLUDE_DIR}
      ${NVCODEC_INCLUDE_DIR}
  )
  # Creare precompiled header
  set_source_files_properties(pch.cpp 
    PROPERTIES 
      COMPILE_FLAGS "/Yc /Ycpch.h"
  )
  set_target_properties(WebRTCLibTest
    PROPERTIES
      LINK_FLAGS "/DELAYLOAD:nvcuda.dll /DELAYLOAD:vulkan-1.dll"
      MSVC_RUNTIME_LIBRARY "MultiThreaded$<$<CONFIG:Debug>:Debug>"
  )
elseif(macOS)
  set(CMAKE_C_FLAGS "-x objective-c")
  set(CMAKE_CXX_FLAGS "-x objective-c++")
  
  set_target_properties(WebRTCLibTest 
    PROPERTIES 
      LINK_FLAGS "-ObjC"
  )
  target_link_libraries(WebRTCLibTest
    PRIVATE
      ${WEBRTC_LIBRARY}
      ${OPENGL_LIBRARIES}
      ${FRAMEWORK_LIBS}
      ${GTEST_MAIN_LIBRARIES}
      ${GTEST_LIBRARIES}
      ${GMOCK_LIBRARIES}
      WebRTCLib
  )
  target_include_directories(WebRTCLibTest
    PRIVATE
      ..
      ${WEBRTC_OBJC_INCLUDE_DIR}
  )
elseif(Linux)
  find_package(OpenGL REQUIRED)
  find_package(GLUT REQUIRED)
  set(CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS} -fno-lto -fno-rtti -stdlib=libc++")

  target_link_libraries(WebRTCLibTest
    PRIVATE
      ${WEBRTC_LIBRARY}
      ${OPENGL_opengl_LIBRARY}
      ${GLUT_LIBRARY}
      ${CMAKE_DL_LIBS}
      ${GTEST_MAIN_LIBRARIES}
      ${GTEST_LIBRARIES}
      ${GMOCK_LIBRARIES}
      ${CMAKE_THREAD_LIBS_INIT}
      ${Vulkan_LIBRARY}
      WebRTCLib
  )
  target_include_directories(WebRTCLibTest
    PRIVATE
      ..
      ${CUDA_INCLUDE_DIRS}
      ${NVCODEC_INCLUDE_DIR}
  )
endif()

gtest_add_tests(TARGET WebRTCLibTest)

target_include_directories(WebRTCLibTest
  PRIVATE
    .
    ../WebRTCPlugin
    ${CMAKE_SOURCE_DIR}/unity/include
    ${WEBRTC_INCLUDE_DIR}
    ${OPENGL_INCLUDE_DIR}
    ${GTEST_INCLUDE_DIRS}
)
