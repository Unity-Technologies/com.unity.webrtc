@echo off

set LIBWEBRTC_DOWNLOAD_URL=https://github.com/Unity-Technologies/com.unity.webrtc/releases/download/M89/webrtc-win.zip
set SOLUTION_DIR=%cd%\Plugin~

echo -------------------
echo Download LibWebRTC 

curl -L %LIBWEBRTC_DOWNLOAD_URL% > webrtc.zip
7z x -aoa webrtc.zip -o%SOLUTION_DIR%\webrtc

echo -------------------
echo Build com.unity.webrtc Plugin

cd %SOLUTION_DIR%
cmake . -G "Visual Studio 15 2017" -A x64 -B "build"
cmake --build build --config Debug

echo -------------------
echo Test com.unity.webrtc Plugin

%SOLUTION_DIR%\build\WebRTCPluginTest\Debug\WebRTCLibTest.exe
if not %errorlevel% == 0 exit 1
echo -------------------