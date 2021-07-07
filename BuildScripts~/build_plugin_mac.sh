#!/bin/bash -eu

export LIBWEBRTC_DOWNLOAD_URL=https://github.com/Unity-Technologies/com.unity.webrtc/releases/download/M89/webrtc-mac.zip
export SOLUTION_DIR=$(pwd)/Plugin~
export BUNDLE_FILE=$(pwd)/Runtime/Plugins/macOS/webrtc.bundle

# Install cmake
export HOMEBREW_NO_AUTO_UPDATE=1
brew install cmake

# Download LibWebRTC 
curl -L $LIBWEBRTC_DOWNLOAD_URL > webrtc.zip
unzip -d $SOLUTION_DIR/webrtc webrtc.zip 

# Remove old bundle file
rm -r "$BUNDLE_FILE"

# Build UnityRenderStreaming Plugin
cd "$SOLUTION_DIR"
cmake . \
  -G Xcode \
  -D "CMAKE_OSX_ARCHITECTURES=arm64;x86_64" \
  -B build

cmake \
  --build build \
  --config Release \
  --target WebRTCPlugin