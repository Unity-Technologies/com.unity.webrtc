#!/bin/bash

export LIBWEBRTC_DOWNLOAD_URL=https://github.com/karasusan/build-webrtc/releases/download/v0.1.0/webrtc-linux.zip
export SOLUTION_DIR=$(pwd)/Plugin~

# Download LibWebRTC 

curl -L $LIBWEBRTC_DOWNLOAD_URL > webrtc.zip
unzip -d $SOLUTION_DIR/webrtc webrtc.zip 

# Build UnityRenderStreaming Plugin 

cd $SOLUTION_DIR
cmake .