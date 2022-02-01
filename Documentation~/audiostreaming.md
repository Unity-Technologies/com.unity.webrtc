# Audio streaming

This package provides the [`AudioStreamTrack`](../api/Unity.WebRTC.AudioStreamTrack.html) class for streaming audio, allowing the sending and receiving of [`AudioSource`](https://docs.unity3d.com/ScriptReference/AudioSource.html). Also, by using the [`SetData`](../api/Unity.WebRTC.AudioStreamTrack.html#Unity_WebRTC_AudioStreamTrack_SetData_) method, raw audio data other than [`AudioSource`](https://docs.unity3d.com/ScriptReference/AudioSource.html) can be sent.

> [!NOTE]
> The [package samples](sample.md) contains the **Audio** scene which demonstrates audio features of the package.

## Sending audio

In order to stream audio, first you need to get the [`AudioStreamTrack`](../api/Unity.WebRTC.AudioStreamTrack.html) instance.

```csharp
// Create `AudioStreamTrack` instance with `AudioSource`.
var inputAudioSource = GetComponent<AudioSource>();
var track = new AudioStreamTrack(inputAudioSource);

// Add a track to the `RTCPeerConnection` instance.
var sendStream = new MediaStream();
var sender = peerConnection.AddTrack(track, sendStream);
```

The [`RTCRtpSender`](../api/Unity.WebRTC.RTCRtpSender.html) instance can use with the [`RemoveTrack`](../api/Unity.WebRTC.RTCPeerConnection.html#Unity_WebRTC_RTCPeerConnection_RemoveTrack_) method to discard the track.

```csharp
// Remove a track from the `RTCPeerConnection` instance.
peerConnection.RemoveTrack(sender);
```

There are two types of [`AudioStreamTrack`](../api/Unity.WebRTC.AudioStreamTrack.html) constructors: one with an [`AudioSource`](https://docs.unity3d.com/ScriptReference/AudioSource.html) argument, and one with no argument. Use the one with no argument if you want to use [`AudioListener`](https://docs.unity3d.com/ScriptReference/AudioListener.html).

The [`SetData`](../api/Unity.WebRTC.AudioStreamTrack.html#Unity_WebRTC_AudioStreamTrack_SetData_) method is used to send audio data; [`SetData`](../api/Unity.WebRTC.AudioStreamTrack.html#Unity_WebRTC_AudioStreamTrack_SetData_) is automatically called internally when [`AudioSource`](https://docs.unity3d.com/ScriptReference/AudioSource.html) is passed to the constructor, but when the constructor has no arguments, [`SetData`](../api/Unity.WebRTC.AudioStreamTrack.html#Unity_WebRTC_AudioStreamTrack_SetData_) must be called. Note that the [`SetData`](../api/Unity.WebRTC.AudioStreamTrack.html#Unity_WebRTC_AudioStreamTrack_SetData_) method is supposed to be called on the audio thread, not the main thread. See [`OnAudioFilterRead`](https://docs.unity3d.com/ScriptReference/MonoBehaviour.OnAudioFilterRead.html) for details.

```csharp
[RequireComponent(typeof(AudioListener))]
class AudioSender : MonoBehaviour
{
    AudioStreamTrack track;
    const int sampleRate = 48000;

    // The initialization process have been omitted for brevity.

    // This method is called on the audio thread.
    private void OnAudioFilterRead(float[] data, int channels)
    {
        track.SetData(data, channels, sampleRate);
    }
}
```

> [!NOTE]
> As with the [`AudioListener`](https://docs.unity3d.com/ScriptReference/AudioListener.html) component, when using the [`OnAudioFilterRead`](https://docs.unity3d.com/ScriptReference/MonoBehaviour.OnAudioFilterRead.html) method, it must be associated with a `GameObject`.

## Receiving audio

You can use [`AudioStreamTrack`](../api/Unity.WebRTC.AudioStreamTrack.html) to receive the audio.
The class for receiving audio is got on [`OnTrack`](../api/Unity.WebRTC.RTCPeerConnection.html#Unity_WebRTC_RTCPeerConnection_OnTrack) event of the [`RTCPeerConnection`](../api/Unity.WebRTC.RTCPeerConnection.html) instance.
If the type of [`MediaStreamTrack`](../api/Unity.WebRTC.MediaStreamTrack.html) argument of the event is [`TrackKind.Audio`](../api/Unity.WebRTC.TrackKind.html), The [`Track`](../api/Unity.WebRTC.MediaStreamTrackEvent.html#Unity_WebRTC_MediaStreamTrackEvent_Track) instance is able to be casted to the [`AudioStreamTrack`](../api/Unity.WebRTC.AudioStreamTrack.html) class.

```CSharp
var receivedAudioSource = GetComponent<AudioSource>();
var receiveStream = new MediaStream();
receiveStream.OnAddTrack = e => {
    if(e.Track is AudioStreamTrack track)
    {
        // `AudioSource.SetTrack` is a extension method which is available 
        // when using `Unity.WebRTC` namespace.
        receivedAudioSource.SetTrack(track);

        // Please do not forget to turn on the `loop` flag.
        receivedAudioSource.loop = true;
        receivedAudioSource.Play();        
    }
    else if (e.Track is VideoStreamTrack track)
    {
        // This track is for video.
    }
}

var peerConnection = new RTCPeerConnection();
peerConnection.OnTrack = (RTCTrackEvent e) => {
    if (e.Track.Kind == TrackKind.Audio)
    {
        // Add track to MediaStream for receiver.
        // This process triggers `OnAddTrack` event of `MediaStream`.
        receiveStream.AddTrack(e.Track);
    }
};
```


