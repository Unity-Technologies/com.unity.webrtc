#!/bin/bash

if [ ! -e "$(pwd)/depot_tools" ]
then
  git clone --depth 1 https://chromium.googlesource.com/chromium/tools/depot_tools.git
fi

export COMMAND_DIR=$(cd $(dirname $0); pwd)
export PATH="$(pwd)/depot_tools:$PATH"
export WEBRTC_VERSION=4183
export OUTPUT_DIR="$(pwd)/out"
export ARTIFACTS_DIR="$(pwd)/artifacts"

if [ ! -e "$(pwd)/src" ]
then
  fetch --nohooks webrtc_ios
  cd src
  git config --system core.longpaths true
  git checkout "refs/remotes/branch-heads/$WEBRTC_VERSION"
  cd ..
  gclient sync -f
fi

# add jsoncpp
patch -N "src/BUILD.gn" < "$COMMAND_DIR/patches/add_jsoncpp.patch"

mkdir -p "$ARTIFACTS_DIR/lib"

# add jsoncpp
patch -N "src/BUILD.gn" < "$COMMAND_DIR/patches/add_jsoncpp.patch"

mkdir -p "$ARTIFACTS_DIR/lib"

for target_cpu in "arm64" "x64"
do
  mkdir "$ARTIFACTS_DIR/lib/${target_cpu}"
  for is_debug in "true" "false"
  do
    # generate ninja files
    gn gen "$OUTPUT_DIR" --root="src" \
      --args="is_debug=${is_debug} target_os=\"ios\" target_cpu=\"${target_cpu}\" rtc_use_h264=false rtc_include_tests=false rtc_build_examples=false"

    # build static library
    ninja -C "$OUTPUT_DIR" webrtc

    filename="libwebrtc.a"
    if [ $is_debug = "true" ]; then
      filename="libwebrtcd.a"
    fi

    # cppy static library
    cp "$OUTPUT_DIR/obj/libwebrtc.a" "$ARTIFACTS_DIR/lib/${target_cpu}/${filename}"
  done
done

# fix error when generate license
patch -N "./src/tools_webrtc/libs/generate_licenses.py" < \
  "$COMMAND_DIR/patches/generate_licenses.patch"

python "./src/tools_webrtc/libs/generate_licenses.py" \
  --target //:default "$OUTPUT_DIR" "$OUTPUT_DIR"

cd src
find . -name "*.h" -print | cpio -pd "$ARTIFACTS_DIR/include"

cp "$OUTPUT_DIR/LICENSE.md" "$ARTIFACTS_DIR"

# create zip
cd "$ARTIFACTS_DIR"
zip -r webrtc-ios.zip lib include LICENSE.md