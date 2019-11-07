file(GLOB sources *.cpp *.h)

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
elseif(APPLE OR UNIX)
  message(STATUS "macos or linux")
  target_compile_options(webrtc PRIVATE -Wall -Wextra -Wno-unknown-pragmas -Wno-unused-parameter -Wno-missing-field-initializers -Wno-long-long)
  target_compile_features(webrtc PRIVATE cxx_std_14)
endif()

if(APPLE)
  target_compile_definitions(webrtc 
    PRIVATE
      WEBRTC_MAC
      WEBRTC_POSIX
  )
  target_link_libraries(webrtc
     PRIVATE
     ${WEBRTC_LIBRARIES}
     ${GLEW_LIBRARIES}
     ${OPENGL_LIBRARIES}
     ${FRAMEWORK_LIBS}
  )
elseif(UNIX)
  set(CMAKE_C_COMPILER_ID Clang)
  set(CMAKE_CXX_COMPILER_ID Clang)
  set(CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS} -fno-lto -fno-rtti")
  set(CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS} -nostdinc++")

  target_compile_definitions(webrtc
    PRIVATE
      WEBRTC_LINUX
      WEBRTC_POSIX
  )
  target_link_libraries(webrtc
    PRIVATE
    ${WEBRTC_LIBRARIES}
    ${GLFW_STATIC_LIBRARIES}
    ${OPENGL_LIBRARIES}
    ${LIBCXX_LIBRARIES}
  )
endif()

target_include_directories(webrtc
  PRIVATE
    ${CMAKE_SOURCE_DIR}/unity/include
    ${WEBRTC_INCLUDE_DIR}
    ${OPENGL_INCLUDE_DIR}
    ${GLEW_INCLUDE_DIRS}
    ${LIBCXX_INCLUDE_DIRS}
)