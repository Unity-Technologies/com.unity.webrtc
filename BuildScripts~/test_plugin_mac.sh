#!/bin/bash -eu

export LIBWEBRTC_DOWNLOAD_URL=https://github.com/Unity-Technologies/com.unity.webrtc/releases/download/M89/webrtc-mac.zip
export SOLUTION_DIR=$(pwd)/Plugin~

# Download LibWebRTC 
curl -L $LIBWEBRTC_DOWNLOAD_URL > webrtc.zip
unzip -d $SOLUTION_DIR/webrtc webrtc.zip

# Build UnityRenderStreaming Plugin 
cd "$SOLUTION_DIR"
cmake . \
  -G Xcode \
  -B build

cmake \
  --build build \
  --config Debug \
  --target WebRTCLibTest

# Copy and run the test on the Metal device
scp -i ~/.ssh/id_rsa_macmini -o "StrictHostKeyChecking=no" -r "$SOLUTION_DIR/build/WebRTCPluginTest/Debug" bokken@$BOKKEN_DEVICE_IP:~/com.unity.webrtc
ssh -i ~/.ssh/id_rsa_macmini -o "StrictHostKeyChecking=no" bokken@$BOKKEN_DEVICE_IP '~/com.unity.webrtc/WebRTCLibTest'