#!/bin/bash -eu

export LIBWEBRTC_DOWNLOAD_URL=https://github.com/Unity-Technologies/com.unity.webrtc/releases/download/M112/webrtc-android.zip
export SOLUTION_DIR=$(pwd)/Plugin~
export PLUGIN_DIR=$(pwd)/Runtime/Plugins/Android
export ARCH_ABI=arm64-v8a

# Download LibWebRTC 
curl -L $LIBWEBRTC_DOWNLOAD_URL > webrtc.zip
unzip -d $SOLUTION_DIR/webrtc webrtc.zip 

# Build UnityRenderStreaming Plugin 
cd "$SOLUTION_DIR"
cmake . \
  -B build \
  -D CMAKE_SYSTEM_NAME=Android \
  -D CMAKE_ANDROID_API_MIN=24 \
  -D CMAKE_ANDROID_API=24 \
  -D CMAKE_ANDROID_ARCH_ABI=$ARCH_ABI \
  -D CMAKE_ANDROID_NDK=$ANDROID_NDK \
  -D CMAKE_BUILD_TYPE=Release \
  -D CMAKE_ANDROID_STL_TYPE=c++_static

cmake \
  --build build \
  --target WebRTCPlugin

# libwebrtc.so move into libwebrtc.aar
cp -f $SOLUTION_DIR/webrtc/lib/libwebrtc.aar $PLUGIN_DIR
pushd $PLUGIN_DIR
mkdir -p jni/$ARCH_ABI
mv libwebrtc.so jni/$ARCH_ABI
zip -g libwebrtc.aar jni/$ARCH_ABI/libwebrtc.so
rm -r jni
popd