# Video Streaming

- [Codec](#codec)
- [Video Track](#videotrack)
  - [Add Track](#add-track)
  - [Multi track](#multi-track)

WebRTC はピア間での映像のストリーミングを可能にします。 Unity でレンダリングされた映像を同時に複数のブラウザに配信することが可能です。

## <a id="codec"/> Codec

ビデオストリーミングで利用するエンコーダーには、ハードウェアで処理するものと、ソフトウェアで処理するものがあります。ハードウェアエンコーダーとしては `H.264` が利用可能です。ソフトウェアエンコーダーを利用する場合は、`VP8` あるいは `VP9` を利用します。

`WebRTC.Initialize` メソッドの引数に `EncoderType` を指定することで、
ソフトウェアエンコーダーとハードウェアエンコーダーのいずれかを選択することができます。

```CSharp
// ソフトウェアエンコーダーを使用
WebRTC.Initialize(EncoderType.Software);
```

> [!NOTE]
> このオプションはハードウェアを利用する/利用しないを選択するオプションです。
> コーデックを明示的に指定する方法は、現在提供していません。



WebRTC をサポートしている主要なブラウザでは `H.264` 及び `VP8` が利用できるため、多くのブラウザで Unity から配信されるビデオストリーミングを受信することができます。

## <a id="videotrack"/> Video Track

ビデオストリーミングを実装するには、ビデオトラック
 `VideoStreamTrack` のインスタンスを生成します。

```CSharp
// Camera からトラックを生成
var camera = GetComponnent<Camera>();
var track = camera.CaptureStreamTrack(1280, 720);
```

`RenderTexture` を直接指定する方法もあります。

```CSharp
// 有効な RendertextureFormat を取得
var gfxType = SystemInfo.graphicsDeviceType;
var format = WebRTC.GetSupportedRenderTextureFormat(gfxType);

// RenderTexture からトラックを生成
var rt = new RenderTexture(width, height, 0, format);
var track = new VideoStreamTrack("video", renderTexture);
```

### <a id="add-track"/> Add Track

生成したビデオトラックを `PeerConnection` のインスタンスに追加します。`AddTrack` メソッドを呼び出すことでトラックを追加できます。その後 SDP を生成するために `PeerConnection` の `CreateOffer` もしくは `CreateAnswer` を呼び出します。

```CSharp
// トラックを追加
peerConnection.AddTrack(track);

// SDP を生成
RTCAnswerOptions options = default;
var op = pc.CreateAnswer(ref options);
yield return op;
```

### <a id="multi-track"/> Multi track

ビデオトラックは複数同時に利用することが可能です。 `PeerConnection` の `AddTrack` メソッドを複数回呼び出してトラックを追加します。

```CSharp
foreach(var track in listTrack)
{
    peerConnection.AddTrack(track);
}
```

ハードウェアエンコーダーを選択している場合、グラフィックデバイスの制約によって、同時に利用可能なトラック数が制限される場合があります。一般的に NVIDIA Geforce で同時に利用可能なビデオトラック数は **2本** までです。詳しくは [NVDIA Codec SDK のドキュメント](https://developer.nvidia.com/video-encode-decode-gpu-support-matrix) を参照してください。

ブラウザ側でトラックを同時に受信する方法については、MDN ドキュメント [`PeerConnection.addTrack`](https://developer.mozilla.org/en-US/docs/Web/API/RTCPeerConnection/addTrack) の **Streamless tracks** の項目を参照してください。

