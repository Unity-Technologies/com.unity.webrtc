file(GLOB sources *.cpp *.h)

add_plugin(webrtc
  SOURCES ${sources}
  PLUGINS_DIR ${CMAKE_INSTALL_PREFIX}/Runtime/Plugins/x86_64
)

target_include_directories(webrtc
  PRIVATE
    ../unity/include
    ${WEBRTC_INCLUDE_DIR}
)

target_link_libraries(webrtc
  PRIVATE
    ${WEBRTC_LIBRARIES}
)

install(TARGETS webrtc DESTINATION ${CMAKE_INSTALL_PREFIX}/com.unity.webrtc/Runtime/Plugins/x86_64)