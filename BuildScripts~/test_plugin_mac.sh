#!/bin/bash -eu

export LIBWEBRTC_DOWNLOAD_URL=https://github.com/Unity-Technologies/com.unity.webrtc/releases/download/M107/webrtc-mac.zip
export SOLUTION_DIR=$(pwd)/Plugin~

# Download LibWebRTC 
curl -L $LIBWEBRTC_DOWNLOAD_URL > webrtc.zip
unzip -d $SOLUTION_DIR/webrtc webrtc.zip

# Build UnityRenderStreaming Plugin 
cd "$SOLUTION_DIR"
cmake --preset=macos
cmake --build --preset=debug-macos --target=WebRTCLibTest

# Copy and run the test on the Metal device
scp -i ~/.ssh/id_rsa_macmini -o "StrictHostKeyChecking=no" -r "$SOLUTION_DIR/out/build/macos/WebRTCPluginTest/Debug" bokken@$BOKKEN_DEVICE_IP:~/com.unity.webrtc
ssh -i ~/.ssh/id_rsa_macmini -o "StrictHostKeyChecking=no" bokken@$BOKKEN_DEVICE_IP '~/com.unity.webrtc/WebRTCLibTest'