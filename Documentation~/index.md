# WebRTC 

![WebRTC header](images/webrtc_header.png)

## What is WebRTC

WebRTC for Unity is a package that allows [WebRTC](https://webrtc.org) to be used in Unity.

## Requirements

This version of the package is compatible with the following versions of the Unity Editor:

- **Unity 2019.4**

### Platform

- **Windows**
- **Linux**
- **macOS** (**Apple Slicon** is not supported yet)
- **iOS**

> [!NOTE]
> **Android** platform is not supported yet.

### Encoder support

| Platform    | Graphics API | Hardware Encoder                                                                                                         | Software Encoder   |
| ----------- | ------------ | ------------------------------------------------------------------------------------------------------------------------ | ------------------ |
| Windows x64 | DirectX11    | :white_check_mark: (Require [NVIDIA Graphics card](https://developer.nvidia.com/video-encode-decode-gpu-support-matrix)) | :white_check_mark: | 
| Windows x64 | DirectX12    | :white_check_mark: (Require [NVIDIA Graphics card](https://developer.nvidia.com/video-encode-decode-gpu-support-matrix)) | :white_check_mark: | 
| Windows x64 | OpenGL       |                                                                                                                          |                    |
| Windows x64 | Vulkan       | :white_check_mark: (Require [NVIDIA Graphics card](https://developer.nvidia.com/video-encode-decode-gpu-support-matrix)) | :white_check_mark: | 
| Linux x64   | OpenGL       | :white_check_mark: (Require [NVIDIA Graphics card](https://developer.nvidia.com/video-encode-decode-gpu-support-matrix)) |                    |
| Linux x64   | Vulkan       | :white_check_mark: (Require [NVIDIA Graphics card](https://developer.nvidia.com/video-encode-decode-gpu-support-matrix)) | :white_check_mark: |
| MacOS       | Metal        | :white_check_mark:                              	                                                                        | :white_check_mark: |
| iOS         | Metal        | :white_check_mark:                              	                                                                        | :white_check_mark: | 
| Android     | Vulkan       |                               	                                                                                        |                    |

### Decoder support

| Platform    | Graphics API | Hardware Decoder                                                                                                         | Software Decoder   |
| ----------- | ------------ | ------------------ | ------------------ |
| Windows x64 | DirectX11    |                    | :white_check_mark: | 
| Windows x64 | DirectX12    |                    | :white_check_mark: | 
| Windows x64 | OpenGL       |                    | :white_check_mark: |
| Windows x64 | Vulkan       |                    | :white_check_mark: | 
| Linux x64   | OpenGL       |                    | :white_check_mark: |
| Linux x64   | Vulkan       |                    | :white_check_mark: |
| MacOS       | Metal        | :white_check_mark: | :white_check_mark: |
| iOS         | Metal        | :white_check_mark: | :white_check_mark: |
| Android     | Vulkan       |                    |                    |


To check the compatible NVIDIA graphics card, please visit on the [NVIDIA VIDEO CODEC SDK web site](https://developer.nvidia.com/video-encode-decode-gpu-support-matrix#Encoder).

This package depends on **NVIDIA Video Codec SDK 9.1**. Please check the graphics driver version.
- Windows: Driver version `436.15` or higher
- Linux:   Driver version `435.21` or higher

> [!NOTE]
> On Linux, `libc++1` `libc++abi1` packages should be installed.
> Please install like command below 
>
> ``` sudo apt install -y libc++1 libc++abi1 ```

## Installation

Please see [Install package](install.md).

## Samples

The package contains the following samples. 

| Scene                   | Details                                        |
| ----------------------- | ---------------------------------------------- |
| PeerConnection          | Checking the process of connecting to a peer   |
| DataChannel             | Sending and receiving text                     |
| MediaStream             | Sending and receiving video/audio              |
| Stats                   | Checking the process of getting stats          |
| MungeSDP                | Checking effects with mungring SDP parameters  |
| VideoReceive            | Sending and receiving video stream             |
| MultiVideoReceive       | Receiving multiple video streams with one peer |
| MultiplePeerConnections | Receiving video stream with multiple peers     |
| ChangeCodecs            | Controlling codecs of the video sender         |
| TricleIce               | Checking the trickle ICE functionality         |

To get these samples, Push the `Import into Project` button on Package Manager.

![Download package sample](images/download_package_sample.png)