#!/bin/bash

export LIBWEBRTC_DOWNLOAD_URL=https://github.com/Unity-Technologies/com.unity.webrtc/releases/download/M72/webrtc-linux.zip
export SOLUTION_DIR=$(pwd)/Plugin~

# Download LibWebRTC 

curl -L $LIBWEBRTC_DOWNLOAD_URL > webrtc.zip
unzip -d $SOLUTION_DIR/webrtc webrtc.zip 

# Install libc++, libc++abi libgtest
# TODO:: Remove this install process from here and recreate an image to build the plugin.
sudo apt install -y libc++-dev libc++abi-dev libgtest-dev

# Install glew static library
wget https://downloads.sourceforge.net/glew/glew-2.1.0.tgz
tar -xvzf glew-2.1.0.tgz
cd $SOLUTION_DIR/glew-2.1.0
make glew.lib.static
find include -name "*.h" -print | cpio -pd "$SOLUTION_DIR/glew"
find lib -name "*.a" -print | cpio -pd "$SOLUTION_DIR/glew"

# Install googletest
cd /usr/src/gtest
sudo cmake CMakeLists.txt
sudo make
sudo cp *.a /usr/lib

# Build UnityRenderStreaming Plugin 
cd $SOLUTION_DIR
cmake .
make