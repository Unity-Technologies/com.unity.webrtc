# WebRTC for Unity (FDGS fork)

<img src="https://img.shields.io/badge/unity-6000.0%2B-green.svg?style=flat-square" alt="unity 6000.0+"> <img src="https://img.shields.io/badge/license-Apache--2.0-blue.svg?style=flat-square" alt="Apache-2.0">

**WebRTC for Unity** lets you use [WebRTC](https://webrtc.org) in Unity: peer connections, audio and video streaming, and data channels, across desktop and mobile platforms.

This repository is a community-maintained fork of [`com.unity.webrtc`](https://github.com/Unity-Technologies/com.unity.webrtc), which Unity has deprecated and no longer maintains. The goal is to keep the package working on current Unity versions, fix long-standing issues and extend it. The package is published as **`com.fdgs.webrtc`**, but keeps the `Unity.WebRTC` namespace and assembly names, so existing code works unchanged.

## Installation

In the Package Manager, choose **Add package from git URL...** and enter:

```
https://github.com/FireDragonGameStudio/com.unity.webrtc.fdgs.git
```

Remove `com.unity.webrtc` first; the two packages cannot be installed side by side. See [Install package](Documentation~/install.md) for details.

## Documentation

- [Documentation](Documentation~/index.md)
- [Requirements and supported platforms](Documentation~/requirements.md)
- [Tutorial](Documentation~/tutorial.md)
- [Samples](Documentation~/sample.md)

### Customize native plugin

The native plugin (C++, built on Google's libwebrtc) lives in [`Plugin~`](Plugin~). See [Build native plugin](Plugin~/README.md) for how to build it.

## Status

| Version | libwebrtc | Notes |
| ------- | --------- | ----- |
| `3.0.0` | [M116](https://groups.google.com/g/discuss-webrtc/c/bEsO8Lz7psE) | Starting point of this fork, identical to the last Apache-2.0 release of `com.unity.webrtc`. |

Planned work is tracked in [GitHub Issues](https://github.com/FireDragonGameStudio/com.unity.webrtc.fdgs/issues). See [CHANGELOG.md](CHANGELOG.md) for the history of this package, including the releases made by Unity Technologies.

## Licenses

- [LICENSE.md](LICENSE.md) (Apache License 2.0)
- [Third Party Notices.md](Third%20Party%20Notices.md)

This fork is based on upstream commit `2f78a89`, the last version of `com.unity.webrtc` released under the Apache License 2.0. Later upstream commits are under the Unity Companion License and are not included here.

This project is not affiliated with or endorsed by Unity Technologies.

## Contribution

- [CONTRIBUTING.md](CONTRIBUTING.md)
