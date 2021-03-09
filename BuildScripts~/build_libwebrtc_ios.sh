#!/bin/bash -eu

if [ ! -e "$(pwd)/depot_tools" ]
then
  git clone --depth 1 https://chromium.googlesource.com/chromium/tools/depot_tools.git
fi

export COMMAND_DIR=$(cd $(dirname $0); pwd)
export PATH="$(pwd)/depot_tools:$PATH"
export WEBRTC_VERSION=4389
export OUTPUT_DIR="$(pwd)/out"
export ARTIFACTS_DIR="$(pwd)/artifacts"

if [ ! -e "$(pwd)/src" ]
then
  fetch --nohooks webrtc_ios
  cd src
  sudo sh -c 'echo 127.0.1.1 $(hostname) >> /etc/hosts'
  sudo git config --system core.longpaths true
  git checkout "refs/remotes/branch-heads/$WEBRTC_VERSION"
  cd ..
  gclient sync -f
fi

# add jsoncpp
patch -N "src/BUILD.gn" < "$COMMAND_DIR/patches/add_jsoncpp.patch"

# add objc library to use videotoolbox
patch -N "src/sdk/BUILD.gn" < "$COMMAND_DIR/patches/add_objc_deps.patch"

# use included python
export PATH="$(pwd)/depot_tools/bootstrap-3.8.0.chromium.8_bin/python/bin:$PATH"

mkdir -p "$ARTIFACTS_DIR/lib"

for is_debug in "true" "false"
do
  for target_cpu in "arm64" "x64"
  do
    # generate ninja files
    # 
    # note: `treat_warnings_as_errors=false` is for avoiding LLVM warning.
    #       https://reviews.llvm.org/D72212
    #       See below for details.
    #       https://bugs.chromium.org/p/webrtc/issues/detail?id=11729
    #
    # note: `use_xcode_clang=true` is for using bitcode.
    #
    gn gen "$OUTPUT_DIR" --root="src" \
      --args="is_debug=${is_debug}    \
      target_os=\"ios\"               \
      target_cpu=\"${target_cpu}\"    \
      rtc_use_h264=false              \
      treat_warnings_as_errors=false  \
      use_xcode_clang=true            \
      enable_ios_bitcode=true         \
      ios_enable_code_signing=false   \
      rtc_include_tests=false         \
      rtc_build_examples=false"

    # build static library
    ninja -C "$OUTPUT_DIR" webrtc

    # copy static library
    mkdir "$ARTIFACTS_DIR/lib/${target_cpu}"
    cp "$OUTPUT_DIR/obj/libwebrtc.a" "$ARTIFACTS_DIR/lib/${target_cpu}/"
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

# fix error when generate license
patch -N "./src/tools_webrtc/libs/generate_licenses.py" < \
  "$COMMAND_DIR/patches/generate_licenses.patch"

vpython "./src/tools_webrtc/libs/generate_licenses.py" \
  --target //:default "$OUTPUT_DIR" "$OUTPUT_DIR"

cd src
find . -name "*.h" -print | cpio -pd "$ARTIFACTS_DIR/include"

cp "$OUTPUT_DIR/LICENSE.md" "$ARTIFACTS_DIR"

# create zip
cd "$ARTIFACTS_DIR"
zip -r webrtc-ios.zip lib include LICENSE.md
