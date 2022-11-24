#!/bin/bash -eu

export LIBWEBRTC_DOWNLOAD_URL=https://github.com/Unity-Technologies/com.unity.webrtc/releases/download/M107/webrtc-linux.zip
export SOLUTION_DIR=$(pwd)/Plugin~

# See False positives
# https://github.com/google/sanitizers/wiki/AddressSanitizerContainerOverflow
export ASAN_OPTIONS=protect_shadow_gap=0:detect_leaks=1:detect_container_overflow=0
export LSAN_OPTIONS=suppressions=$(pwd)/Plugin~/tools/sanitizer/lsan_suppressions.txt

source ~/.profile

# Download LibWebRTC 
curl -L $LIBWEBRTC_DOWNLOAD_URL > webrtc.zip
unzip -d $SOLUTION_DIR/webrtc webrtc.zip 

# Install glfw3 ninja-build
sudo apt install -y libglfw3-dev ninja-build

# Install glad2
pip3 install git+https://github.com/dav1dde/glad.git@glad2#egg=glad2

# Build UnityRenderStreaming Plugin 
cd "$SOLUTION_DIR"
cmake --preset=x86_64-linux
cmake --build --preset=debug-linux --target WebRTCLibTest

# Run UnitTest
"$SOLUTION_DIR/out/build/x86_64-linux/WebRTCPluginTest/Debug/WebRTCLibTest"