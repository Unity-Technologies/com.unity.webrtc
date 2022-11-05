# Patch


## Workaround for gn error for building android libwebrtc

Build libwebrtc for Android with `use_custom_libcxx=false` args for gn will get error. More details [here](https://bugs.chromium.org/p/webrtc/issues/detail?id=13535#c8).

### Patch files

- add_visibility_lubunwind.patch
- add_deps_libunwind.patch

### Example

```
# `src` is a root directory of libwebrtc. 

pushd src/buildtools
patch -p1 add_visibility_libunwind.patch
popd

pushd src/build
patch -p1 add_deps_libunwind.patch
popd
```