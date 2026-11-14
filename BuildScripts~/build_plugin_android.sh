#!/usr/bin/env bash
set -euo pipefail

# Resolve paths from the script, rather than depending on the caller's directory.
REPO_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
ARTIFACT_DIR="${ARTIFACT_DIR:-$REPO_ROOT/artifacts~/android}"
PLUGIN_DIR="${PLUGIN_DIR:-$REPO_ROOT/Runtime/Plugins/Android}"
ANDROID_NDK="${ANDROID_NDK:-${ANDROID_NDK_HOME:-${ANDROID_NDK_ROOT:-}}}"
CMAKE="${CMAKE:-cmake}"
BUILD_TYPE="${1:-release}"
case "$BUILD_TYPE" in
  release) CMAKE_BUILD_TYPE=Release; BASE_AAR=libwebrtc.aar ;;
  debug) CMAKE_BUILD_TYPE=Debug; BASE_AAR=libwebrtc-debug.aar ;;
  *) echo "Usage: $0 [release|debug]" >&2; exit 1 ;;
esac
if [[ -z "$ANDROID_NDK" || ! -f "$ANDROID_NDK/build/cmake/android.toolchain.cmake" ]]; then
  echo 'Set ANDROID_NDK to an Android NDK installation.' >&2
  exit 1
fi

RELEASE=M116-20250805
ARCHIVE_URL="https://github.com/Unity-Technologies/com.unity.webrtc/releases/download/$RELEASE/webrtc-android.zip"
ARCHIVE_SHA256=f561153cf06dc5d0c95cb7272168582df9cb37a960c8fb7907dd775547766d4a
DOWNLOAD_DIR="$ARTIFACT_DIR/downloads"
DEPENDENCY_DIR="$ARTIFACT_DIR/dependencies/$RELEASE"
PACKAGE_DIR="$ARTIFACT_DIR/package/$CMAKE_BUILD_TYPE"
LOG_DIR="$ARTIFACT_DIR/logs/$CMAKE_BUILD_TYPE"
ARCHIVE="$DOWNLOAD_DIR/webrtc-android-$RELEASE.zip"
mkdir -p "$DOWNLOAD_DIR" "$PACKAGE_DIR" "$LOG_DIR"

if [[ ! -f "$ARCHIVE" ]]; then
  curl --fail --location --retry 3 --silent --show-error "$ARCHIVE_URL" -o "$ARCHIVE.partial"
  mv "$ARCHIVE.partial" "$ARCHIVE"
fi
if command -v sha256sum >/dev/null 2>&1; then
  ACTUAL_SHA256="$(sha256sum "$ARCHIVE" | cut -d ' ' -f 1)"
else
  ACTUAL_SHA256="$(shasum -a 256 "$ARCHIVE" | cut -d ' ' -f 1)"
fi
if [[ "$ACTUAL_SHA256" != "$ARCHIVE_SHA256" ]]; then
  echo "SHA256 mismatch for $ARCHIVE" >&2
  exit 1
fi
if [[ ! -f "$DEPENDENCY_DIR/.archive-sha256" ]]; then
  mkdir -p "$DEPENDENCY_DIR"
  unzip -q -o "$ARCHIVE" -d "$DEPENDENCY_DIR"
  printf '%s\n' "$ARCHIVE_SHA256" > "$DEPENDENCY_DIR/.archive-sha256"
elif [[ "$(tr '[:upper:]' '[:lower:]' < "$DEPENDENCY_DIR/.archive-sha256" | tr -d '\r\n')" != "$ARCHIVE_SHA256" ]]; then
  echo 'The extracted dependency does not match the pinned archive.' >&2
  exit 1
fi

for ABI in arm64-v8a x86_64; do
  BUILD_DIR="$ARTIFACT_DIR/build/$CMAKE_BUILD_TYPE/$ABI"
  "$CMAKE" -S "$REPO_ROOT/Plugin~" -B "$BUILD_DIR" -G Ninja \
    -D "CMAKE_TOOLCHAIN_FILE=$ANDROID_NDK/build/cmake/android.toolchain.cmake" \
    -D "ANDROID_NDK=$ANDROID_NDK" -D "ANDROID_ABI=$ABI" -D ANDROID_PLATFORM=android-24 \
    -D ANDROID_STL=c++_static -D ANDROID_SUPPORT_FLEXIBLE_PAGE_SIZES=ON \
    -D "CMAKE_BUILD_TYPE=$CMAKE_BUILD_TYPE" -D BUILD_TESTING=OFF \
    -D 'CMAKE_SHARED_LINKER_FLAGS=-Wl,-z,max-page-size=16384' \
    -D "WEBRTC_DIR=$DEPENDENCY_DIR" -D "WEBRTC_PLUGIN_OUTPUT_DIR=$BUILD_DIR/plugin" \
    2>&1 | tee "$LOG_DIR/$ABI-configure.log"
  "$CMAKE" --build "$BUILD_DIR" --target WebRTCPlugin --parallel "${JOBS:-4}" \
    2>&1 | tee "$LOG_DIR/$ABI-build.log"
done

# Package and publish only after both ABIs built successfully.
cp "$DEPENDENCY_DIR/lib/$BASE_AAR" "$PACKAGE_DIR/libwebrtc.aar"
for ABI in arm64-v8a x86_64; do
  mkdir -p "$PACKAGE_DIR/jni/$ABI"
  cp "$ARTIFACT_DIR/build/$CMAKE_BUILD_TYPE/$ABI/plugin/libwebrtc.so" "$PACKAGE_DIR/jni/$ABI/libwebrtc.so"
done

# Retain the existing debug workflow's Vulkan validation-layer packaging.
if [[ "$BUILD_TYPE" == debug ]]; then
  VALIDATION_DIR="$ARTIFACT_DIR/validation"
  if [[ ! -f "$VALIDATION_DIR/arm64-v8a/libVkLayer_khronos_validation.so" ]]; then
    VALIDATION_ARCHIVE="$DOWNLOAD_DIR/android-binaries-1.4.321.0.zip"
    curl --fail --location --retry 3 --silent --show-error \
      'https://github.com/KhronosGroup/Vulkan-ValidationLayers/releases/download/vulkan-sdk-1.4.321.0/android-binaries-1.4.321.0.zip' \
      -o "$VALIDATION_ARCHIVE"
    unzip -q -o "$VALIDATION_ARCHIVE" -d "$ARTIFACT_DIR"
    mkdir -p "$VALIDATION_DIR"
    for ABI in arm64-v8a x86_64; do
      mkdir -p "$VALIDATION_DIR/$ABI"
      cp "$ARTIFACT_DIR/android-binaries-1.4.321.0/$ABI/libVkLayer_khronos_validation.so" "$VALIDATION_DIR/$ABI/"
    done
  fi
  for ABI in arm64-v8a x86_64; do
    cp "$VALIDATION_DIR/$ABI/libVkLayer_khronos_validation.so" "$PACKAGE_DIR/jni/$ABI/"
  done
fi
(
  cd "$PACKAGE_DIR"
  zip -q -g libwebrtc.aar jni/arm64-v8a/*.so jni/x86_64/*.so
)
mkdir -p "$PLUGIN_DIR"
if [[ "$PACKAGE_DIR/libwebrtc.aar" != "$PLUGIN_DIR/libwebrtc.aar" ]]; then
  cp "$PACKAGE_DIR/libwebrtc.aar" "$PLUGIN_DIR/libwebrtc.aar.partial"
  mv "$PLUGIN_DIR/libwebrtc.aar.partial" "$PLUGIN_DIR/libwebrtc.aar"
fi
printf 'Built %s (%s, arm64-v8a and x86_64).\nDependency: %s\nSHA256: %s\n' \
  "$PLUGIN_DIR/libwebrtc.aar" "$CMAKE_BUILD_TYPE" "$ARCHIVE_URL" "$ARCHIVE_SHA256"
