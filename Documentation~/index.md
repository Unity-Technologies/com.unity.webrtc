# WebRTC 

![WebRTC header](images/webrtc_header.png)

## What is WebRTC

WebRTC for Unity is a package that allows [WebRTC](https://webrtc.org) to be used in Unity.

## Requirements

This version of the package is compatible with the following versions of the Unity Editor:

- **Unity 2019.3**

| Platform    | Graphics API | Hardware Encoder                                                                                                         | Software Encoder   |
| ----------- | ------------ | ------------------------------------------------------------------------------------------------------------------------ | ------------------ |
| Windows x64 | DirectX11    | :white_check_mark: (Require [NVIDIA Graphics card](https://developer.nvidia.com/video-encode-decode-gpu-support-matrix)) | :white_check_mark: | 
| Windows x64 | DirectX12    |                                                                                                                          |                    | 
| Windows x64 | OpenGL       |                                                                                                                          |                    |
| Windows x64 | Vulkan       |                                                                                                                          |                    | 
| Linux x64   | OpenGL       | :white_check_mark: (Require [NVIDIA Graphics card](https://developer.nvidia.com/video-encode-decode-gpu-support-matrix)) |                    |
| Linux x64   | Vulkan       |                                                 	                                                                        |                    |
| MacOS       | OpenGL       |                                                 	                                                                        |                    |
| MacOS       | Metal        |                                                 	                                                                        | :white_check_mark: |

To check the compatible NVIDIA graphics card, please visit on the [NVIDIA VIDEO CODEC SDK web site](https://developer.nvidia.com/video-encode-decode-gpu-support-matrix#Encoder).

> [!NOTE]
> On Linux, `libc++1` `libc++abi1` packages should be installed.
> Please install like command below 
>
> ``` sudo apt install -y libc++1 libc++abi1 ```

## Installation
To install the package, download WebRTC for Unity from the package manager. See the [documentation](https://docs.unity3d.com/Manual/upm-ui.html) for details on how to use the package manager. 

![WebRTC Package Manager](images/webrtc_package_manager.png)

## Samples

The package contains the following 3 samples. 

| Scene          | Details                                                  |
| -------------- | -------------------------------------------------------- |
| PeerConnection | A scene for checking the process of connecting to a peer |
| DataChannel    | A scene for sending and receiving text                   |
| MediaStream    | A scene for sending and receiving video/audio            |

To get these samples, Push the `Import into Project` button on Package Manager.

![Download package sample](images/download_package_sample.png)

## Other Languages

- [Japanese](jp/index.md)