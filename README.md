# WebRTC for Unity

<img src="https://img.shields.io/badge/unity-2019.3-green.svg?style=flat-square" alt="unity 2019.3">

**WebRTC for Unity** is a package that allows [WebRTC](https://webrtc.org) to be used in Unity.

If you are interested in the streaming solution with WebRTC, you can check [Unity Render Streaming](https://github.com/Unity-Technologies/UnityRenderStreaming). 

## Documentation

- [English](./Documentation~/index.md)
- [Japanese]( ./Documentation~/jp/index.md)

### Guide

- [Tutorial](./Documentation~/en/tutorial.md)
- [Build plugin](Plugin~/README.md)

## Installation

To install the package, download WebRTC for Unity from the package manager. See the [documentation](https://docs.unity3d.com/Packages/com.unity.package-manager-ui@latest/index.html) for details on how to use the package manager. 

<img src="./Documentation~/images/webrtc_package_manager.png" width=600 align=center>

## Requirements

This version of the package is compatible with the following versions of the Unity Editor:

- Unity 2019.3 and later

| Platform    | Graphics API | Hardware Encoder                                  | Software Encoder   |
| ----------- | ------------ | ------------------------------------------------- | ------------------ |
| Windows X64 | DirectX11    | :white_check_mark: (Require NVIDIA Graphics card) | :white_check_mark: | 
| Windows X64 | DirectX12    |                                                   |                    | 
| Windows X64 | OpenGL       |                                                   |                    |
| Windows X64 | Vulkan       |                                                   |                    | 
| Linux X64   | OpenGL       | :white_check_mark: (Require NVIDIA Graphics card) |                    |
| Linux X64   | Vulkan       |                                                   |                    |
| MacOS       | OpenGL       |                                                   |                    |
| MacOS       | Metal        |                                                   | :white_check_mark: |

On Linux, `libc++1` `libc++abi1` packages should be installed.

```
sudo apt install -y libc++1 libc++abi1
```

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
│   ├── cmake
│   ├── gl3w
│   ├── unity
│   ├── WebRTCPlugin
│   │   ├── Codec
│   │   │   ├── NvCodec
│   │   │   ├── SoftwareCodec
│   │   │   └── VideoToolbox
│   │   └── GraphicsDevice
│   │       ├── D3D11
│   │       ├── D3D12
│   │       ├── Metal
│   │       ├── OpenGL
│   │       └── Vulkan
│   └── WebRTCPluginTest
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
    ├── Assets
    ├── Packages
    │   └── com.unity.webrtc
    │       ├── Editor
    │       ├── Runtime
    │       └── Tests
    └── ProjectSettings
```

### Samples

The package contains the following 3 samples. 

| Scene          | Details                                                  |
| -------------- | -------------------------------------------------------- |
| PeerConnection | A scene for checking the process of connecting to a peer |
| DataChannel    | A scene for sending and receiving text                   |
| MediaStream    | A scene for sending and receiving video/audio            |

## Roadmap

| Version | libwebrtc version                                                                    | Focus                                                             |
| ------- | ------------------------------------------------------------------------------------ | ----------------------------------------------------------------- |
| `1.0`   | [M72](https://groups.google.com/forum/#!msg/discuss-webrtc/3h4y0fimHwg/j6G4dTVvCAAJ) | - First release                                                   |
| `1.1`   | [M72](https://groups.google.com/forum/#!msg/discuss-webrtc/3h4y0fimHwg/j6G4dTVvCAAJ) | - IL2CPP Support<br> - Linux Support<br/> - Add software encoder  |
| `2.0`   | [M79](https://groups.google.com/forum/#!msg/discuss-webrtc/X8q5Ae9VKco/oEiGuteoBAAJ) | - Multi camera <br>- DirectX12 (DXR) Support                      |
| `2.1`   |                                                                                      | - MacOS support <br>                                              |

## Licenses

- [LICENSE.md](LICENSE.md)
- [Third Party Notices.md](Third%20Party%20Notices.md)
- [CONTRIBUTING.md](CONTRIBUTING.md)
