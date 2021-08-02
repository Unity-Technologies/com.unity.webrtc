# WebRTC 

![WebRTC header](images/webrtc_header.png)

## What is WebRTC

WebRTC for Unity is a package that allows [WebRTC](https://webrtc.org) to be used in Unity.

## Installation

Please see [Install package](install.md).

## Package samples

Please see [sample page](sample.md).

## Requirements

This version of the package is compatible with the following versions of the Unity Editor:

- **Unity 2019.4**
- **Unity 2020.3**

### Platform

- **Windows**
- **Linux**
- **macOS** (**Apple Slicon** is not supported yet)
- **iOS**
- **Android** (**ARM64** only. **ARMv7** is not supported)

> [!NOTE]
> **WebGL** platform is not supported.

### Encoder support

| Platform    | Graphics API | Hardware Encoder                                                                                                         | Software Encoder   |
| ----------- | ------------ | ------------------------------------------------------------------------------------------------------------------------ | ------------------ |
| Windows x64 | DirectX11    | :white_check_mark: (Require [NVIDIA Graphics card](https://developer.nvidia.com/video-encode-decode-gpu-support-matrix)) | :white_check_mark: | 
| Windows x64 | DirectX12    | :white_check_mark: (Require [NVIDIA Graphics card](https://developer.nvidia.com/video-encode-decode-gpu-support-matrix)) | :white_check_mark: | 
| Windows x64 | OpenGL Core  |                                                                                                                          |                    |
| Windows x64 | Vulkan       | :white_check_mark: (Require [NVIDIA Graphics card](https://developer.nvidia.com/video-encode-decode-gpu-support-matrix)) | :white_check_mark: | 
| Linux x64   | OpenGL Core  | :white_check_mark: (Require [NVIDIA Graphics card](https://developer.nvidia.com/video-encode-decode-gpu-support-matrix)) | :white_check_mark: |
| Linux x64   | Vulkan       | :white_check_mark: (Require [NVIDIA Graphics card](https://developer.nvidia.com/video-encode-decode-gpu-support-matrix)) | :white_check_mark: |
| MacOS       | Metal        | :white_check_mark:                              	                                                                        | :white_check_mark: |
| iOS         | Metal        | :white_check_mark:                              	                                                                        | :white_check_mark: | 
| Android     | Vulkan       | :white_check_mark:            	                                                                                        | :white_check_mark: |
| Android     | OpenGL ES    | :white_check_mark:            	                                                                                        | :white_check_mark: |

### Decoder support

| Platform    | Graphics API | Hardware Decoder                                                                                                         | Software Decoder   |
| ----------- | ------------ | ------------------ | ------------------ |
| Windows x64 | DirectX11    |                    | :white_check_mark: | 
| Windows x64 | DirectX12    |                    | :white_check_mark: | 
| Windows x64 | OpenGL Core  |                    | :white_check_mark: |
| Windows x64 | Vulkan       |                    | :white_check_mark: | 
| Linux x64   | OpenGL Core  |                    | :white_check_mark: |
| Linux x64   | Vulkan       |                    | :white_check_mark: |
| MacOS       | Metal        | :white_check_mark: | :white_check_mark: |
| iOS         | Metal        | :white_check_mark: | :white_check_mark: |
| Android     | Vulkan       | :white_check_mark: | :white_check_mark: |
| Android     | OpenGL ES    | :white_check_mark: | :white_check_mark: |

To check the compatible NVIDIA graphics card, please visit on the [NVIDIA VIDEO CODEC SDK web site](https://developer.nvidia.com/video-encode-decode-gpu-support-matrix#Encoder).

This package depends on **NVIDIA Video Codec SDK 9.1**. Please check the graphics driver version.
- Windows: Driver version `436.15` or higher
- Linux:   Driver version `435.21` or higher

> [!NOTE]
> On Linux, `libc++1` `libc++abi1` packages should be installed.
> Please install like command below 
>
> ``` sudo apt install -y libc++1 libc++abi1 ```

> [!NOTE]
> To make the archive for **iOS platform** to publish App Store, you need to use `lipo` command to eliminate the `x86_64` architecture from the binary in the `webrtc.framework`.
>
> ```lipo -remove x86_64 Runtime/Plugins/iOS/webrtc.framework/webrtc -o Runtime/Plugins/iOS/webrtc.framework/webrtc```

> [!NOTE]
> To build the apk file for **Android platform**, you need to configure player settings below.
> - **Scripting backend** - IL2CPP
> - **Target Architectures** - ARM64 (Do disable ARMv7)
>