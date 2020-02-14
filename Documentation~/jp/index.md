# WebRTC

![WebRTC header](../images/webrtc_header.png)

WebRTC for Unity は、 [WebRTC](https://webrtc.org) を Unity で利用可能にするためのパッケージです。

このパッケージを利用することで、ブラウザとの連携が可能になります。
[Unity Render Streaming パッケージ](https://docs.unity3d.com/Packages/com.unity.renderstreaming@1.1/manual/jp/index.html) は、WebRTC を利用したサンプルを提供しています。

## 動作要件

以下の Unity バージョンに対応しています。

- **Unity 2019.3**

| Platform    | Graphics API | Hardware Encoder                                                                                                            | Software Encoder   |
| ----------- | ------------ | --------------------------------------------------------------------------------------------------------------------------- | ------------------ |
| Windows x64 | DirectX11    | :white_check_mark: (NVIDIA の[グラフィックスカード](https://developer.nvidia.com/video-encode-decode-gpu-support-matrix)が必要) | :white_check_mark: | 
| Windows x64 | DirectX12    |                                                                                                                             |                    | 
| Windows x64 | OpenGL       |                                                                                                                             |                    |
| Windows x64 | Vulkan       |                                                                                                                             |                    | 
| Linux x64   | OpenGL       | :white_check_mark: (NVIDIA の[グラフィックスカード](https://developer.nvidia.com/video-encode-decode-gpu-support-matrix)が必要) |                    |
| Linux x64   | Vulkan       |                                                                                                                             |                    |
| MacOS       | OpenGL       |                                                                                                                             |                    |
| MacOS       | Metal        |                                                                                                                             | :white_check_mark: |

対応している NVIDIA のグラフィックスカードについては、[NVIDIA VIDEO CODEC SDK](https://developer.nvidia.com/video-encode-decode-gpu-support-matrix#Encoder) のページをご覧ください。

> [!NOTE]
> Linux で動作させる場合は、 `libc++1` `libc++abi1` をインストールする必要があります。
>
> ``` sudo apt install -y libc++1 libc++abi1 ```


## インストール方法
パッケージをインストールするためには、パッケージマネージャーから WebRTC for Unity を検索しインストールします。パッケージマネージャーの利用方法は[ドキュメント](https://docs.unity3d.com/Manual/upm-ui.html)を参照してください。

![WebRTC Package Manager](../images/webrtc_package_manager.png)

## サンプル

パッケージでは以下のサンプルを用意しています。

| シーン名        | 説明                                  |
| -------------- | ------------------------------------ |
| PeerConnection | ピアを接続する手続きを確認するシーン      |
| DataChannel    | テキスト送受信を確認するシーン           |
| MediaStream    | ビデオ/音声送信を確認するシーン          |

サンプルを入手するには、 Package Manager の `Import into Project` ボタンを押してください。

![Download package sample](../images/download_package_sample.png)


## その他の表示言語

- [English](../index.md)
