# Patch

## Workaround for the linker error of jsoncpp and InternalVideoEncoderFactory

The Unity package depends on modules `jsoncpp` and `InternalVideoEncoderFactory`. However, they are not contained in libwebrtc library by default. This patch file rewrites `BUILD.gn` file to add dependencies of the library for resolving the linker error which there are no symbols about these modules.

### Patch files

- add_jsoncpp.patch

### Example

```
# `src` is a root directory of libwebrtc. 

patch -N "src/BUILD.gn" < "$COMMAND_DIR/patches/add_jsoncpp.patch"
```

## Workaround for the compile error about designated initializer which is supported since c++20

The C++ version of the compiler which build native plugins is **c++17** but not **c++20**. The libwebrtc library is built with **c++20**, and the header file contains the c++20 language feature [**designated initializer list**](https://en.cppreference.com/w/cpp/language/aggregate_initialization). That's why the compiler puts errors about the header file without this patch.

### Patch files

- fix_task_queue_base.patch

### Example

```
# `src` is a root directory of libwebrtc. 

patch -N "src/api/task_queue/task_queue_base.h" < "$COMMAND_DIR/patches/fix_task_queue_base.patch"
```

## Workaround for gn error for building Android libwebrtc

Building libwebrtc for Android with `use_custom_libcxx=false` args for gn will get error. More details [here](https://bugs.chromium.org/p/webrtc/issues/detail?id=13535#c8).

### Patch files

- add_visibility_lubunwind.patch
- add_deps_libunwind.patch

### Example

```
# `src` is a root directory of libwebrtc. 

patch -N "src/buildtools/third_party/libunwind/BUILD.gn" < "$COMMAND_DIR/patches/add_visibility_libunwind.patch"

patch -N "src/build/config/BUILD.gn" < "$COMMAND_DIR/patches/add_deps_libunwind.patch"
```

## Workaround for runtime error on Android

Unity supports OpenJDK version 1.8 for building Android app. You can see [the table](https://docs.unity3d.com/Manual/android-sdksetup.html) shows the JDK version that each Unity version supports. However, JDK version for libwebrtc has been updated in [this commit](https://source.chromium.org/chromium/chromium/src/+/ff333588f945ab6438a98a8d5feabec2be60ccf1). We made a patch to downgrade the JDK.

### Patch files

- downgradeJDKto8_compile_java.patch
- downgradeJDKto8_turbine.patch

### Example

```
# `src` is a root directory of libwebrtc. 

patch -N "src/build/android/gyp/compile_java.py" < "$COMMAND_DIR/patches/downgradeJDKto8_compile_java.patch"
patch -N "src/build/android/gyp/turbine.py" < "$COMMAND_DIR/patches/downgradeJDKto8_turbine.patch"
```


## Workaround for gn error for building Windows libwebrtc

Building libwebrtc for Windows with `use_custom_libcxx=false` args for gn will get error.

```
../src/modules/desktop_capture/win/full_screen_win_application_handler.cc(281,18): error: no member named 'towupper' in namespace 'std'; did you mean simply 'towupper'?
std::towupper);
^~~~~~~~~~~~~
towupper
../../../../../Program Files (x86)/Windows Kits/10/include/10.0.20348.0/ucrt\corecrt_wctype.h(102,40): note: 'towupper' declared here
_Check_return_ _ACRTIMP wint_t __cdecl towupper(_In_ wint_t _C);
^
```

### Patch files

- fix_towupper.patch

### Example

```
# `src` is a root directory of libwebrtc. 

patch -N "src\modules\desktop_capture\win\full_screen_win_application_handler.cc" < "BuildScripts~\patches\fix_towupper.patch"
```

## Workaround for abseil library building Windows libwebrtc 

Building libwebrtc for Windows with `use_custom_libcxx=false` args for gn will get error. Referenced [here](https://github.com/abseil/abseil-cpp/pull/1289) for the patch file.

```
../src/audio/audio_send_stream.cc(344,25): error: object of type 'absl::optional<std::pair<TimeDelta, TimeDelta>>' cannot be assigned because its copy assignment operator is implicitly deleted
frame_length_range_ = encoder->GetFrameLengthRange();
^
../src/third_party/abseil-cpp\absl/types/optional.h(279,13): note: explicitly defaulted function was implicitly deleted here
optional& operator=(const optional& src) = default;
^
../src/third_party/abseil-cpp\absl/types/optional.h(119,18): note: copy assignment operator of 'optional<std::pair<webrtc::TimeDelta, webrtc::TimeDelta>>' is implicitly deleted because base class 'optional_internal::optional_data<pair<TimeDelta, TimeDelta>>' has a deleted copy assignment operator
class optional : private optional_internal::optional_data<T>,
^
../src/third_party/abseil-cpp\absl/types/internal/optional.h(189,32): note: copy assignment operator of 'optional_data<std::pair<webrtc::TimeDelta, webrtc::TimeDelta>, true>' is implicitly deleted because base class 'optional_data_base<pair<TimeDelta, TimeDelta>>' has a deleted copy assignment operator
class optional_data<T, true> : public optional_data_base<T> {
^
../src/third_party/abseil-cpp\absl/types/internal/optional.h(145,28): note: copy assignment operator of 'optional_data_base<std::pair<webrtc::TimeDelta, webrtc::TimeDelta>>' is implicitly deleted because base class 'optional_data_dtor_base<pair<TimeDelta, TimeDelta>>' has a deleted copy assignment operator
class optional_data_base : public optional_data_dtor_base<T> {
^
../src/third_party/abseil-cpp\absl/types/internal/optional.h(131,7): note: copy assignment operator of 'optional_data_dtor_base<std::pair<webrtc::TimeDelta, webrtc::TimeDelta>, true>' is implicitly deleted because variant field 'data_' has a non-trivial copy assignment operator
T data_;
^
```

### Patch file

- fix_abseil.patch

### Example

```
# `src` is a root directory of libwebrtc.

patch -N "src\third_party\abseil-cpp/absl/base/config.h" < "BuildScripts~\patches\fix_abseil.patch"
```

## Workaround for can't be profiled correctly encode/decode process on macOS/iOS

Task queuing in libwebrtc is implemented differently on each platform. macOS/iOS is implemented using [GCD (Global Central Dispatch)](https://developer.apple.com/documentation/DISPATCH). GCD makes no guarantees about which thread it uses to execute a task. Referenced [here](https://developer.apple.com/documentation/dispatch/dispatchqueue). Since UnityProfiler is implemented assuming execution in the same thread, it was changed to use TaskQueue implementation by STDLIB which is executed in the same thread.

### Patch file

- disable_task_queue_gcd.patch

### Example

```
# `src` is a root directory of libwebrtc.

patch -N "src/api/task_queue/BUILD.gn" < "BuildScripts~/patches/disable_task_queue_gcd.patch"
```

## Workaround for dlopen error on Android.

The `nooutlineatomics` flag no longer applies due to the update to M112. However, DLOpenError occurs when loading a plugin built without the flag in the Unity Runtime. [Commit](https://chromium.googlesource.com/chromium/src/build/+/49b04e63ae168f3299bcf6fd2cc50d9906a1a165%5E%21/) where this flag was removed.

### Patch file

- add_nooutlineatomics_flag.patch

### Example

```
# `src` is a root directory of libwebrtc.

patch -N "src/build/config/compiler/BUILD.gn" < "$COMMAND_DIR/patches/add_nooutlineatomics_flag.patch"
```

## Workaround for timeout when downloading libwebrtc source.

It takes a long time to download data that is not necessary for building the library, and a timeout occurs. Avoid downloading anything that takes a particularly long time.

### Patch file

- fetch_exclude_examples.patch

### Example

```
# `depot_tools` is a root directory of depot_tools.

patch -N "depot_tools/fetch_configs/webrtc.py" < "$COMMAND_DIR/patches/fetch_exclude_examples.patch"
```

## Workaround for received video not displayed on Android.

No video is being passed to the encoder because `VideoAdapter::GetMaxFramerate` always returns `0`. The return value of `AdaptedVideoTrackSource::video_adapter()` was returning a different reference.

### Patch file

- fix_adaptedvideotracksource.patch

### Example

```
# `src` is a root directory of libwebrtc.

pushd src
patch -p1 < "$COMMAND_DIR/patches/fix_adaptedvideotracksource.patch"
popd
```
