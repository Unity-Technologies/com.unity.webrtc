#!/bin/bash -eu

if [ ! -e "$(pwd)/depot_tools" ]
then
  git clone --depth 1 https://chromium.googlesource.com/chromium/tools/depot_tools.git
fi

export COMMAND_DIR=$(cd $(dirname $0); pwd)
export PATH="$(pwd)/depot_tools:$PATH"
export WEBRTC_VERSION=5615
export OUTPUT_DIR="$(pwd)/out"
export ARTIFACTS_DIR="$(pwd)/artifacts"
export PYTHON3_BIN="$(pwd)/depot_tools/python-bin/python3"

if [ ! -e "$(pwd)/src" ]
then
  fetch --nohooks webrtc
  cd src
  sudo sh -c 'echo 127.0.1.1 $(hostname) >> /etc/hosts'
  sudo git config --system core.longpaths true
  git checkout "refs/remotes/branch-heads/$WEBRTC_VERSION"
  cd ..
  gclient sync -D --force --reset
fi

# add jsoncpp
patch -N "src/BUILD.gn" < "$COMMAND_DIR/patches/add_jsoncpp.patch"

mkdir -p "$ARTIFACTS_DIR/lib"

for target_cpu in "x64"
do
  mkdir -p "$ARTIFACTS_DIR/lib/${target_cpu}"
  for is_debug in "true" "false"
  do
    args="is_debug=${is_debug} \
      target_os=\"linux\" \
      target_cpu=\"${target_cpu}\" \
      use_custom_libcxx=false \
      rtc_include_tests=false \
      rtc_build_examples=false \
      rtc_use_h264=false \
      symbol_level=0 \
      enable_iterator_debugging=false \
      is_component_build=false \
      use_rtti=true \
      rtc_use_x11=false \
      use_cxx17=true"

    if [ $is_debug = "true" ]; then
      args="${args} is_asan=true is_lsan=true";
    fi

    # generate ninja files
    gn gen "$OUTPUT_DIR" --root="src" --args="${args}"

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

"$PYTHON3_BIN" "./src/tools_webrtc/libs/generate_licenses.py" \
  --target :webrtc "$OUTPUT_DIR" "$OUTPUT_DIR"

cd src
find . -name "*.h" -print | cpio -pd "$ARTIFACTS_DIR/include"

cp "$OUTPUT_DIR/LICENSE.md" "$ARTIFACTS_DIR"

# create zip
cd "$ARTIFACTS_DIR"
zip -r webrtc-linux.zip lib include LICENSE.md