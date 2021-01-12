#!/bin/bash

export LIBWEBRTC_DOWNLOAD_URL=https://github.com/Unity-Technologies/com.unity.webrtc/releases/download/M85/webrtc-linux.zip
export SOLUTION_DIR=$(pwd)/Plugin~

# Download LibWebRTC 
curl -L $LIBWEBRTC_DOWNLOAD_URL > webrtc.zip
unzip -d $SOLUTION_DIR/webrtc webrtc.zip 

# Install libc++, libc++abi googletest clang glut
# TODO:: Remove this install process from here and recreate an image to build the plugin.
sudo apt update
sudo apt install -y libc++-dev libc++abi-dev clang freeglut3-dev

# Build UnityRenderStreaming Plugin 
cd "$SOLUTION_DIR"
cmake -DCMAKE_C_COMPILER="clang" \
      -DCMAKE_CXX_COMPILER="clang++" \
      -DCMAKE_BUILD_TYPE="Debug" \
      -DCMAKE_CXX_FLAGS="-stdlib=libc++" \
      -Dcxx_no_rtti=ON \
      -B "build" \
      .
cmake --build build --config Debug

# Run UnitTest
"$SOLUTION_DIR/build/WebRTCPluginTest/WebRTCLibTest"