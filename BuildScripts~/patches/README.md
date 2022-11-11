# Patch


## Workaround for gn error for building android libwebrtc

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

## Workaround for gn error for building windows libwebrtc

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

## Workaround for abseil library building windows libwebrtc 

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

Patch file

- fix_abseil.patch

### Example

```
# `src` is a root directory of libwebrtc.

patch -N "src\third_party\abseil-cpp/absl/base/config.h" < "BuildScripts~\patches\fix_abseil.patch"
```