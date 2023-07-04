@echo off

set LIBWEBRTC_DOWNLOAD_URL=https://github.com/Unity-Technologies/com.unity.webrtc/releases/download/M112/webrtc-win.zip
set SOLUTION_DIR=%cd%\Plugin~

echo -------------------
echo Download LibWebRTC 

if not exist %SOLUTION_DIR%\webrtc (
  curl -L %LIBWEBRTC_DOWNLOAD_URL% > webrtc.zip
  7z x -aoa webrtc.zip -o%SOLUTION_DIR%\webrtc
)

echo -------------------
echo Build com.unity.webrtc Plugin

pushd %SOLUTION_DIR%
cmake --preset=x64-windows-msvc
cmake --build --preset=debug-windows-msvc --target=WebRTCLibTest
popd

echo -------------------
echo Copy test runner

copy %SOLUTION_DIR%\out\build\x64-windows-msvc\WebRTCPluginTest\Debug\WebRTCLibTest.exe .