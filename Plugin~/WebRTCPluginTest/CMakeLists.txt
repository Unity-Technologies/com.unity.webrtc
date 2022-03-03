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

# todo(kazuki):: fix linker error on macOS and iOS
if(NOT (macOS OR iOS))
target_sources(WebRTCLibTest
  PRIVATE
    VideoRendererTest.cpp
    VideoTrackSourceTest.cpp
)    
endif()

if(Windows OR Linux)
  add_subdirectory(NvCodec)
endif()

include(FetchContent)

FetchContent_Declare(
  googletest
  GIT_REPOSITORY https://github.com/google/googletest.git
  GIT_TAG        release-1.10.0
)

FetchContent_GetProperties(googletest)
if(NOT googletest_POPULATED)
  FetchContent_Populate(googletest)

  add_subdirectory(
    ${googletest_SOURCE_DIR} 
    ${googletest_BINARY_DIR}
  )
endif()

include(GoogleTest)

if(NOT iOS)
  target_include_directories(WebRTCLibTest
    PRIVATE
      $<TARGET_PROPERTY:gtest,INTERFACE_SYSTEM_INCLUDE_DIRECTORIES>
      $<TARGET_PROPERTY:gtest_main,INTERFACE_SYSTEM_INCLUDE_DIRECTORIES>
      $<TARGET_PROPERTY:gmock,INTERFACE_SYSTEM_INCLUDE_DIRECTORIES>
      $<TARGET_PROPERTY:gmock_main,INTERFACE_SYSTEM_INCLUDE_DIRECTORIES>
  )
  set_target_properties(gtest
    PROPERTIES
      MSVC_RUNTIME_LIBRARY "MultiThreaded$<$<CONFIG:Debug>:Debug>"
  )
  set_target_properties(gtest_main
    PROPERTIES
      MSVC_RUNTIME_LIBRARY "MultiThreaded$<$<CONFIG:Debug>:Debug>"
  )
  set_target_properties(gmock
    PROPERTIES
      MSVC_RUNTIME_LIBRARY "MultiThreaded$<$<CONFIG:Debug>:Debug>"
  )
  set_target_properties(gmock_main
    PROPERTIES
      MSVC_RUNTIME_LIBRARY "MultiThreaded$<$<CONFIG:Debug>:Debug>"
  )
else()
  set(GOOGLETEST_ROOT_DIR ${CMAKE_SOURCE_DIR}/googletest)
  set(GTEST_INCLUDE_DIRS
    ${GOOGLETEST_ROOT_DIR}/include
  )
  set(GTEST_MAIN_LIBRARIES 
    debug ${GOOGLETEST_ROOT_DIR}/lib/libgtest_maind.a
    optimized ${GOOGLETEST_ROOT_DIR}/lib/libgtest_main.a
  )
  set(GTEST_LIBRARIES 
    debug ${GOOGLETEST_ROOT_DIR}/lib/libgtestd.a
    optimized ${GOOGLETEST_ROOT_DIR}/lib/libgtest.a
  )
  set(GMOCK_LIBRARIES
    debug ${GOOGLETEST_ROOT_DIR}/lib/libgmockd.a
    optimized ${GOOGLETEST_ROOT_DIR}/lib/libgmock.a
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
      ${NVCODEC_LIBRARIES}
      gtest
      gmock
      gtest_main
      d3d11
      d3d12
      dxgi
      winmm
      Secur32
      Msdmo
      Dmoguids
      wmcodecdspuuid
      WebRTCLib
      Strmiids
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
      gtest
      gmock
      gtest_main
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
  find_package(PkgConfig REQUIRED)

  pkg_check_modules(GLIB REQUIRED glib-2.0)

  target_compile_options(WebRTCLibTest
    PUBLIC
      -fno-lto
      -fno-rtti
      $<$<CONFIG:Debug>:-fsanitize=address$<COMMA>undefined>
  )

  target_link_options(WebRTCLibTest
    PUBLIC
      $<$<CONFIG:Debug>:-fsanitize=address$<COMMA>undefined>
  )

  target_compile_definitions(WebRTCLibTest
    PRIVATE
      $<$<CONFIG:Debug>:LEAK_SANITIZER>
  )

  target_link_libraries(WebRTCLibTest
    PRIVATE
      ${WEBRTC_LIBRARY}
      ${OPENGL_opengl_LIBRARY}
      ${GLUT_LIBRARY}
      ${CMAKE_DL_LIBS}
      ${CMAKE_THREAD_LIBS_INIT}
      ${Vulkan_LIBRARY}
      ${GLIB_LIBRARIES}
      gtest
      gmock
      gtest_main
      WebRTCLib
  )
  target_include_directories(WebRTCLibTest
    PRIVATE
      ..
      ${CUDA_INCLUDE_DIRS}
      ${NVCODEC_INCLUDE_DIR}
      ${GLIB_INCLUDE_DIRS}
  )
elseif(iOS)
  set(CMAKE_C_FLAGS "-x objective-c")
  set(CMAKE_CXX_FLAGS "-x objective-c++")

  set_target_properties(WebRTCLibTest 
    PROPERTIES 
      LINK_FLAGS "-ObjC"
      MACOSX_BUNDLE_GUI_IDENTIFIER "com.unity.WebRTCTest"
      MACOSX_BUNDLE_BUNDLE_VERSION "${CMAKE_PROJECT_VERSION}"
      MACOSX_BUNDLE_SHORT_VERSION_STRING "${CMAKE_PROJECT_VERSION}"
  )
  target_link_libraries(WebRTCLibTest
    PRIVATE
      ${WEBRTC_LIBRARY}
      ${OPENGL_LIBRARIES}
      ${FRAMEWORK_LIBS}
      gtest
      gmock
      gtest_main
      WebRTCLib
  )
  target_include_directories(WebRTCLibTest
    PRIVATE
      ..
      ${WEBRTC_OBJC_INCLUDE_DIR}
  )
elseif(Android)
  target_link_libraries(WebRTCLibTest
    PRIVATE
      gtest
      gmock
      gtest_main
      WebRTCLib
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
)
