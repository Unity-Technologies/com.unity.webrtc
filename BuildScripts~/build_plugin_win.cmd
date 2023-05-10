@echo off

set LIBWEBRTC_DOWNLOAD_URL=https://github.com/Unity-Technologies/com.unity.webrtc/releases/download/M112/webrtc-win.zip
set SOLUTION_DIR=%cd%\Plugin~

echo -------------------
echo Download LibWebRTC 

curl -L %LIBWEBRTC_DOWNLOAD_URL% > webrtc.zip
7z x -aoa webrtc.zip -o%SOLUTION_DIR%\webrtc

echo -------------------
echo Build com.unity.webrtc Plugin

cd %SOLUTION_DIR%
cmake --preset=x64-windows-clang
cmake --build --preset=release-windows-clang --target=WebRTCPlugin