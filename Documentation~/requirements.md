# Requirements

This page lists the Unity version and platform that the package supports.

## Unity Version

It is recommended to use the latest Long-Term Support (LTS) version of Unity, see [the post](https://blog.unity.com/technology/new-plans-for-unity-releases-introducing-the-tech-and-long-term-support-lts-streams) in Unity blog for LTS versions.

This version of the package is compatible with the following versions of the Unity Editor:

- **Unity 2019.4**
- **Unity 2020.3**
- **Unity 2021.2**

## Support Platform

- **Windows**
- **Linux**
- **macOS** (**Intel** and **Apple Slicon**)
- **iOS**
- **Android** (**ARM64** only. **ARMv7** is not supported)

## Additional Notes

Please note that there are unsupported platforms below.

- **Windows UWP** platform is not supported.
- Building for **iOS Simulator** is not supported.
- **WebGL** platform is not supported.

### Build on Linux

On Linux, `libc++1` `libc++abi1` packages should be installed.
Please install like command below 

``` 
sudo apt install -y libc++1 libc++abi1 
```

### Build on Android

To build the apk file for **Android platform**, you need to configure player settings below.

- **Scripting backend** - IL2CPP
- **Target Architectures** - ARM64 (Do disable ARMv7)