#!/bin/bash

export LIBWEBRTC_DOWNLOAD_URL=https://github.com/Unity-Technologies/com.unity.webrtc/releases/download/M79/webrtc-mac.zip
export SOLUTION_DIR=$(pwd)/Plugin~

# Install cmake
brew install cmake

# Download LibWebRTC 
curl -L $LIBWEBRTC_DOWNLOAD_URL > webrtc.zip
unzip -d $SOLUTION_DIR/webrtc webrtc.zip 

# Install googletest
git clone https://github.com/google/googletest.git
cd googletest
git checkout 6f5fd0d7199b9a19faa9f499ecc266e6ae0329e7
cmake .
make
sudo make install

# Build UnityRenderStreaming Plugin 
cd "$SOLUTION_DIR"
cmake -GXcode .
xcodebuild -scheme webrtc -configuration Release build
xcodebuild -scheme WebRTCPluginTest -configuration Release build

# Copy and run the test on the Metal device
scp -i ~/.ssh/id_rsa_macmini -o "StrictHostKeyChecking=no" -r "$SOLUTION_DIR/WebRTCPluginTest/Release" bokken@$BOKKEN_DEVICE_IP:~/com.unity.webrtc
ssh -i ~/.ssh/id_rsa_macmini -o "StrictHostKeyChecking=no" bokken@$BOKKEN_DEVICE_IP '~/com.unity.webrtc/WebRTCPluginTest'

# Running test locally. Left as a reference
#"$SOLUTION_DIR/WebRTCPluginTest/Release/WebRTCPluginTest"
