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
cd $SOLUTION_DIR
wget https://downloads.sourceforge.net/glew/glew-2.1.0.tgz
tar -xvzf glew-2.1.0.tgz
cd $SOLUTION_DIR/glew-2.1.0
make glew.lib.static
find include -name "*.h" -print | cpio -pd "$SOLUTION_DIR/glew"
find lib -name "*.a" -print | cpio -pd "$SOLUTION_DIR/glew"

# Install googletest
wget https://github.com/google/googletest/archive/release-1.8.0.tar.gz
tar -zxvf release-1.8.0.tar.gz
cd googletest-release-1.8.0
patch "googletest/cmake/internal_utils.cmake" < "$SOLUTION_DIR/../BuildScripts~/gtest/internal_utils.cmake.patch"
patch "googletest/CMakeLists.txt"             < "$SOLUTION_DIR/../BuildScripts~/gtest/googletest_CMakeLists.txt.patch"
patch "googlemock/CMakeLists.txt"             < "$SOLUTION_DIR/../BuildScripts~/gtest/googlemock_CMakeLists.txt.patch"
cmake -Dcxx_no_rtti=ON \
      -DCMAKE_CXX_FLAGS=-nostdinc++ \
      -DCMAKE_C_COMPILER="clang" \
      -DCMAKE_CXX_COMPILER="clang++" \
      .
make
mkdir -p "$SOLUTION_DIR/gtest/lib"
cp googlemock/*.a "$SOLUTION_DIR/gtest/lib"
cp googlemock/gtest/*.a "$SOLUTION_DIR/gtest/lib"
cd googlemock; find include -name "*.h" -print | cpio -pd "$SOLUTION_DIR/gtest"; cd -;
cd googletest; find include -name "*.h" -print | cpio -pd "$SOLUTION_DIR/gtest"; cd -;

# Build UnityRenderStreaming Plugin 
cd $SOLUTION_DIR
cmake .
make