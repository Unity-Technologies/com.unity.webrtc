#!/bin/bash

export LIBWEBRTC_DOWNLOAD_URL=https://github.com/Unity-Technologies/com.unity.webrtc/releases/download/M72/webrtc-linux.zip
export SOLUTION_DIR=$(pwd)/Plugin~

# Download LibWebRTC 

curl -L $LIBWEBRTC_DOWNLOAD_URL > webrtc.zip
unzip -d $SOLUTION_DIR/webrtc webrtc.zip 

# Install libc++ and libc++abi
# TODO:: Remove this install process from here and recreate an image to build the plugin.
sudo apt install -y libc++-dev libc++abi-dev

# Build UnityRenderStreaming Plugin 
cd $SOLUTION_DIR
cmake .
make