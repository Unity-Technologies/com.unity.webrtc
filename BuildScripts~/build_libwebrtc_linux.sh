#!/bin/bash

if [ ! -e "$(pwd)/depot_tools" ]
then
  git clone --depth 1 https://chromium.googlesource.com/chromium/tools/depot_tools.git
fi

export PATH="$(pwd)/depot_tools:$PATH"
export WEBRTC_VERSION=m79
export OUTPUT_DIR="$(pwd)/out"
export ARTIFACTS_DIR="$(pwd)/artifacts"

fetch webrtc

cd src
git config --system core.longpaths true
git checkout "refs/remotes/branch-heads/$WEBRTC_VERSION"
cd ..

gclient sync -f

# add jsoncpp
patch "src/BUILD.gn" < "BuildScripts~/add_jsoncpp.patch"

gn gen "$OUTPUT_DIR" --root="src" --args="is_debug=false target_os=\"linux\" rtc_include_tests=false rtc_build_examples=false rtc_use_h264=false symbol_level=0 enable_iterator_debugging=false use_rtti=true rtc_use_x11=false"
ninja -C "$OUTPUT_DIR"

./src/third_party/llvm-build/Release+Asserts/bin/llvm-ar -rc "$OUTPUT_DIR/libwebrtc.a" `find $OUTPUT_DIR/obj/. -name '*.o'`

python ./src/tools_webrtc/libs/generate_licenses.py --target //:default "$OUTPUT_DIR" "$OUTPUT_DIR"

cd src
find . -name "*.h" -print | cpio -pd "$ARTIFACTS_DIR/include"

mkdir "$ARTIFACTS_DIR/lib"
cp "$OUTPUT_DIR/libwebrtc.a" "$ARTIFACTS_DIR/lib"

cp "$OUTPUT_DIR/LICENSE.md" "$ARTIFACTS_DIR"

# create zip
cd "$ARTIFACTS_DIR"
zip -r webrtc-linux.zip lib include LICENSE.md