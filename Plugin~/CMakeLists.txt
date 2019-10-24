cmake_minimum_required(VERSION 3.10)
enable_language(CXX)
set(CMAKE_CXX_STANDARD 14)
set(CMAKE_CXX_STANDARD_REQUIRED ON)
set(CMAKE_CXX_EXTENSIONS OFF)
find_package(Threads REQUIRED)
find_package(GLEW REQUIRED)

project(webrtc)

list(APPEND CMAKE_MODULE_PATH "${CMAKE_SOURCE_DIR}/cmake")
find_package(WebRTC REQUIRED)
include(AddPlugin)

add_subdirectory(WebRTCPlugin)