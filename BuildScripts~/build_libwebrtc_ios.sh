#!/bin/bash -eu

if [ ! -e "$(pwd)/depot_tools" ]
then
  git clone --depth 1 https://chromium.googlesource.com/chromium/tools/depot_tools.git
fi

export COMMAND_DIR=$(cd $(dirname $0); pwd)
export PATH="$(pwd)/depot_tools:$(pwd)/depot_tools/python-bin:$PATH"
export WEBRTC_VERSION=6367
export OUTPUT_DIR="$(pwd)/out"
export ARTIFACTS_DIR="$(pwd)/artifacts"
export PYTHON3_BIN="$(pwd)/depot_tools/python-bin/python3"

if [ ! -e "$(pwd)/src" ]
then
  fetch --nohooks webrtc_ios
  cd src
  sudo sh -c 'echo 127.0.1.1 $(hostname) >> /etc/hosts'
  sudo git config --system core.longpaths true
  git checkout "refs/remotes/branch-heads/$WEBRTC_VERSION"
  cd ..
  gclient sync -D --force --reset
else
  # fetch and init config on only first time
  cd src
  git checkout "refs/remotes/branch-heads/$WEBRTC_VERSION"
  cd ..
  gclient sync -D --force --reset
fi

# add jsoncpp
patch -N "src/BUILD.gn" < "$COMMAND_DIR/patches/add_jsoncpp.patch"

# disable GCD taskqueue, use stdlib taskqueue instead
# This is because GCD cannot measure with UnityProfiler
# patch -N "src/api/task_queue/BUILD.gn" < "$COMMAND_DIR/patches/disable_task_queue_gcd.patch"

# add objc library to use videotoolbox
patch -N "src/sdk/BUILD.gn" < "$COMMAND_DIR/patches/add_objc_deps.patch"

# Fix SetRawImagePlanes() in LibvpxVp8Encoder
patch -N "src/modules/video_coding/codecs/vp8/libvpx_vp8_encoder.cc" < "$COMMAND_DIR/patches/libvpx_vp8_encoder.patch"

mkdir -p "$ARTIFACTS_DIR/lib"

outputDir=""

for is_debug in "true" "false"
do
  for target_cpu in "arm64" "x64"
  do
    outputDir="${OUTPUT_DIR}_${is_debug}_${target_cpu}"
    # generate ninja files
    # 
    # note: `treat_warnings_as_errors=false` is for avoiding LLVM warning.
    #       https://reviews.llvm.org/D72212
    #       See below for details.
    #       https://bugs.chromium.org/p/webrtc/issues/detail?id=11729
    #
    gn gen "$outputDir" --root="src" \
      --args=" \
      is_debug=${is_debug} \
      is_component_build=false \
      ios_enable_code_signing=false \
      target_os=\"ios\" \
      target_cpu=\"${target_cpu}\" \
      treat_warnings_as_errors=false \
      rtc_use_h264=false \
      rtc_include_tests=false \
      rtc_build_examples=false \
      rtc_build_tools=false \
      use_custom_libcxx=false"
      
    # build static library
    ninja -C "$outputDir" webrtc

    # copy static library
    mkdir -p "$ARTIFACTS_DIR/lib/${target_cpu}"
    cp "$outputDir/obj/libwebrtc.a" "$ARTIFACTS_DIR/lib/${target_cpu}/"
  done

  filename="libwebrtc.a"
  if [ $is_debug = "true" ]; then
    filename="libwebrtcd.a"
  fi

  # make universal binary
  lipo -create -output                   \
  "$ARTIFACTS_DIR/lib/${filename}"       \
  "$ARTIFACTS_DIR/lib/arm64/libwebrtc.a" \
  "$ARTIFACTS_DIR/lib/x64/libwebrtc.a"
  
  rm -r "$ARTIFACTS_DIR/lib/arm64"
  rm -r "$ARTIFACTS_DIR/lib/x64"
done

"$PYTHON3_BIN" "./src/tools_webrtc/libs/generate_licenses.py" \
  --target :webrtc "$outputDir" "$outputDir"

cd src
find . -name "*.h" -print | cpio -pd "$ARTIFACTS_DIR/include"

cp "$outputDir/LICENSE.md" "$ARTIFACTS_DIR"

# create zip
cd "$ARTIFACTS_DIR"
zip -r webrtc-ios.zip lib include LICENSE.md
