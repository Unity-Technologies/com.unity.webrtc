#!/bin/bash

export LIBWEBRTC_DOWNLOAD_URL=https://github.com/Unity-Technologies/com.unity.webrtc/releases/download/M85/webrtc-mac.zip
export SOLUTION_DIR=$(pwd)/Plugin~

# Install cmake
brew install cmake

# Download LibWebRTC 
curl -L $LIBWEBRTC_DOWNLOAD_URL > webrtc.zip
unzip -d $SOLUTION_DIR/webrtc webrtc.zip 

# Install googletest
git clone https://github.com/google/googletest.git
cd googletest
git checkout 2fe3bd994b3189899d93f1d5a881e725e046fdc2

cmake .
  -G Xcode                                     \
  -D CMAKE_SYSTEM_NAME=iOS                     \
  -D "CMAKE_OSX_ARCHITECTURES=arm64;x86_64"    \
  -D CMAKE_XCODE_ATTRIBUTE_ONLY_ACTIVE_ARCH=NO \
  -D CMAKE_XCODE_ATTRIBUTE_ENABLE_BITCODE=YES  \
  -D CMAKE_IOS_INSTALL_COMBINED=YES            \
  -D CMAKE_INSTALL_PREFIX=.                    \
  -B build

cmake --build build --config Debug --target install

# Build webrtc Unity plugin for test
cd "$SOLUTION_DIR"
cmake -G Xcode                                 \
  -D CMAKE_SYSTEM_NAME=iOS                     \
  -D "CMAKE_OSX_ARCHITECTURES=arm64;x86_64"    \
  -D CMAKE_XCODE_ATTRIBUTE_ONLY_ACTIVE_ARCH=NO \
  -D CMAKE_XCODE_ATTRIBUTE_ENABLE_BITCODE=YES  \
  .

xcodebuild build-for-testing      \
  -project build/webrtc.xcodeproj \
  -target WebRTCPluginTest        \
  -configuration Debug

# Copy and run the test on the Metal device
scp -i ~/.ssh/id_rsa_macmini -o "StrictHostKeyChecking=no" -r "$SOLUTION_DIR/WebRTCPluginTest/Release" bokken@$BOKKEN_DEVICE_IP:~/com.unity.webrtc
ssh -i ~/.ssh/id_rsa_macmini -o "StrictHostKeyChecking=no" bokken@$BOKKEN_DEVICE_IP '~/com.unity.webrtc/WebRTCPluginTest'

# Running test locally. Left as a reference
# "$SOLUTION_DIR/WebRTCPluginTest/Release/WebRTCPluginTest"
