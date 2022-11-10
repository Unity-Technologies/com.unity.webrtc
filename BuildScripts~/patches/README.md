# Patch


## Workaround for gn error for building android libwebrtc

Build libwebrtc for Android with `use_custom_libcxx=false` args for gn will get error. More details [here](https://bugs.chromium.org/p/webrtc/issues/detail?id=13535#c8).

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

Build libwebrtc for Windows with `use_custom_libcxx=false` args for gn will get error.

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

patch -N "src\modules\desktop_capture\win\full_screen_win_application_handler.cc" < "%COMMAND_DIR%\patches\fix_towupper.patch"
```