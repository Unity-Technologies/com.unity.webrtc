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
cmake . -G "Visual Studio 16 2019" -A x64 -B "build64"
cmake --build build64 --config Debug --target WebRTCLibTest

echo -------------------
echo Test com.unity.webrtc Plugin

%SOLUTION_DIR%\build64\WebRTCPluginTest\Debug\WebRTCLibTest.exe
if not %errorlevel% == 0 exit 1
echo -------------------