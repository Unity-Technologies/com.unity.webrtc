#!/bin/bash -eu

export LIBWEBRTC_DOWNLOAD_URL=https://github.com/Unity-Technologies/com.unity.webrtc/releases/download/M92/webrtc-linux.zip
export SOLUTION_DIR=$(pwd)/Plugin~

# See False positives
# https://github.com/google/sanitizers/wiki/AddressSanitizerContainerOverflow
export ASAN_OPTIONS=protect_shadow_gap=0:detect_leaks=1:detect_container_overflow=0
export LSAN_OPTIONS=suppressions=$(pwd)/Plugin~/tools/sanitizer/lsan_suppressions.txt

source ~/.profile

# Download LibWebRTC 
curl -L $LIBWEBRTC_DOWNLOAD_URL > webrtc.zip
unzip -d $SOLUTION_DIR/webrtc webrtc.zip 

# Install libc++, libc++abi googletest clang glut
# TODO:: Remove this install process from here and recreate an image to build the plugin.
sudo apt update
sudo apt install -y clang-10 libc++-10-dev libc++abi-10-dev freeglut3-dev

# Install glad2
pip3 install git+https://github.com/dav1dde/glad.git@glad2#egg=glad

# Build UnityRenderStreaming Plugin 
cd "$SOLUTION_DIR"
cmake . \
  -D CMAKE_C_COMPILER="clang-10" \
  -D CMAKE_CXX_COMPILER="clang++-10" \
  -D CMAKE_BUILD_TYPE="Debug" \
  -D CMAKE_CXX_FLAGS="-stdlib=libc++" \
  -D cxx_no_rtti=ON \
  -B build

cmake \
  --build build \
  --config Debug \
  --target WebRTCLibTest

# Run UnitTest
"$SOLUTION_DIR/build/WebRTCPluginTest/WebRTCLibTest"