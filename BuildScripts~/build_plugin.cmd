@echo off

set LIBWEBRTC_DOWNLOAD_URL=https://github.com/Unity-Technologies/com.unity.webrtc/releases/download/M83/webrtc-win.zip
set SOLUTION_DIR=%cd%\Plugin~

echo -------------------
echo Download LibWebRTC 

curl -L %LIBWEBRTC_DOWNLOAD_URL% > webrtc.zip
7z x -aoa webrtc.zip -o%SOLUTION_DIR%\webrtc

echo -------------------
echo Install nuget
choco install nuget.commandline

echo -------------------
echo Install nuget packages
nuget restore %SOLUTION_DIR%\WebRTCPlugin.sln
if not %errorlevel% == 0 exit 1

echo -------------------
echo Build com.unity.webrtc Plugin 

MSBuild %SOLUTION_DIR%\WebRTCPlugin.sln -t:Rebuild -p:Configuration=Release
if not %errorlevel% == 0 exit 1

echo -------------------
echo Test com.unity.webrtc Plugin 

%SOLUTION_DIR%\x64\Release\WebRTCPluginTest.exe
if not %errorlevel% == 0 exit 1
echo -------------------
