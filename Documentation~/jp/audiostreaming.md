# オーディオストリーミング

オーディオをストリーミングするためには、はじめにストリームのインスタンスを取得します。`Audio.CaptureStream()` を呼び出してください。

```csharp
audioStream = Audio.CaptureStream();
```

ピアにオーディオトラックを追加します。`RTCRtpSender` のインスタンスは、メディアを破棄する際に利用します。

```csharp
    var senders = new List<RTCRtpSender>();
    foreach (var track in audioStream.GetTracks())
    {
        var sender = localConnection.AddTrack(track);
        senders.Add(sender);
    }
```

メディアの破棄は、 `RemoveTrack` メソッドを使用します。

```csharp
    foreach(var sender in senders)
    {
        localConnection.RemoveTrack(sender);
    }
```

`MonoBehaviour` の `OnAudioFilterRead` メソッド内で、`Audio` の `Update` メソッドを呼び出してください。

```csharp
    private void OnAudioFilterRead(float[] data, int channels)
    {
        Audio.Update(data, data.Length);
    }
```

> [!NOTE]
> `OnAudioFilterRead` メソッドを利用する場合は、 `AudioListener` コンポーネントと同じ GameObject に関連付ける必要があります。

あるいは、`AudioRenderer` を利用する方法もあります。

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