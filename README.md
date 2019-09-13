# WebRTC for Unity

<img src="https://img.shields.io/badge/unity-2019.1-green.svg?style=flat-square" alt="unity 2019.1"><img src="https://img.shields.io/badge/unity-2019.2-green.svg?style=flat-square" alt="unity 2019.2">

**WebRTC for Unity** is a package that allows [WebRTC](https://webrtc.org) to be used in Unity.

If you are interested in the streaming solution with WebRTC, you can check [Unity Render Streaming](https://github.com/Unity-Technologies/UnityRenderStreaming). 

## Documentation

- [English](./Documentation~/index.md)
- [Japanese]( ./Documentation~/jp/index.md)

### Guide

- [Tutorial](./Documentation~/en/tutorial.md)

## Installation

To install the package, download WebRTC for Unity from the package manager. See the [documentation](https://docs.unity3d.com/Packages/com.unity.package-manager-ui@latest/index.html) for details on how to use the package manager. 

<img src="./Documentation~/images/webrtc_package_manager.png" width=600 align=center>

## Requirements

This version of the package is compatible with the following versions of the Unity Editor:

- 2019.1 and later (recommended)

> [!NOTE]
> <`Unity 2018.3` is not supported.>

- Currently the software only supports `windows64`.

- Graphics API version only supports `Direct3D11`.
-  `IL2CPP` is not supported in Scripting Backend by this package.

### Limitations

This package uses GPU hardware acceleration for video encoding, so it only runs on graphics cards that support [NVIDIA VIDEO CODEC SDK](https://developer.nvidia.com/nvidia-video-codec-sdk).

## Package Structure

```
.
├── BuildScripts~
├── Documentation~
│   ├── en
│   ├── images
│   └── jp
├── Editor
├── Plugin~
│   ├── unity
│   └── WebRTCPlugin
├── Runtime
│   ├── Plugins
│   │   └── x86_64
│   └── Srcipts
├── Samples~
│   └── Example
├── Tests
│   ├── Editor
│   └── Runtime
└── WebRTC~
```

### Samples

The package contains the following 3 samples. 

| Scene          | Details                                                  |
| -------------- | -------------------------------------------------------- |
| PeerConnection | A scene for checking the process of connecting to a peer |
| DataChannel    | A scene for sending and receiving text                   |
| MediaStream    | A scene for sending and receiving video/audio            |

## Roadmap

| Version | libwebrtc version                                            | Focus                                                        |
| ------- | ------------------------------------------------------------ | ------------------------------------------------------------ |
| `1.0`   | [M72](https://groups.google.com/forum/#!msg/discuss-webrtc/3h4y0fimHwg/j6G4dTVvCAAJ) | - First release                                              |
| `2.0`   |                                                              | - Multi camera <br>- DirectX12 (DXR) Support<br/>- IL2CPP Support |
| `2.1`   |                                                              | - Linux support <br>- Add HW encoder                         |

## Licenses

- [LICENSE.md](LICENSE.md)
- [Third Party Notices.md](Third%20Party%20Notices.md)
- [CONTRIBUTING.md](CONTRIBUTING.md)
