# Audio Streaming

In order to stream audio, first you need to get the stream instance. Call `Audio.CaptureStream()`.

```csharp
audioStream = Audio.CaptureStream();
```

Add the audio track to the peer. `RTCRtpSender` will be used when discarding media. 

```csharp
    var senders = new List<RTCRtpSender>();
    foreach (var track in audioStream.GetTracks())
    {
        var sender = localConnection.AddTrack(track);
        senders.Add(sender);
    }
```

Use the `RemoveTrack` method to discard the media.

```csharp
    foreach(var sender in senders)
    {
        localConnection.RemoveTrack(sender);
    }
```

Call the `Audio`'s `Update` method inside the `MonoBehaviour`'s `OnAudioFilterRead` method.

```csharp
    private void OnAudioFilterRead(float[] data, int channels)
    {
        Audio.Update(data, data.Length);
    }
```

> [!NOTE]
> As with the `AudioListener` component, when using the `OnAudioFilterRead` method, it must be associated with a GameObject.

Alternatively, `AudioRenderer` can also be used.

```csharp

    private void Start()
    {
        AudioRenderer.Start();
    }

    private void Update()
    {
        var sampleCountFrame = AudioRenderer.GetSampleCountForCaptureFrame();
        var channelCount = 2; // AudioSettings.speakerMode == Stereo
        var length = sampleCountFrame * channelCount;
        var buffer = new NativeArray<float>(length, Allocator.Temp);
        AudioRenderer.Render(buffer);
        Audio.Update(buffer.ToArray(), buffer.Length);
        buffer.Dispose();
    }

```
