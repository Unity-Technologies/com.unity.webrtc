#!/bin/bash

export LIBWEBRTC_DOWNLOAD_URL=https://github.com/Unity-Technologies/com.unity.webrtc/releases/download/M85/webrtc-ios.zip
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
mkdir release
cd release
cmake .. -DCMAKE_BUILD_TYPE=Release
make
sudo make install

# Build UnityRenderStreaming Plugin 
cd "$SOLUTION_DIR"
cmake -G Xcode                                 \
  -D CMAKE_SYSTEM_NAME=iOS                     \
  -D "CMAKE_OSX_ARCHITECTURES=arm64;x86_64"    \
  -D CMAKE_OSX_DEPLOYMENT_TARGET=10.0          \
  -D CMAKE_INSTALL_PREFIX=`pwd`/_install       \
  -D CMAKE_XCODE_ATTRIBUTE_ONLY_ACTIVE_ARCH=NO \
  -D CMAKE_IOS_INSTALL_COMBINED=YES            \
  .

#xcodebuild -scheme webrtc -configuration Release build
#xcodebuild -scheme WebRTCPluginTest -configuration Release build
cmake --build . --config Release --target install


# Copy and run the test on the Metal device
scp -i ~/.ssh/id_rsa_macmini -o "StrictHostKeyChecking=no" -r "$SOLUTION_DIR/WebRTCPluginTest/Release" bokken@$BOKKEN_DEVICE_IP:~/com.unity.webrtc