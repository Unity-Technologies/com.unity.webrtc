@echo off

set LIBWEBRTC_DOWNLOAD_URL=https://github.com/Unity-Technologies/com.unity.webrtc/releases/download/M83/webrtc-win.zip
set SOLUTION_DIR=%cd%\Plugin~

echo -------------------
echo Download LibWebRTC 

curl -L %LIBWEBRTC_DOWNLOAD_URL% > webrtc.zip
7z x -aoa webrtc.zip -o%SOLUTION_DIR%\webrtc

echo -------------------
echo Install googletest

git clone https://github.com/google/googletest.git
cd googletest
git checkout 2fe3bd994b3189899d93f1d5a881e725e046fdc2
cmake . -G "Visual Studio 15 2017" -A x64 -B "build64"
cmake --build build64 --config Release
mkdir include\gtest
xcopy /e googletest\include\gtest include\gtest
mkdir include\gmock
xcopy /e googlemock\include\gmock include\gmock
mkdir lib
xcopy /e build64\googlemock\Release lib
xcopy /e build64\googlemock\gtest\Release lib

echo -------------------
echo Build com.unity.webrtc Plugin 

cd %SOLUTION_DIR%
cmake . -G "Visual Studio 15 2017" -A x64
cmake --build . --config Release

echo -------------------
echo Move com.unity.webrtc Plugin

copy %SOLUTION_DIR%\WebRTCPlugin\Release\webrtc.dll %SOLUTION_DIR%\..\Runtime\Plugins\x86_64

echo -------------------
echo Test com.unity.webrtc Plugin 

%SOLUTION_DIR%\WebRTCPluginTest\Release\WebRTCPluginTest.exe
if not %errorlevel% == 0 exit 1
echo -------------------