# Video Streaming

WebRTC enables streaming video between peers. It can stream video rendered by Unity to multiple browsers at the same time.

## Codec

There are two types of encoder for video streaming, one is using hardware for encoding and one is using software. Regarding different kinds of codecs, the hardware encoder uses `H.264`, and the software encoder uses `VP8`.

We can select the type of encoder by specifying the EncoderType in WebRTC.Initialize's method argument.

```CSharp
// Use a software encoder
WebRTC.Initialize(EncoderType.Software);
```

> [!NOTE]
> This option selects whether or not to use hardware for encoding.
> Currently, there is no way to explicitly designate a codec. 



The major browsers that support WebRTC can use `H.264` and `VP8`, which means most browsers can recieve video streaming from Unity.

## <a id="videotrack"/> Video Track

To implement video streaming, create a
 `VideoStreamTrack` instance.

```CSharp
// Create a track from the Camera
var camera = GetComponnent<Camera>();
var track = camera.CaptureStreamTrack(1280, 720);
```

There is also a way to directly assign a `RenderTexture`. 

```CSharp
// Get a valid RendertextureFormat
var gfxType = SystemInfo.graphicsDeviceType;
var format = WebRTC.GetSupportedRenderTextureFormat(gfxType);

// Create a track from the RenderTexture
var rt = new RenderTexture(width, height, 0, format);
var track = new VideoStreamTrack("video", renderTexture);
```

### <a id="add-track"/> Add Track

Add the created video track to the `PeerConnection` instance. The track can be added by calling the `AddTrack` method. Next, call the `PeerConnection`'s `CreateOffer` or `CreateAnswer` to create an SDP.

```CSharp
// Add the track
peerConnection.AddTrack(track);

// Create the SDP
RTCAnswerOptions options = default;
var op = pc.CreateAnswer(ref options);
yield return op;
```

### <a id="multi-track"/> Multi track

It's possible to use multiple video tracks simultaneously. Simply call the `PeerConnection`'s `AddTrack` method multiple times and add the tracks. 

```CSharp
foreach(var track in listTrack)
{
    peerConnection.AddTrack(track);
}
```

When using hardware encoding, the number of tracks that can be used simultaneously may be limited depending on the graphic device's limitations. Generally, on desktop GPUs, up to **two tracks** can be used simultaneously on an NVIDIA Geforce card (On server-grade GPUs this is typically 4). For details, see the [NVDIA Codec SDK documentation](https://developer.nvidia.com/video-encode-decode-gpu-support-matrix).


See the section on **Streamless tracks** under [`PeerConnection.addTrack`](https://developer.mozilla.org/en-US/docs/Web/API/RTCPeerConnection/addTrack) in the MDN documentation for information on simultaneously receiving multiple tracks in the browser. 
