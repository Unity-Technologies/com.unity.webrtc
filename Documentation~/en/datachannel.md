# Data Channel

- [Creating Data Channel](#creating-datachannel)
- [Send Message](#send-message)
- [Receive Message](#recv-message)

The `DataChannel` feature passes text strings and binary between peers. It has the same features as WebSocket and uses UDP protocol, giving it several high performance characteristics. 

## <a id="videotrack"/> Creating Data Channel

Multiple data channels can be created for a single peer. To create a data channel, first call the `RTCPeerConnection`'s  `CreateDataChannel` method.

```CSharp
// Create the data channel
var option = new RTCDataChannelInit(true);
var channel = peerConnection.CreateDataChannel("test", ref option);
```

If another peer creates a data channel, an `RTCPeerConnection.OnDataChannel` delegate will be executed as a call back.

```CSharp
// Register the OnDataChannel delegate
peerConnnection.OnDataChannel = channel => 
{
    // ...
}
```

Once the data channel is able to communicate between peers, the `RTCDataChannel.OnOpen` delegate will be executed. When the connection is closed, `RTCDataChannel.OnClose` will execute. 

## <a id="send-message"/> Send Message

Text strings or binary can be used for messages.  Execute the `RTCDataChannel.Send` method to do so.

```CSharp
// Send a text string
string text = "hello";
channel.Send(text);

// Send binary
byte[] data = System.Text.Encoding.ASCII.GetBytes(text);
channel.Send(data);

```

## <a id="recv-message"/> Receive Message

The `RTCDataChannel.OnMessage` delegate is used to receive messages.

```CSharp
// Register OnMessage delegate
channel.OnMessage = bytes => 
{
    // ...
}
```
