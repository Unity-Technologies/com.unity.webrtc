#!/bin/bash -eu

export LIBWEBRTC_DOWNLOAD_URL=https://github.com/Unity-Technologies/com.unity.webrtc/releases/download/M92/webrtc-linux.zip
export SOLUTION_DIR=$(pwd)/Plugin~
export LIBCXX_BUILD_DIR=$(pwd)/llvm-project/build

source ~/.profile

# Download LibWebRTC 
curl -L $LIBWEBRTC_DOWNLOAD_URL > webrtc.zip
unzip -d $SOLUTION_DIR/webrtc webrtc.zip 

# Install glfw3
sudo apt install -y libglfw3-dev

# Install glad2
pip3 install git+https://github.com/dav1dde/glad.git@glad2#egg=glad

# Make libc++ static library
sudo apt install ninja-build
git clone --depth 1 --branch release/13.x https://github.com/llvm/llvm-project.git
pushd llvm-project
mkdir build
cmake -G Ninja -S runtimes -B build -DCMAKE_C_COMPILER=clang-10 -DCMAKE_CXX_COMPILER=clang++-10 -DLLVM_ENABLE_PIC=ON -DLLVM_ENABLE_RUNTIMES="libcxx;libcxxabi"
ninja -C build cxx cxxabi
popd

# Build UnityRenderStreaming Plugin 
cd "$SOLUTION_DIR"
cmake . \
  -D CMAKE_C_COMPILER="clang-10" \
  -D CMAKE_CXX_COMPILER="clang++-10" \
  -D CMAKE_BUILD_TYPE="Release" \
  -D USE_CUSTOM_LIBCXX_STATIC=ON \
  -D CUSTOM_LIBCXX_DIR="$LIBCXX_BUILD_DIR" \
  -B build

cmake \
  --build build \
  --config Release \
  --target WebRTCPlugin
