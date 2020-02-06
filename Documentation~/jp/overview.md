# 概要

## 動作要件

以下の Unity バージョンに対応しています。

- **Unity 2019.3**

| Platform    | Graphics API | Hardware Encoder                                     | Software Encoder   |
| ----------- | ------------ | ---------------------------------------------------- | ------------------ |
| Windows x64 | DirectX11    | :white_check_mark: (NVIDIA のグラフィックスカードが必要) | :white_check_mark: | 
| Windows x64 | DirectX12    |                                                      |                    | 
| Windows x64 | OpenGL       |                                                      |                    |
| Windows x64 | Vulkan       |                                                      |                    | 
| Linux x64   | OpenGL       | :white_check_mark: (NVIDIA のグラフィックスカードが必要) |                    |
| Linux x64   | Vulkan       |                                                      |                    |
| MacOS       | OpenGL       |                                                      |                    |
| MacOS       | Metal        |                                                      | :white_check_mark: |

対応している NVIDIA のグラフィックスカードについては、[NVIDIA VIDEO CODEC SDK のウェブサイト](https://developer.nvidia.com/video-encode-decode-gpu-support-matrix#Encoder)をご覧ください。

## インストール方法
パッケージをインストールするためには、パッケージマネージャーから WebRTC for Unity を検索しインストールします。パッケージマネージャーの利用方法は[ドキュメント](https://docs.unity3d.com/Manual/upm-ui.html)を参照してください。

![WebRTC Package Manager](../images/webrtc_package_manager.png)

## サンプル

パッケージでは以下の 3 つのサンプルを用意しています。

| シーン名        | 説明                                 |
| -------------- | ------------------------------------ |
| PeerConnection | ピアを接続する手続きを確認するシーン      |
| DataChannel    | テキスト送受信を確認するシーン           |
| MediaStream    | ビデオ/音声送受信を確認するシーン        |