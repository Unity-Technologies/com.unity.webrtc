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
  fetch --nohooks webrtc_android
  cd src
  sudo sh -c 'echo 127.0.1.1 $(hostname) >> /etc/hosts'
  sudo git config --system core.longpaths true
  git checkout "refs/remotes/branch-heads/$WEBRTC_VERSION"
  cd ..
  gclient sync -D --force --reset
fi

# Add jsoncpp
patch -N "src/BUILD.gn" < "$COMMAND_DIR/patches/add_jsoncpp.patch"

# Add visibility libunwind
patch -N "src/buildtools/third_party/libunwind/BUILD.gn" < "$COMMAND_DIR/patches/add_visibility_libunwind.patch"

# Add deps libunwind
patch -N "src/build/config/BUILD.gn" < "$COMMAND_DIR/patches/add_deps_libunwind.patch"

# Add -mno-outline-atomics flag
patch -N "src/build/config/compiler/BUILD.gn" < "$COMMAND_DIR/patches/add_nooutlineatomics_flag.patch"

# downgrade to JDK8 because Unity supports OpenJDK version 1.8.
# https://docs.unity3d.com/Manual/android-sdksetup.html
patch -N "src/build/android/gyp/compile_java.py" < "$COMMAND_DIR/patches/downgradeJDKto8_compile_java.patch"
patch -N "src/build/android/gyp/turbine.py" < "$COMMAND_DIR/patches/downgradeJDKto8_turbine.patch"

mkdir -p "$ARTIFACTS_DIR/lib"

outputDir=""

for target_cpu in "arm64"
do
  mkdir -p "$ARTIFACTS_DIR/lib/${target_cpu}"

  for is_debug in "true" "false"
  do
    outputDir="${OUTPUT_DIR}_${target_cpu}_${is_debug}"
    # generate ninja files
    # use `treat_warnings_as_errors` option to avoid deprecation warnings
    gn gen "$outputDir" --root="src" \
      --args="is_debug=${is_debug} \
      is_java_debug=${is_debug} \
      target_os=\"android\" \
      target_cpu=\"${target_cpu}\" \
      rtc_use_h264=false \
      rtc_include_tests=false \
      rtc_build_examples=false \
      is_component_build=false \
      use_rtti=true \
      use_custom_libcxx=false \
      treat_warnings_as_errors=false \
      use_errorprone_java_compiler=false \
      use_cxx17=true"

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
  outputDir="${OUTPUT_DIR}_aar_${is_debug}"
  # use `treat_warnings_as_errors` option to avoid deprecation warnings
  "$PYTHON3_BIN" tools_webrtc/android/build_aar.py \
    --build-dir $outputDir \
    --output $outputDir/libwebrtc.aar \
    --arch arm64-v8a \
    --extra-gn-args "is_debug=${is_debug} \
      is_java_debug=${is_debug} \
      rtc_use_h264=false \
      rtc_include_tests=false \
      rtc_build_examples=false \
      is_component_build=false \
      use_rtti=true \
      use_custom_libcxx=false \
      treat_warnings_as_errors=false \
      use_errorprone_java_compiler=false \
      use_cxx17=true"

  filename="libwebrtc.aar"
  if [ $is_debug = "true" ]; then
    filename="libwebrtc-debug.aar"
  fi
  # copy aar
  cp "$outputDir/libwebrtc.aar" "$ARTIFACTS_DIR/lib/${filename}"
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
