@echo off

set LIBWEBRTC_DOWNLOAD_URL=https://github.com/Unity-Technologies/com.unity.webrtc/releases/download/M72/webrtc-win.zip
set SOLUTION_DIR=%cd%\Plugin~

echo -------------------
echo Download LibWebRTC 

curl -L %LIBWEBRTC_DOWNLOAD_URL% > webrtc.zip
7z x -aoa webrtc.zip -o%SOLUTION_DIR%\webrtc

echo -------------------
echo Build UnityRenderStreaming Plugin 

MSBuild %SOLUTION_DIR%\UnityRenderStreamingPlugin.sln -t:Rebuild -p:Configuration=Release
if not %errorlevel% == 0 exit 1
echo -------------------