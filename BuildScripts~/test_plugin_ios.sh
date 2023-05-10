#!/bin/bash -eu

export LIBWEBRTC_DOWNLOAD_URL=https://github.com/Unity-Technologies/com.unity.webrtc/releases/download/M112/webrtc-ios.zip
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

cmake .                                        \
  -G Xcode                                     \
  -D CMAKE_SYSTEM_NAME=iOS                     \
  -D "CMAKE_OSX_ARCHITECTURES=arm64;x86_64"    \
  -D CMAKE_XCODE_ATTRIBUTE_ONLY_ACTIVE_ARCH=NO \
  -D CMAKE_IOS_INSTALL_COMBINED=YES            \
  -D CMAKE_INSTALL_PREFIX=.                    \
  -B build

cmake --build build --config Debug --target install

# Build webrtc Unity plugin for test
cd "$SOLUTION_DIR"
cmake . \
  -G Xcode \
  -D CMAKE_SYSTEM_NAME=iOS \
  -D "CMAKE_OSX_ARCHITECTURES=arm64;x86_64" \
  -D CMAKE_XCODE_ATTRIBUTE_ONLY_ACTIVE_ARCH=NO \
  -B build

xcodebuild build \
  -sdk iphonesimulator \
  -project build/webrtc.xcodeproj \
  -scheme WebRTCLibTest \
  -configuration Debug

# todo(kazuki): testing app on the iOS simulator
#xcodebuild test                                               \
#  -project build/webrtc.xcodeproj                             \
#  -scheme WebRTCLibTest                                       \
#  -destination 'platform=iOS Simulator,name=iPhone 8,OS=13.3'
