# ビデオストリーミング

- [コーデック](#codec)
- [ビデオトラック](#video-track)
  - [トラックの追加](#add-track)
  - [マルチトラック](#multi-track)

WebRTC はピア間での映像のストリーミングを可能にします。 Unity でレンダリングされた映像を同時に複数のブラウザに配信することが可能です。

## <a id="codec"/> コーデック

ビデオストリーミングで利用するエンコーダーには、ハードウェアでエンコードするものと、ソフトウェアでエンコードするものがあります。利用するコーデックは、ハードウェアエンコーダーの場合には `H.264` を利用し、ソフトウェアエンコーダーの場合は、`VP8` コーデックを利用します。

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

## <a id="video-track"/> ビデオトラック

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

### <a id="add-track"/> トラックの追加

生成したビデオトラックを `RTCPeerConnection` のインスタンスに追加します。`AddTrack` メソッドを呼び出すことでトラックを追加できます。その後 SDP を生成するために `RTCPeerConnection` の `CreateOffer` もしくは `CreateAnswer` を呼び出します。

```CSharp
// トラックを追加
peerConnection.AddTrack(track);

// SDP を生成
RTCAnswerOptions options = default;
var op = pc.CreateAnswer(ref options);
yield return op;
```

### <a id="multi-track"/> マルチトラック

ビデオトラックは複数同時に利用することが可能です。 `RTCPeerConnection` の `AddTrack` メソッドを複数回呼び出してトラックを追加します。

```CSharp
// 複数のトラックを追加
foreach(var track in listTrack)
{
    peerConnection.AddTrack(track);
}
```

ハードウェアエンコーダーを選択している場合、グラフィックデバイスの制約によって、同時に利用可能なトラック数が制限される場合があります。一般的に NVIDIA Geforce で同時に利用可能なビデオトラック数は **2本** までです。詳しくは [NVDIA Codec SDK のドキュメント](https://developer.nvidia.com/video-encode-decode-gpu-support-matrix) を参照してください。

ブラウザ側でトラックを同時に受信する方法については、MDN ドキュメント [`RTCPeerConnection.addTrack`](https://developer.mozilla.org/en-US/docs/Web/API/RTCPeerConnection/addTrack) の **Streamless tracks** の項目を参照してください。

