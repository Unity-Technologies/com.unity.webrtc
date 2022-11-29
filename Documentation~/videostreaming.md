# Video streaming

WebRTC enables streaming video between peers. It can stream video rendered by Unity to multiple browsers at the same time.

> [!NOTE]
> The [package samples](sample.md) contains the **PeerConnection** scene which demonstrates video streaming features of the package.

## Invoke Update with [`StartCoroutine`](https://docs.unity3d.com/ScriptReference/MonoBehaviour.StartCoroutine.html)

First, you need to invoke [`WebRTC.Update`](../api/Unity.WebRTC.WebRTC.html#Unity_WebRTC_WebRTC_Update) method with [`StartCoroutine`](https://docs.unity3d.com/ScriptReference/MonoBehaviour.StartCoroutine.html) because this method copies textures to video buffer per frame.

```CSharp
void Start()
{
    StartCoroutine(WebRTC.Update());
}
```

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
> - It's impossible to send and receive video in a single [`VideoStreamTrack`](../api/Unity.WebRTC.VideoStreamTrack.html) instance.
> - The [`VideoStreamTrack`](../api/Unity.WebRTC.VideoStreamTrack.html) used to receive the video should be the track received in the event of the [`RTCPeerConnection.OnTrack`](../api/Unity.WebRTC.RTCPeerConnection.html#Unity_WebRTC_RTCPeerConnection_OnTrack).

## Video configuration

Developers can control properties about video streaming quality in real-time.

- Bitrate
- Frame rate
- Video resolution

> [!NOTE]
> If you want to check these features, the **Bandwidth** scene in the package sample is good to learn.

### Bitrate control

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

### Frame rate control

Developers can also change the encoding frame rate using [`SetParameters`](../api/Unity.WebRTC.RTCRtpSender.html#Unity_WebRTC_RTCRtpSender_SetParameters_) method. The example code below shows how to change the frame rate of the video encoder. You should set this parameter lower than [`Application.targetFramerate`](https://docs.unity3d.com/ScriptReference/Application-targetFrameRate.html).

```CSharp
// Get `RTCRtpSendParameters`
var parameters = sender.GetParameters();

// Changing framerate of all encoders.
foreach (var encoding in parameters.Encodings)
{
    // Change encoding frequency 30 frame per second.
    encoding.maxFramerate = 30;
}

// Set updated parameters.
sender.SetParameters(parameters);
```

### Video resolution control

You can also change the video resolution to reduce network traffic. The [`scaleResolutionDownBy`](../api/Unity.WebRTC.RTCRtpEncodingParameters.html#Unity_WebRTC_RTCRtpEncodingParameters_scaleResolutionDownBy) property in [`RTCRtpEncodingParameters`](../api/Unity.WebRTC.RTCRtpEncodingParameters.html) class can resize the video resolution. The type of property is **float** which represents the factor of the size. If you set **2.0** for this value, it reduces the size of the video, the result's resolution **25%** of the original one.

```CSharp
// Get `RTCRtpSendParameters`
var parameters = sender.GetParameters();

// Changing framerate of all encoders.
foreach (var encoding in parameters.Encodings)
{
    // Change video size to half.
    encoding.scaleResolutionDownBy = 2.0f;
}

// Set updated parameters.
sender.SetParameters(parameters);
```

### Checking bitrate

It's possible to check the current bitrate on browsers. If using Google Chrome, shows statistics of WebRTC by accessing the address `chrome://webrtc-internals`. Check the graph showing the received bytes per unit time in the category `RTCInboundRTPVideoStream` of statistics.

![Chrome WebRTC Stats](images/chrome-webrtc-stats.png)

## Video codec

You can choose from several codecs to use in this package. Noted that the available codecs vary by platform.

### Selecting video codec

To select video codec, first call [`GetCapabilities`](../api/Unity.WebRTC.RTCRtpSender.html#Unity_WebRTC_RTCRtpSender_GetCapabilities_Unity_WebRTC_TrackKind_) method to get a list of available codecs on the device. By default, all available codecs are used for negotiation with other peers. When negotiating between peers, each peers search for codecs that are commonly available to both peers in order of priority. Therefore, developers need to filter and sort the video codec list to explicitly select codecs to use. The following example extracts the H.264 codecs from the list of available codecs.

```CSharp
// Get all available video codecs.
var codecs = RTCRtpSender.GetCapabilities(TrackKind.Video).codecs;

// Filter codecs.
var h264Codecs = codecs.Where(codec => codec.mimeType == "video/H264");
```

Create a list of codecs to use and pass it to the [`SetCodecPreferences`](../api/Unity_WebRTC_RTCRtpTransceiver_SetCodecPreferences_Unity_WebRTC_RTCRtpCodecCapability___) method. If an unavailable codec is passed, [`InvalidParameter`](../api/Unity.WebRTC.RTCErrorType.html) is returned.

```CSharp
var error = transceiver.SetCodecPreferences(h264Codecs.ToArray());
if (error != RTCErrorType.None)
    Debug.LogError("SetCodecPreferences failed");
```

### Video Encoder

There are two types of encoder for video streaming, one is using hardware for encoding and one is using software. Regarding different kinds of codecs, the hardware encoder uses `H.264`, and the software encoder uses `VP8`, `VP9`, `AV1`.

### Video Decoder

As with video encoders, we offer hardware-intensive `H.264` decoders and non-hardware-intensive `VP8`, `VP9`, and `AV1` decoders.

### Hardware acceleration codecs

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

**NVCODEC** is available on the PC installed NVIDIA driver.
- Windows: NVIDIA Driver version `456.71` or higher
- Linux:   NVIDIA Driver version `455.27` or higher

To check the compatible NVIDIA graphics card, please visit on the [NVIDIA VIDEO CODEC SDK web site](https://developer.nvidia.com/video-encode-decode-gpu-support-matrix#Encoder).