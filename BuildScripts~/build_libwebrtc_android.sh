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
  # Exclude example for reduction
  patch -N "depot_tools/fetch_configs/webrtc.py" < "$COMMAND_DIR/patches/fetch_exclude_examples.patch"
  fetch --nohooks webrtc_android
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

# Add jsoncpp
patch -N "src/BUILD.gn" < "$COMMAND_DIR/patches/add_jsoncpp.patch"

# Add -mno-outline-atomics flag
patch -N "src/build/config/compiler/BUILD.gn" < "$COMMAND_DIR/patches/add_nooutlineatomics_flag.patch"

# Fix SetRawImagePlanes() in LibvpxVp8Encoder
patch -N "src/modules/video_coding/codecs/vp8/libvpx_vp8_encoder.cc" < "$COMMAND_DIR/patches/libvpx_vp8_encoder.patch"

pushd src
# Fix AdaptedVideoTrackSource::video_adapter()
patch -p1 < "$COMMAND_DIR/patches/fix_adaptedvideotracksource.patch"
# Fix Android video encoder 
patch -p1 < "$COMMAND_DIR/patches/fix_android_videoencoder.patch"
popd

mkdir -p "$ARTIFACTS_DIR/lib"

outputDir=""

for target_cpu in "arm64" "x64"
do
  mkdir -p "$ARTIFACTS_DIR/lib/${target_cpu}"
  for is_debug in "true" "false"
  do
    outputDir="${OUTPUT_DIR}_${is_debug}_${target_cpu}"
    # generate ninja files
    # use `treat_warnings_as_errors` option to avoid deprecation warnings
    gn gen "$outputDir" --root="src" \
      --args=" \
      exclude_unwind_tables=true \
      is_component_build=false \
      is_debug=${is_debug} \
      is_java_debug=${is_debug} \
      rtc_use_h264=false \
      rtc_include_tests=false \
      rtc_build_examples=false \
      rtc_build_tools=false \
      target_os=\"android\" \
      target_cpu=\"${target_cpu}\" \
      treat_warnings_as_errors=false \
      use_rtti=true \
      use_errorprone_java_compiler=false"

    # build static library
    ninja -C "$outputDir" webrtc

    filename="libwebrtc.a"
    if [ $is_debug = "true" ]; then
      filename="libwebrtcd.a"
    fi

    # copy static library
    cp "$outputDir/obj/libwebrtc.a" "$ARTIFACTS_DIR/lib/${target_cpu}/${filename}"
  done
done

pushd src

for is_debug in "true" "false"
do
  aarOutputDir="${OUTPUT_DIR}_${is_debug}_aar"
  # use `treat_warnings_as_errors` option to avoid deprecation warnings
  "$PYTHON3_BIN" tools_webrtc/android/build_aar.py \
    --build-dir $aarOutputDir \
    --output $aarOutputDir/libwebrtc.aar \
    --arch arm64-v8a x86_64 \
    --extra-gn-args " \
      is_debug=${is_debug} \
      is_java_debug=${is_debug} \
      is_component_build=false \
      rtc_use_h264=false \
      rtc_include_tests=false \
      rtc_build_examples=false \
      rtc_build_tools=false \
      treat_warnings_as_errors=false \
      use_rtti=true \
      use_errorprone_java_compiler=false"

  filename="libwebrtc.aar"
  if [ $is_debug = "true" ]; then
    filename="libwebrtc-debug.aar"
  fi
  # copy aar
  cp "$aarOutputDir/libwebrtc.aar" "$ARTIFACTS_DIR/lib/${filename}"
done

popd

"$PYTHON3_BIN" "./src/tools_webrtc/libs/generate_licenses.py" \
  --target :webrtc "$outputDir" "$outputDir"

cd src
find . -name "*.h" -print | cpio -pd "$ARTIFACTS_DIR/include"

cp "$outputDir/LICENSE.md" "$ARTIFACTS_DIR"

# create zip
cd "$ARTIFACTS_DIR"
zip -r webrtc-android.zip lib include LICENSE.md
