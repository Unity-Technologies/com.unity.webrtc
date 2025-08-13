#!/bin/bash -eu

export LIBWEBRTC_DOWNLOAD_URL=https://github.com/Unity-Technologies/com.unity.webrtc/releases/download/M116-20250805/webrtc-mac.zip
export SOLUTION_DIR=$(pwd)/Plugin~
export DYLIB_FILE=$(pwd)/Runtime/Plugins/macOS/libwebrtc.dylib

BUILD_TYPE="${1:-release}"
if [ "$BUILD_TYPE" = "debug" ]; then
  CMAKE_BUILD_TYPE="Debug"
else
  CMAKE_BUILD_TYPE="Release"
fi

# Install cmake
export HOMEBREW_NO_AUTO_UPDATE=1
brew install cmake

# Download LibWebRTC 
curl -L $LIBWEBRTC_DOWNLOAD_URL > webrtc.zip
unzip -d $SOLUTION_DIR/webrtc webrtc.zip 

# Remove old dylib file
rm -rf "$DYLIB_FILE"

# Build UnityRenderStreaming Plugin
cd "$SOLUTION_DIR"
cmake --preset=macos -DCMAKE_BUILD_TYPE=$CMAKE_BUILD_TYPE
cmake --build --preset=release-macos --target=WebRTCPlugin
