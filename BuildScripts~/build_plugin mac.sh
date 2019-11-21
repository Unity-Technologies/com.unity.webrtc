#!/bin/bash

export LIBWEBRTC_DOWNLOAD_URL=https://github.com/Unity-Technologies/com.unity.webrtc/releases/download/M72/webrtc-mac.zip
export SOLUTION_DIR=$(pwd)/Plugin~

# Download LibWebRTC 
curl -L $LIBWEBRTC_DOWNLOAD_URL > webrtc.zip
unzip -d $SOLUTION_DIR/webrtc webrtc.zip 

# Install googletest
git clone https://github.com/google/googletest.git
cd /usr/src/googletest
sudo cmake -Dcxx_no_rtti=ON \
           -DCMAKE_C_COMPILER="clang" \
           -DCMAKE_CXX_COMPILER="clang++" \
           -DCMAKE_C_FLAGS="-x objective-c" \
           -DCMAKE_CXX_FLAGS="-stdlib=libc++ -x objective-c" \
           CMakeLists.txt
sudo make
sudo cp googlemock/*.a "/usr/lib"
sudo cp googlemock/gtest/*.a "/usr/lib"

# Build UnityRenderStreaming Plugin 
cd "$SOLUTION_DIR"
cmake -GXcode .
xcodebuild -scheme webrtc -configuration Release build
xcodebuild -scheme WebRTCPluginTest -configuration Release build

# Run UnitTest
"$SOLUTION_DIR/WebRTCPluginTest/Release/WebRTCPluginTest"