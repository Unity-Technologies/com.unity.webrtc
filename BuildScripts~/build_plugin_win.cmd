@echo off

set LIBWEBRTC_DOWNLOAD_URL=https://github.com/Unity-Technologies/com.unity.webrtc/releases/download/M116/webrtc-win.zip
set SOLUTION_DIR=%cd%\Plugin~

echo Download LibWebRTC

if not exist %SOLUTION_DIR%\webrtc (
  curl -L %LIBWEBRTC_DOWNLOAD_URL% > webrtc.zip
  7z x -aoa webrtc.zip -o%SOLUTION_DIR%\webrtc
)

echo Build com.unity.webrtc Plugin

rem CMake doesn't support building CUDA kernel with Clang compiler on Windows. 
rem https://gitlab.kitware.com/cmake/cmake/-/issues/20776
rem This program use CUDA kernel to change the video resolution when using NVIDIA Video Codec.

cd %SOLUTION_DIR%
cmake --preset=x64-windows-msvc
cmake --build --preset=release-windows-msvc --target=WebRTCPlugin