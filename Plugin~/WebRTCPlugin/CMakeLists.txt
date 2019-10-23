file(GLOB sources *.cpp *.h)

add_plugin(webrtc
  SOURCES ${sources}
  PLUGINS_DIR ${CMAKE_INSTALL_PREFIX}/Runtime/Plugins/x86_64
)

if(MSVC)
  # Force to always compile with W4
  if(CMAKE_CXX_FLAGS MATCHES "/W[0-4]")
    string(REGEX REPLACE "/W[0-4]" "/W4" CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS}")
  else()
    set(CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS} /W4")
  endif()
elseif(APPLE OR UNIX)
  # Update if necessary
  set(CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS} -Wall -Wextra -Wno-long-long -pedantic")
  target_compile_definitions(webrtc 
    PRIVATE
      WEBRTC_POSIX
  )  
endif()

target_include_directories(webrtc
  PRIVATE
    ${CMAKE_SOURCE_DIR}/unity/include
    ${WEBRTC_INCLUDE_DIR}
    ${OPENGL_INCLUDE_DIR}
)

target_link_libraries(webrtc
  PRIVATE
    ${WEBRTC_LIBRARIES}
)

install(TARGETS webrtc DESTINATION ${CMAKE_INSTALL_PREFIX}/com.unity.webrtc/Runtime/Plugins/x86_64)