#!/bin/bash -eu

export LIBWEBRTC_DOWNLOAD_URL=https://github.com/Unity-Technologies/com.unity.webrtc/releases/download/M112/webrtc-linux.zip
export SOLUTION_DIR=$(pwd)/Plugin~

# Download LibWebRTC 
curl -L $LIBWEBRTC_DOWNLOAD_URL > webrtc.zip
unzip -d $SOLUTION_DIR/webrtc webrtc.zip 

# Build UnityRenderStreaming Plugin 
pushd "$SOLUTION_DIR"
cmake --preset=x86_64-linux
cmake --build --preset=debug-linux --target WebRTCLibTest
popd

# Copy test runner
cp "$SOLUTION_DIR/out/build/x86_64-linux/WebRTCPluginTest/Debug/WebRTCLibTest" .