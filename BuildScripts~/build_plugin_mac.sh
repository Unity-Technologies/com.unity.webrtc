#!/bin/bash

export LIBWEBRTC_DOWNLOAD_URL=https://github.com/Unity-Technologies/com.unity.webrtc/releases/download/M85/webrtc-mac.zip
export SOLUTION_DIR=$(pwd)/Plugin~
export BUNDLE_FILE=$(pwd)/Runtime/Plugins/x86_64/webrtc.bundle

# Install cmake
brew install cmake

# Download LibWebRTC 
curl -L $LIBWEBRTC_DOWNLOAD_URL > webrtc.zip
unzip -d $SOLUTION_DIR/webrtc webrtc.zip 

# Remove old bundle file
rm -r "$BUNDLE_FILE"

# Build UnityRenderStreaming Plugin
cd "$SOLUTION_DIR"
cmake . -G Xcode \
        -B build

cmake --build build         \
      --config Release       \
      --target WebRTCPlugin