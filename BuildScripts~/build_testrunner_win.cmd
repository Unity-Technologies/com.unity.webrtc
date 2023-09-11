@echo off

set LIBWEBRTC_DOWNLOAD_URL=https://github.com/Unity-Technologies/com.unity.webrtc/releases/download/M116/webrtc-win.zip
set SOLUTION_DIR=%cd%\Plugin~

echo -------------------
echo Download LibWebRTC 

if not exist %SOLUTION_DIR%\webrtc (
  curl -L %LIBWEBRTC_DOWNLOAD_URL% > webrtc.zip
  7z x -aoa webrtc.zip -o%SOLUTION_DIR%\webrtc
)

echo -------------------
echo Build com.unity.webrtc Plugin

rem Debug build with MSVC compiler crashes. Release build works fine.
rem This issue is reported on the webrtc forum.
rem https://groups.google.com/g/discuss-webrtc/c/N7L6fx784GA

pushd %SOLUTION_DIR%
cmake --preset=x64-windows-msvc
cmake --build --preset=release-windows-msvc --target=WebRTCLibTest
popd

echo -------------------
echo Copy test runner

copy %SOLUTION_DIR%\out\build\x64-windows-msvc\WebRTCPluginTest\Release\WebRTCLibTest.exe .