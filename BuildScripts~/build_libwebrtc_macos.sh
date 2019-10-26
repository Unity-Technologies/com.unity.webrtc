#!/bin/bash

if [ ! -e "$(pwd)/depot_tools" ]
then
  git clone --depth 1 https://chromium.googlesource.com/chromium/tools/depot_tools.git
fi

export PATH="$(pwd)/depot_tools:$PATH"
export WEBRTC_VERSION=72
export OUTPUT_DIR="$(pwd)/out"
export ARTIFACTS_DIR="$(pwd)/artifacts"

fetch webrtc

cd src
git config --system core.longpaths true
git branch -r
git checkout -b my_branch "refs/remotes/branch-heads/$WEBRTC_VERSION"
cd ..

gclient sync -f

# add jsoncpp
patch src/BUILD.gn < "$(pwd)/add_jsoncpp.patch"

gn gen "$OUTPUT_DIR" --root="src" --args="is_debug=false target_os=\"mac\" rtc_include_tests=false rtc_build_examples=false symbol_level=0 enable_iterator_debugging=false use_rtti=true"
ninja -C "$OUTPUT_DIR"

cd src
find . -name "*.h" -print | cpio -pd "$ARTIFACTS_DIR/include"

mkdir "$ARTIFACTS_DIR/lib"
array=("libwebrtc.a" "libaudio_decoder_opus.a" "libwebrtc_opus.a" "libjsoncpp.a")
for item in ${array[@]}; do
  find "$OUTPUT_DIR/obj" -name $item | xargs -J % cp % "$ARTIFACTS_DIR/lib"
done

# create zip
cd "$ARTIFACTS_DIR"
zip -r webrtc-mac.zip lib include 