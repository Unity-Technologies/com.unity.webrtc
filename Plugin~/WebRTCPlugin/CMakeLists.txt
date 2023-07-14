add_library(WebRTCLib STATIC)

target_sources(
  WebRTCLib
  PRIVATE Context.cpp
          Context.h
          CreateSessionDescriptionObserver.cpp
          CreateSessionDescriptionObserver.h
          DataChannelObject.cpp
          DataChannelObject.h
          DummyAudioDevice.cpp
          DummyAudioDevice.h
          EncodedStreamTransformer.cpp
          EncodedStreamTransformer.h
          AudioTrackSinkAdapter.h
          AudioTrackSinkAdapter.cpp
          Logger.cpp
          MediaStreamObserver.cpp
          MediaStreamObserver.h
          pch.cpp
          pch.h
          PeerConnectionObject.cpp
          PeerConnectionObject.h
          PeerConnectionStatsCollectorCallback.cpp
          PeerConnectionStatsCollectorCallback.h
          PlatformBase.h
          ProfilerMarkerFactory.cpp
          ProfilerMarkerFactory.h
          SetLocalDescriptionObserver.cpp
          SetLocalDescriptionObserver.h
          SetRemoteDescriptionObserver.cpp
          SetRemoteDescriptionObserver.h
          ScopedProfiler.h
          ScopedProfiler.cpp
          targetver.h
          UnityAudioDecoderFactory.cpp
          UnityAudioDecoderFactory.h
          UnityAudioEncoderFactory.cpp
          UnityAudioEncoderFactory.h
          UnityAudioTrackSource.cpp
          UnityAudioTrackSource.h
          UnityProfilerInterfaceFunctions.cpp
          UnityProfilerInterfaceFunctions.h
          UnityVideoDecoderFactory.cpp
          UnityVideoDecoderFactory.h
          UnityVideoEncoderFactory.cpp
          UnityVideoEncoderFactory.h
          UnityVideoRenderer.cpp
          UnityVideoRenderer.h
          UnityVideoTrackSource.cpp
          UnityVideoTrackSource.h
          WebRTCMacros.h
          WebRTCPlugin.h
          UnityLogStream.h
          UnityLogStream.cpp
          VideoFrame.cpp
          VideoFrame.h
          VideoFrameAdapter.cpp
          VideoFrameAdapter.h
          VideoFrameScheduler.cpp
          VideoFrameScheduler.h
          VideoFrameUtil.cpp
          VideoFrameUtil.h
          GpuMemoryBuffer.cpp
          GpuMemoryBuffer.h
          GpuMemoryBufferPool.cpp
          GpuMemoryBufferPool.h)

if(CMAKE_CXX_COMPILER_ID MATCHES "Clang")
  # Visual Studio Generator has not been supported supression warnings of
  # external headers yet. https://gitlab.kitware.com/cmake/cmake/-/issues/17904
  if(CMAKE_GENERATOR MATCHES "Visual Studio")
    set(VISUALSTUDIO_GENERATOR 1)
  else()
    set(VISUALSTUDIO_GENERATOR 0)
  endif()

  target_compile_options(
    WebRTCLib
    PUBLIC $<$<NOT:${VISUALSTUDIO_GENERATOR}>:-Werror>
           $<${VISUALSTUDIO_GENERATOR}:-Wno-c++98-compat>
           $<${VISUALSTUDIO_GENERATOR}:-Wno-c++98-compat-pedantic>
           -Wall
           -Wextra
           -Wformat=2
           -Wno-unused-parameter)
endif()

target_compile_definitions(WebRTCLib PUBLIC $<$<CONFIG:Debug>:DEBUG>)

set_target_properties(WebRTCLib PROPERTIES CXX_VISIBILITY_PRESET hidden
                                           VISIBILITY_INLINES_HIDDEN ON)

if(iOS OR macOS)
  target_sources(WebRTCLib PRIVATE ../libcxx/debug.cpp)
else()
  target_sources(WebRTCLib PRIVATE UnityVulkanInterfaceFunctions.cpp
                                   UnityVulkanInterfaceFunctions.h)
endif()

if(Android)
  add_subdirectory(Android)
endif()

add_subdirectory(Codec)
add_subdirectory(GraphicsDevice)

if(Windows)
  target_precompile_headers(WebRTCLib PRIVATE pch.h)
  # Force to always compile with W4
  if(CMAKE_CXX_COMPILER_ID MATCHES "MSVC")
    if(CMAKE_CXX_FLAGS MATCHES "/W[0-4]")
      string(REGEX REPLACE "/W[0-4]" "/W4" CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS}")
    else()
      set(CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS} /W4")
    endif()
  endif()

  # Replace /GR option to disable RTTI
  foreach(flag_var
          CMAKE_CXX_FLAGS CMAKE_CXX_FLAGS_DEBUG CMAKE_CXX_FLAGS_RELEASE
          CMAKE_CXX_FLAGS_MINSIZEREL CMAKE_CXX_FLAGS_RELWITHDEBINFO)
    string(REPLACE /GR /GR- "${flag_var}" "${${flag_var}}")
  endforeach()
endif()

if(Windows)
  set(PROJECT_BINARY_DIR "${CMAKE_SOURCE_DIR}/../Runtime/Plugins/x86_64")

  target_compile_definitions(WebRTCLib PUBLIC WEBRTC_WIN NOMINMAX
                                              VK_NO_PROTOTYPES)
  target_link_libraries(
    WebRTCLib
    PRIVATE ${WEBRTC_LIBRARY}
            ${Vulkan_LIBRARY}
            ${CUDA_CUDA_LIBRARY}
            ${NVCODEC_LIBRARIES}
            d3d11
            winmm
            Secur32
            Iphlpapi
            Msdmo
            Dmoguids
            wmcodecdspuuid
            Strmiids)
  target_include_directories(
    WebRTCLib SYSTEM PUBLIC ${CUDA_INCLUDE_DIRS} ${Vulkan_INCLUDE_DIR}
                            ${NVCODEC_INCLUDE_DIR})

  # Select runtime library (MT, MTD) on windows platform
  set_target_properties(
    WebRTCLib PROPERTIES MSVC_RUNTIME_LIBRARY
                         "MultiThreaded$<$<CONFIG:Debug>:Debug>")
elseif(macOS)
  set(CMAKE_C_FLAGS "${CMAKE_C_FLAGS} -x objective-c")
  set(CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS} -fno-lto -fno-rtti")
  set(CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS} -x objective-c++")
  set(PROJECT_BINARY_DIR "${CMAKE_SOURCE_DIR}/../Runtime/Plugins/macOS")

  target_compile_definitions(WebRTCLib PUBLIC WEBRTC_MAC WEBRTC_POSIX)
  target_link_libraries(WebRTCLib PRIVATE ${WEBRTC_LIBRARY} ${FRAMEWORK_LIBS})
  target_include_directories(WebRTCLib SYSTEM PUBLIC ${WEBRTC_OBJC_INCLUDE_DIR})
  set_target_properties(WebRTCLib PROPERTIES LINK_FLAGS "-ObjC")
elseif(Linux)
  set(PROJECT_BINARY_DIR "${CMAKE_SOURCE_DIR}/../Runtime/Plugins/x86_64")
  set(GLAD_SOURCES_DIR "${CMAKE_SOURCE_DIR}/glad")
  add_subdirectory("${GLAD_SOURCES_DIR}/cmake" glad_cmake)

  glad_add_library(glad_gl_core_46 REPRODUCIBLE STATIC LOADER API gl:core=4.6)
  glad_add_library(glad_vulkan_11 REPRODUCIBLE STATIC LOADER API vulkan=1.1)
  glad_add_library(glad_glx_14 REPRODUCIBLE STATIC LOADER API glx=1.4)

  set_target_properties(glad_gl_core_46 PROPERTIES POSITION_INDEPENDENT_CODE ON)

  set_target_properties(glad_vulkan_11 PROPERTIES POSITION_INDEPENDENT_CODE ON)

  set_target_properties(glad_glx_14 PROPERTIES POSITION_INDEPENDENT_CODE ON)

  target_link_libraries(glad_glx_14 PUBLIC glad_gl_core_46)

  find_package(PkgConfig REQUIRED)

  target_compile_definitions(WebRTCLib PUBLIC WEBRTC_LINUX WEBRTC_POSIX
                                              VK_NO_PROTOTYPES)

  target_link_libraries(WebRTCLib PUBLIC NvCodec glad_gl_core_46 glad_vulkan_11
                                         glad_glx_14 ${WEBRTC_LIBRARY})

  target_include_directories(
    WebRTCLib SYSTEM PUBLIC ${CUDA_INCLUDE_DIRS} glad_gl_core_46 glad_vulkan_11
                            glad_glx_14 ${NVCODEC_INCLUDE_DIR})

  # enable -fPIC option
  set_target_properties(WebRTCLib PROPERTIES POSITION_INDEPENDENT_CODE ON)
elseif(iOS)
  set(CMAKE_C_FLAGS "${CMAKE_C_FLAGS} -x objective-c")
  set(CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS} -fno-lto -fno-rtti")
  set(CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS} -x objective-c++")
  set(PROJECT_BINARY_DIR "${CMAKE_SOURCE_DIR}/../Runtime/Plugins/iOS")

  target_compile_definitions(WebRTCLib PUBLIC WEBRTC_IOS WEBRTC_MAC
                                              WEBRTC_POSIX)
  target_include_directories(WebRTCLib SYSTEM PUBLIC ${WEBRTC_OBJC_INCLUDE_DIR})
  target_link_libraries(WebRTCLib PRIVATE ${WEBRTC_LIBRARY} ${FRAMEWORK_LIBS})
  set_target_properties(WebRTCLib PROPERTIES LINK_FLAGS "-ObjC")
elseif(Android)
  target_compile_definitions(WebRTCLib PUBLIC WEBRTC_ANDROID WEBRTC_POSIX
                                              VK_NO_PROTOTYPES)
  target_link_libraries(WebRTCLib PRIVATE ${WEBRTC_LIBRARY} log OpenSLES GLESv3
                                          EGL)
  target_compile_options(WebRTCLib PUBLIC -fno-lto -fno-rtti -fPIC)

  # Linker excludes unused symbols from the shared library when linking the
  # archive file (libwebrtc.a).
  file(READ "${CMAKE_SOURCE_DIR}/tools/android/jni_symbols.txt" JNI_SYMBOLS)
  string(REPLACE "\n" ";" JNI_SYMBOLS ${JNI_SYMBOLS})
  list(TRANSFORM JNI_SYMBOLS PREPEND "-Wl,--undefined=")
  target_link_options(WebRTCLib PUBLIC ${JNI_SYMBOLS})

  set(PROJECT_BINARY_DIR "${CMAKE_SOURCE_DIR}/../Runtime/Plugins/Android")
endif()

target_include_directories(
  WebRTCLib SYSTEM PUBLIC ${CMAKE_SOURCE_DIR}/unity/include
                          ${WEBRTC_INCLUDE_DIR})

target_include_directories(WebRTCLib PUBLIC .)

add_library(WebRTCPlugin SHARED)

set_target_properties(WebRTCPlugin PROPERTIES CXX_VISIBILITY_PRESET hidden
                                              VISIBILITY_INLINES_HIDDEN ON)

if(iOS)
  set_target_properties(WebRTCPlugin PROPERTIES FRAMEWORK TRUE)
endif()

if(Windows)
  # link delayimp.lib to use delayload when building with Clang on Windows.
  target_link_libraries(WebRTCPlugin PRIVATE WebRTCLib delayimp.lib)
else()
  target_link_libraries(WebRTCPlugin PRIVATE WebRTCLib)
endif()

target_sources(WebRTCPlugin PRIVATE pch.cpp pch.h WebRTCPlugin.cpp
                                    WebRTCPlugin.h UnityRenderEvent.cpp)

# rename dll/framework filename
set_target_properties(
  WebRTCPlugin
  PROPERTIES OUTPUT_NAME webrtc
             RUNTIME_OUTPUT_DIRECTORY_RELEASE ${PROJECT_BINARY_DIR}
             RUNTIME_OUTPUT_DIRECTORY_DEBUG ${PROJECT_BINARY_DIR}
             LIBRARY_OUTPUT_DIRECTORY_RELEASE ${PROJECT_BINARY_DIR}
             LIBRARY_OUTPUT_DIRECTORY_DEBUG ${PROJECT_BINARY_DIR})

if(Windows)
  # Set linker option
  set_target_properties(
    WebRTCPlugin
    PROPERTIES
      LINK_FLAGS
      "-delayload:nvcuda.dll -delayload:nvEncodeAPI64.dll -delayload:nvcuvid.dll"
      MSVC_RUNTIME_LIBRARY "MultiThreaded$<$<CONFIG:Debug>:Debug>")
elseif(macOS)
  set_target_properties(WebRTCPlugin PROPERTIES LINK_FLAGS "-ObjC")
elseif(iOS)
  #
  # Build Setting Reference
  # https://developer.apple.com/library/archive/documentation/DeveloperTools/Reference/XcodeBuildSettingRef/1-Build_Setting_Reference/build_setting_ref.html
  #
  set_target_properties(
    WebRTCPlugin
    PROPERTIES LINK_FLAGS "-ObjC"
               MACOSX_FRAMEWORK_IDENTIFIER "com.unity.webrtc"
               MACOSX_FRAMEWORK_BUNDLE_VERSION ${CMAKE_PROJECT_VERSION}
               MACOSX_FRAMEWORK_SHORT_VERSION_STRING ${CMAKE_PROJECT_VERSION}
               XCODE_ATTRIBUTE_ENABLE_BITCODE "NO"
               XCODE_ATTRIBUTE_SKIP_INSTALL "NO"
               XCODE_ATTRIBUTE_INSTALL_PATH "@rpath"
               XCODE_ATTRIBUTE_STRIP_STYLE non-global
               XCODE_ATTRIBUTE_STRIP_INSTALLED_PRODUCT
               $<IF:$<CONFIG:Debug>,NO,YES>
               XCODE_ATTRIBUTE_GCC_SYMBOLS_PRIVATE_EXTERN
               $<IF:$<CONFIG:Debug>,NO,YES>)
endif()
