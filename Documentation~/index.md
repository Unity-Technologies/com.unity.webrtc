# WebRTC

- [Japanese](./jp/index.md)

**WebRTC for Unity** is a package that allows [WebRTC](https://webrtc.org) to be used in Unity.

If you are interested in the streaming solution with WebRTC, you can check [Unity Render Streaming](https://github.com/Unity-Technologies/UnityRenderStreaming). 

## Guide

* [Tutorial](./en/tutorial.md)

## Installation
To install the package, download WebRTC for Unity from the package manager. See the [documentation](https://docs.unity3d.com/Packages/com.unity.package-manager-ui@latest/index.html) for details on how to use the package manager. 

<img src="./images/webrtc_package_manager.png" width=600 align=center>

## Samples
The package contains the following 3 samples. 

| Scene       | Details                                 |
| -------------- | ------------------------------------ |
| PeerConnection | A scene for checking the process of connecting to a peer |
| DataChannel    | A scene for sending and receiving text       |
| MediaStream    | A scene for sending and receiving video/audio    |

## Requirements

This version of the package is compatible with the following versions of the Unity Editor:

- 2019.1 and later (recommended)

Currently the software only supports `windows64`.

Graphics API version only supports `Direct3D11`.

`IL2CPP` is not supported in Scripting Backend by this package.

## Limitations

This package uses GPU hardware acceleration for video encoding, so it only runs on graphics cards that support [NVIDIA VIDEO CODEC SDK](https://developer.nvidia.com/nvidia-video-codec-sdk).

## Update History

|Date|Reason|
|---|---|
|June 21, 2019|Document Released|
