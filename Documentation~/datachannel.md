# DataChannel

The **DataChannel** feature passes text strings and binary between peers. It has the same features as WebSocket and uses UDP protocol, giving it several high performance characteristics. 

> [!NOTE]
> The [package samples](sample.md) contains the **DataChannel** scene which demonstrates DataChannel features of the package.

## Creating DataChannel

Multiple data channels can be created for a single peer. To create a data channel, first call the [`RTCPeerConnection`](../api/Unity.WebRTC.RTCPeerConnection.html)'s  [`CreateDataChannel`](../api/Unity.WebRTC.RTCPeerConnection.html#Unity_WebRTC_RTCPeerConnection_CreateDataChannel_) method.

```CSharp
// Create the `RTCDataChannel` instance.
var option = new RTCDataChannelInit();
var channel = peerConnection.CreateDataChannel("test", option);
```

If another peer creates a data channel, an [`OnDataChannel`](../api/Unity.WebRTC.RTCPeerConnection.html#Unity_WebRTC_RTCPeerConnection_OnDataChannel) delegate will be executed as a call back.

```CSharp
// Register the OnDataChannel delegate.
peerConnnection.OnDataChannel = channel => 
{
    // ...
}
```

Once the data channel is able to communicate between peers, the [`RTCDataChannel.OnOpen`](../api/Unity.WebRTC.RTCDataChannel.html#Unity_WebRTC_RTCDataChannel_OnOpen) delegate will be executed. When the connection is closed, [`RTCDataChannel.OnClose`](../api/Unity.WebRTC.RTCDataChannel.html#Unity_WebRTC_RTCDataChannel_OnClose) will execute. 

## Send messages

Text strings or binary can be used for messages.  Execute the [`RTCDataChannel.Send`](../api/Unity.WebRTC.RTCDataChannel.html#Unity_WebRTC_RTCDataChannel_Send_) method to do so.

```CSharp
// Send a text string
string text = "hello";
channel.Send(text);

// Send byte array.
byte[] data = System.Text.Encoding.ASCII.GetBytes(text);
channel.Send(data);

// Send a NativeArray.
NativeArray<float> array = new NativeArray<float>(10, Allocator.Temp);
channel.Send(array);
```

## Receive messages

The [`RTCDataChannel.OnMessage`](../api/Unity.WebRTC.RTCDataChannel.html#Unity_WebRTC_RTCDataChannel_OnMessage) delegate is used to receive messages.

```CSharp
// Register OnMessage delegate.
channel.OnMessage = bytes => 
{
    // ...
}
```
