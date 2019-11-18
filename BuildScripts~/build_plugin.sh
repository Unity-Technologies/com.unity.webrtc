#!/bin/bash

export LIBWEBRTC_DOWNLOAD_URL=https://github.com/Unity-Technologies/com.unity.webrtc/releases/download/M72/webrtc-linux.zip
export SOLUTION_DIR=$(pwd)/Plugin~

# Download LibWebRTC 

curl -L $LIBWEBRTC_DOWNLOAD_URL > webrtc.zip
unzip -d $SOLUTION_DIR/webrtc webrtc.zip 

# Install libc++, libc++abi libgtest
# TODO:: Remove this install process from here and recreate an image to build the plugin.
sudo apt install -y libc++-dev libc++abi-dev

# Install glew static library
wget https://downloads.sourceforge.net/glew/glew-2.1.0.tgz
tar -xvzf glew-2.1.0.tgz
cd $SOLUTION_DIR/glew-2.1.0
make glew.lib.static
find include -name "*.h" -print | cpio -pd "$SOLUTION_DIR/glew"
find lib -name "*.a" -print | cpio -pd "$SOLUTION_DIR/glew"

# Install googletest
sudo apt install -y googletest=1.8.0-6
cd /usr/src/googletest
sudo patch "googletest/cmake/internal_utils.cmake" < "$SOLUTION_DIR/BuildScripts~/gtest/internal_utils.cmake.patch"
sudo patch "googletest/CMakeLists.txt"             < "$SOLUTION_DIR/BuildScripts~/gtest/CMakeLists.txt.patch"
sudo cmake -Dcxx_no_rtti=ON \
           -DCMAKE_CXX_FLAGS=-nostdinc++ \
           .
sudo make
sudo cp googletest/*.a /usr/lib
sudo cp googlemock/*.a /usr/lib

# Build UnityRenderStreaming Plugin 
cd $SOLUTION_DIR
cmake .
make