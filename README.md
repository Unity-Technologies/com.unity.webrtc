# WebRTC for Unity

<img src="https://img.shields.io/badge/unity-2019.4-green.svg?style=flat-square" alt="unity 2019.4">
<img src="https://img.shields.io/badge/unity-2020.2-green.svg?style=flat-square" alt="unity 2020.2">

**WebRTC for Unity** is a package that allows [WebRTC](https://webrtc.org) to be used in Unity.

If you are interested in the streaming solution with WebRTC, you can check [Unity Render Streaming](https://github.com/Unity-Technologies/UnityRenderStreaming). 

## Documentation

- [English](https://docs.unity3d.com/Packages/com.unity.webrtc@latest/index.html)
- [Japanese](https://docs.unity3d.com/ja/Packages/com.unity.webrtc@latest/index.html
)

### Guide

Please read this if you have an interest to customize native code in this project.

- [Build native plugin](Plugin~/README.md)

## Installation

Please see [Install package](Documentation~/install.md).

## Requirements

Please see [Requirements](Documentation~/index.md#requirements).

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
│   └── Scripts
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

## Roadmap

| Version | libwebrtc version                                                              | Focus                                                                                          | When     | 
| ------- | ------------------------------------------------------------------------------ | ---------------------------------------------------------------------------------------------- | -------- |
| `1.0`   | [M72](https://groups.google.com/d/msg/discuss-webrtc/3h4y0fimHwg/j6G4dTVvCAAJ) | - First release                                                                                | Sep 2019 |    
| `1.1`   | [M72](https://groups.google.com/d/msg/discuss-webrtc/3h4y0fimHwg/j6G4dTVvCAAJ) | - IL2CPP Support<br> - Linux platform Support<br/> - Add software encoder                      | Feb 2020 |
| `2.0`   | [M79](https://groups.google.com/d/msg/discuss-webrtc/Ozvbd0p7Q1Y/M4WN2cRKCwAJ) | - Multi camera <br>- DirectX12 (DXR) Support                                                   | Apr 2020 |
| `2.1`   | [M84](https://groups.google.com/g/discuss-webrtc/c/MRAV4jgHYV0/m/A5X253_ZAQAJ) | - Profiler tool <br>- Bitrate control                                                          | Aug 2020 |
| `2.2`   | [M85](https://groups.google.com/g/discuss-webrtc/c/Qq3nsR2w2HU/m/7WGLPscPBwAJ) | - Video decoder (VP8, VP9 only) <br>- Vulkan HW encoder support <br>- MacOS HW encoder support | Oct 2020 |
| `2.3`   | [M85](https://groups.google.com/g/discuss-webrtc/c/Qq3nsR2w2HU/m/7WGLPscPBwAJ) | - iOS platform suppport                                                                        | Dec 2020 |
| `2.4`   | [M89](https://groups.google.com/g/discuss-webrtc/c/Zrsn2hi8FV0/m/KIbn0EZPBQAJ) | - Android platform suppport                                                                        | Apr 2021 |
| `2.5`   | [M89](https://groups.google.com/g/discuss-webrtc/c/Zrsn2hi8FV0/m/KIbn0EZPBQAJ) | - Audio renderer support                                                                        | Jun 2021 |

## Licenses

- [LICENSE.md](LICENSE.md)
- [Third Party Notices.md](Third%20Party%20Notices.md)

## Contribution
- [CONTRIBUTING.md](CONTRIBUTING.md)
