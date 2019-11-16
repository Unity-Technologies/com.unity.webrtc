@echo off

set LIBWEBRTC_DOWNLOAD_URL=https://github.com/Unity-Technologies/com.unity.webrtc/releases/download/M72/webrtc-win.zip
set SOLUTION_DIR=%cd%\Plugin~

echo -------------------
echo Download LibWebRTC 

curl -L %LIBWEBRTC_DOWNLOAD_URL% > webrtc.zip
7z x -aoa webrtc.zip -o%SOLUTION_DIR%\webrtc

echo -------------------
echo Build com.unity.webrtc Plugin 

echo Uses the project file in the current folder by default
MSBuild %SOLUTION_DIR%\WebRTCPlugin.sln -t:Restore

MSBuild %SOLUTION_DIR%\WebRTCPlugin.sln -t:Rebuild -p:Configuration=Release
if not %errorlevel% == 0 exit 1
echo -------------------

echo -------------------
echo Test com.unity.webrtc Plugin 

%SOLUTION_DIR%\x65\Release\WebRTCPluginTest.exe
if not %errorlevel% == 0 exit 1
echo -------------------
