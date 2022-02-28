# Video streaming

WebRTC enables streaming video between peers. It can stream video rendered by Unity to multiple browsers at the same time.

> [!NOTE]
> The [package samples](sample.md) contains the **PeerConnection** scene which demonstrates video streaming features of the package.

## Sending video

To implement video streaming, create a [`VideoStreamTrack`](../api/Unity.WebRTC.VideoStreamTrack.html) instance.

```CSharp
// Create a track from the Camera
var camera = GetComponnent<Camera>();
var track = camera.CaptureStreamTrack(1280, 720);
```

There is also a way to directly assign a [`RenderTexture`](https://docs.unity3d.com/ScriptReference/RenderTexture.html). 

```CSharp
// Get a valid RendertextureFormat
var gfxType = SystemInfo.graphicsDeviceType;
var format = WebRTC.GetSupportedRenderTextureFormat(gfxType);

// Create a track from the RenderTexture
var rt = new RenderTexture(width, height, 0, format);
var track = new VideoStreamTrack("video", renderTexture);
```

### Add track

Add the created video track to the [`RTCPeerConnection`](../api/Unity.WebRTC.RTCPeerConnection.html) instance. The track can be added by calling the [`AddTrack`](../api/Unity.WebRTC.RTCPeerConnection.html#Unity_WebRTC_RTCPeerConnection_AddTrack_) method. Next, call the [`CreateOffer`](../api/Unity.WebRTC.RTCPeerConnection.html#Unity_WebRTC_RTCPeerConnection_CreateOffer) or [`CreateAnswer`](../api/Unity.WebRTC.RTCPeerConnection.html#Unity_WebRTC_RTCPeerConnection_CreateAnswer) to create an SDP.

```CSharp
// Add the track.
peerConnection.AddTrack(track);

// Create the SDP.
RTCAnswerOptions options = default;
var op = pc.CreateAnswer(ref options);
yield return op;
```

### Multi track

It's possible to use multiple video tracks simultaneously. Simply call the [`AddTrack`](../api/Unity.WebRTC.RTCPeerConnection.html#Unity_WebRTC_RTCPeerConnection_AddTrack_) method multiple times and add the tracks. 

```CSharp
foreach(var track in listTrack)
{
    peerConnection.AddTrack(track);
}
```

> [!NOTE]
> When using hardware encoding, the number of tracks that can be used simultaneously may be limited depending on the graphic device's limitations. Generally, on desktop GPUs, up to **two tracks** can be used simultaneously on an NVIDIA Geforce card (On server-grade GPUs this is typically 4). For details, see the [NVIDIA Codec SDK documentation](https://developer.nvidia.com/video-encode-decode-gpu-support-matrix).

## Receiving video

You can use [`VideoStreamTrack`](../api/Unity.WebRTC.VideoStreamTrack.html) to receive the video.
The class for receiving video is got on [`OnTrack`](../api/Unity.WebRTC.RTCPeerConnection.html#Unity_WebRTC_RTCPeerConnection_OnTrack) event of the [`RTCPeerConnection`](../api/Unity.WebRTC.RTCPeerConnection.html) instance.
If the type of [`MediaStreamTrack`](../api/Unity.WebRTC.MediaStreamTrack.html) argument of the event is [`TrackKind.Video`](../api/Unity.WebRTC.TrackKind.html), The [Track]  instance is able to be casted to the [`VideoStreamTrack`](../api/Unity.WebRTC.VideoStreamTrack.html) class.

```CSharp

var receiveStream = new MediaStream();
receiveStream.OnAddTrack = e => {
    if (e.Track is VideoStreamTrack track)
    {
        // You can access received texture using `track.Texture` property.
    }
    else if(e.Track is AudioStreamTrack track)
    {
        // This track is for audio.
    }
}

var peerConnection = new RTCPeerConnection();
peerConnection.OnTrack = (RTCTrackEvent e) => {
    if (e.Track.Kind == TrackKind.Video)
    {
        // Add track to MediaStream for receiver.
        // This process triggers `OnAddTrack` event of `MediaStream`.
        receiveStream.AddTrack(e.Track);
    }
};
```

### Receiving multi video

Multiple VideoTracks can be received in a single [`RTCPeerConnection`](../api/Unity.WebRTC.RTCPeerConnection.html).
It is a good idea to call the [`AddTransceiver`](../api/Unity.WebRTC.RTCPeerConnection.html#Unity_WebRTC_RTCPeerConnection_AddTransceiver_) method on the [`RTCPeerConnection`](../api/Unity.WebRTC.RTCPeerConnection.html) instance as needed track count, and then do signaling.

```CSharp
// Call AddTransceiver as needed track count.
peerConnection.AddTransceiver(TrackKind.Video);
// Do process signaling
```

> [!NOTE]
> - It is not possible to send and receive video in a single [`VideoStreamTrack`](../api/Unity.WebRTC.VideoStreamTrack.html) instance.
> - The [`VideoStreamTrack`](../api/Unity.WebRTC.VideoStreamTrack.html) used to receive the video should be the track received in the event of the [`RTCPeerConnection.OnTrack`](../api/Unity.WebRTC.RTCPeerConnection.html#Unity_WebRTC_RTCPeerConnection_OnTrack).

## Bitrate control

To control the bitrate of video streaming, use [`SetParameters`](../api/Unity.WebRTC.RTCRtpSender.html#Unity_WebRTC_RTCRtpSender_SetParameters_) method of [`RTCRtpSender`](../api/Unity.WebRTC.RTCRtpSender.html) instance. The instance of [`RTCRtpSender`](../api/Unity.WebRTC.RTCRtpSender.html) is obtained from [`RTCPeerConnection`](../api/Unity.WebRTC.RTCPeerConnection.html). Or, obtained from [`AddTrack`](../api/Unity.WebRTC.RTCPeerConnection.html#Unity_WebRTC_RTCPeerConnection_AddTrack_) method as its return value.

```CSharp
// Get from `GetSenders` method.
var senders = peerConnection.GetSenders();

// Get from `AddTrack` method.
var sender = peerConnection.AddTrack(track);
```

After obtained the instance of [`RTCRtpSender`](../api/Unity.WebRTC.RTCRtpSender.html), to get the settings about the sending stream, call the [`GetParameters`](../api/Unity.WebRTC.RTCRtpSender.html#Unity_WebRTC_RTCRtpSender_GetParameters) method is able. The returning value is defined as [`RTCRtpSendParameters`](../api/Unity.WebRTC.RTCRtpSendParameters.html) class. And call the [`SetParameters`](../api/Unity.WebRTC.RTCRtpSender.html#Unity_WebRTC_RTCRtpSender_SetParameters_) method with customized settings. as a result, the settings are reflected.

```CSharp
// Get `RTCRtpSendParameters`
var parameters = sender.GetParameters();

// Changing bitrate of all encoders.
foreach (var encoding in parameters.Encodings)
{
    encoding.maxBitrate = bitrate;
}

// Set updated parameters.
sender.SetParameters(parameters);
```

> [!NOTE]
> Currently not supported [`maxFramerate`](../api/Unity.WebRTC.RTCRtpEncodingParameters.html#Unity_WebRTC_RTCRtpEncodingParameters_maxFramerate) in values of the settings.
>

### Checking bitrate

It is possible to check the current bitrate on browsers. If using Google Chrome, shows statistics of WebRTC by accessing the URL `chrome://webrtc-internals`. Check the graph showing the received bytes per unit time in the category `RTCInboundRTPVideoStream` of statistics.

![Chrome WebRTC Stats](images/chrome-webrtc-stats.png)

## Video codec

You can choose from several codecs to use in this package. Noted that the available codecs vary by platform.

### Encode

There are two types of encoder for video streaming, one is using hardware for encoding and one is using software. Regarding different kinds of codecs, the hardware encoder uses `H.264`, and the software encoder uses `VP8`, `VP9`, `AV1`.

### Decode

Currently, only SoftwareDecoder can be used, in which case VP8/VP9 can be used as a codec.
In this case, `VP8` or `VP9` can be used as a codec.

### HWA Codec

For codecs that support hardware acceleration, the following codecs are supported.

- [**NVCODEC**](https://developer.nvidia.com/nvidia-video-codec-sdk)
- **VideoToolbox**
- **MediaCodec**

The encoders that support hardware acceleration are shown below.

| Platform | Graphics API | NVCODEC | VideoToolbox | MediaCodec |
| -------- | ------------ | ----- | ------------ | ---------- |
| Windows x64 | DirectX11    | **Y** | - | - |
| Windows x64 | DirectX12    | **Y** | - | - |
| Windows x64 | OpenGL Core | N | - | - |
| Windows x64 | Vulkan | **Y** | - | - |
| Linux x64   | OpenGL Core | **Y** | - | - |
| Linux x64   | Vulkan | **Y** | - | - |
| MacOS | Metal | - | **Y** | - |
| iOS | Metal | - | **Y** | - |
| Android | Vulkan | - | - | **Y** |
| Android | OpenGL ES | - | - | **Y** |

The decoders that support hardware acceleration are shown below.

| Platform | Graphics API | NVCODEC | VideoToolbox | MediaCodec |
| -------- | ------------ | ----- | ------------ | ---------- |
| Windows x64 | DirectX11    | N | - | - |
| Windows x64 | DirectX12    | N | - | - |
| Windows x64 | OpenGL Core | N | - | - |
| Windows x64 | Vulkan | N | - | - |
| Linux x64   | OpenGL Core | N | - | - |
| Linux x64   | Vulkan | N | - | - |
| MacOS | Metal | - | **Y** | - |
| iOS | Metal | - | **Y** | - |
| Android | Vulkan | - | - | **Y** |
| Android | OpenGL ES | - | - | **Y** |

**NVCODEC** is available on the PC installed NVIDIA driver.
- Windows: NVIDIA Driver version `456.71` or higher
- Linux:   NVIDIA Driver version `455.27` or higher

To check the compatible NVIDIA graphics card, please visit on the [NVIDIA VIDEO CODEC SDK web site](https://developer.nvidia.com/video-encode-decode-gpu-support-matrix#Encoder).

### Enabling hardware acceleration

We can select the type of encoder by specifying the [`EncoderType`](../api/api/Unity.WebRTC.EncoderType.html) in [`WebRTC.Initialize`](../api/Unity.WebRTC.WebRTC.html#Unity_WebRTC_WebRTC_Initialize_Unity_WebRTC_EncoderType_System_Boolean_System_Boolean_Unity_WebRTC_NativeLoggingSeverity_) method argument.

```CSharp
// Enable a hardware acceleration.
WebRTC.Initialize(EncoderType.Hardware);
```