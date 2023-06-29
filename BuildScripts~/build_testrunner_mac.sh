#!/bin/bash -eu

export LIBWEBRTC_DOWNLOAD_URL=https://github.com/Unity-Technologies/com.unity.webrtc/releases/download/M112/webrtc-mac.zip
export SOLUTION_DIR=$(pwd)/Plugin~

# Install cmake
export HOMEBREW_NO_AUTO_UPDATE=1
brew install cmake

# Download LibWebRTC 
curl -L $LIBWEBRTC_DOWNLOAD_URL > webrtc.zip
unzip -d $SOLUTION_DIR/webrtc webrtc.zip

# Build UnityRenderStreaming Plugin 
pushd "$SOLUTION_DIR"
cmake --preset=macos
cmake --build --preset=debug-macos --target=WebRTCLibTest
popd

# Copy test runner
cp "$SOLUTION_DIR/out/build/macos/WebRTCPluginTest/Debug/WebRTCLibTest" .