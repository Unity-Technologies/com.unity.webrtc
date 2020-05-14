# ビデオストリーミング

WebRTC はピア間での映像のストリーミングを可能にします。 Unity でレンダリングされた映像を同時に複数のブラウザに配信することが可能です。

## コーデック

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

## ビデオトラック

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

### トラックの追加

生成したビデオトラックを `RTCPeerConnection` のインスタンスに追加します。`AddTrack` メソッドを呼び出すことでトラックを追加できます。その後 SDP を生成するために `RTCPeerConnection` の `CreateOffer` もしくは `CreateAnswer` を呼び出します。

```CSharp
// トラックを追加
peerConnection.AddTrack(track);

// SDP を生成
RTCAnswerOptions options = default;
var op = pc.CreateAnswer(ref options);
yield return op;
```

### マルチトラック

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

### 帯域制御

ビデオトラックの帯域を制御するには、 `RTCRtpSender` の `SetParameter` メソッドを利用します。`RTCRtpSender` は `RTCPeerConnection` から取得することができます。

```CSharp
var senders = peerConnection.GetSenders();
```

あるいは `AddTrack` の戻り値として取得できます。

```CSharp
var sender = peerConnection.AddTrack(track);
```

`RTCRtpSender` のインスタンスを取得後、 `GetParameter` メソッドを呼び出すと、現在の送信に関する設定を取得できます。 この設定情報を書き換えて `SetParameter` を呼び出すと、値を反映させることができます。

```CSharp
var parameters = sender.GetParameters();
foreach (var encoding in parameters.Encodings)
{
    encoding.maxBitrate = bitrate;
}
sender.SetParameters(parameters);
```

> [!NOTE]
> 設定に含まれる値の中で、 `maxFramerate` は現在未対応です。
> `scaleResolutionDownBy` は、ソフトウェアエンコーダでのみ動作します。
>

現在使用している帯域はブラウザ上で確認できます。Google Chrome では `chrome://webrtc-internals` にアクセスすると、現在動作している WebRTC の各種統計情報が表示されます。その中の `RTCInboundRTPVideoStream` の項目にある、単位時間当たりの受信バイト数のグラフ（`[bytesReceived_in_bits/s]`）をご覧ください。

![Chrome WebRTC Stats](../images/chrome-webrtc-stats.png)

