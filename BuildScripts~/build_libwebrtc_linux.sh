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

# change jsoncpp static library
# sed -i '' 's/source_set/static_library/' src/third_party/jsoncpp/BUILD.gn

gn gen "$OUTPUT_DIR" --root="src" --args="is_debug=false target_os=\"linux\" target_cpu=\"x64\" rtc_include_tests=false rtc_build_examples=false symbol_level=0 enable_iterator_debugging=false"

# add json.obj in link list of webrtc.ninja
# sed -i '' 's|obj/rtc_base/rtc_base/crc32.obj|obj/rtc_base/rtc_base/crc32.obj obj/rtc_base/rtc_json/json.obj|' "$OUTPUT_DIR/obj/webrtc.ninja"

ninja -C "$OUTPUT_DIR"

cd src
find . -name "*.h" -print | cpio -pd "$ARTIFACTS_DIR/include"

mkdir "$ARTIFACTS_DIR/lib"
array=("libwebrtc.a" "libaudio_decoder_opus.a" "libwebrtc_opus.a" "libjsoncpp.a")
for item in ${array[@]}; do
  find "$OUTPUT_DIR/obj" -name $item | xargs -I % cp % "$ARTIFACTS_DIR/lib"
done

# create zip
cd "$ARTIFACTS_DIR"
zip -r webrtc-linux.zip lib include