# Tutorial

This tutorial will cover the basics of using the WebRTC package.


### Adding a Namespace

The namespace specifies [`Unity.WebRTC`](../api/Unity.WebRTC.html).

```CSharp
using UnityEngine;
using Unity.WebRTC;
```

### Creating a local peer

Create a local peer and get [`RTCDataChannel`](../api/Unity.WebRTC.RTCDataChannel.html). Use [`RTCDataChannel`](../api/Unity.WebRTC.RTCDataChannel.html) to enable binary data transmission. Register [`OnOpen`](../api/Unity.WebRTC.RTCDataChannel.html#Unity_WebRTC_RTCDataChannel_OnOpen) and [`OnClose`](../api/Unity.WebRTC.RTCDataChannel.html#Unity_WebRTC_RTCDataChannel_OnClose) callbacks to run a process when [`RTCDataChannel`](../api/Unity.WebRTC.RTCDataChannel.html) starts or finishes. Set the [`OnMessage`](../api/Unity.WebRTC.RTCDataChannel.html#Unity_WebRTC_RTCDataChannel_OnMessage) callback to receive messages.

```CSharp
    // Create local peer
    var localConnection = new RTCPeerConnection();
    var sendChannel = localConnection.CreateDataChannel("sendChannel");
    sendChannel.OnOpen = handleSendChannelStatusChange;
    sendChannel.OnClose = handleSendChannelStatusChange;
```

### Creating a remote peer

Create a remote peer and set the [`OnDataChannel`](../api/Unity.WebRTC.RTCPeerConnection.html#Unity_WebRTC_RTCPeerConnection_OnDataChannel) callback.

```CSharp
    // Create remote peer
    var remoteConnection = new RTCPeerConnection();
    remoteConnection.OnDataChannel = ReceiveChannelCallback;
```

### Register potential communication paths

An ICE (Interactive Connectivity Establishment) exchange is required to establish a peer connection. Once the potential communication paths for all peers have been discovered, [`OnIceCandidate`](../api/Unity.WebRTC.RTCPeerConnection.html#Unity_WebRTC_RTCPeerConnection_OnIceCandidate) is called. Use callbacks to call [`AddIceCandidate`](../api/Unity.WebRTC.RTCPeerConnection.html#Unity_WebRTC_RTCPeerConnection_AddIceCandidate_) on each peer to register potential paths.


```CSharp
localConnection.OnIceCandidate = e => { !string.IsNullOrEmpty(e.candidate)
        || remoteConnection.AddIceCandidate(e); }

remoteConnection.OnIceCandidate = e => { !string.IsNullOrEmpty(e.candidate)
        || localConnection.AddIceCandidate(e); }

```

### The signaling process

SDP exchanges happen between peers. [`CreateOffer`](../api/Unity.WebRTC.RTCPeerConnection.html#Unity_WebRTC_RTCPeerConnection_CreateOffer) creates the initial Offer SDP. After getting the Offer SDP, both the local and remote peers set the SDP. Be careful not to mix up [`SetLocalDescription`](../api/Unity.WebRTC.RTCPeerConnection.html#Unity_WebRTC_RTCPeerConnection_SetLocalDescription) and [`SetRemoteDescription`](../api/Unity.WebRTC.RTCPeerConnection.html#Unity_WebRTC_RTCPeerConnection_SetRemoteDescription) during this exchange. 

Once the Offer SDP is set, call [`CreateAnswer`](../api/Unity.WebRTC.RTCPeerConnection.html#Unity_WebRTC_RTCPeerConnection_CreateAnswer) to create an Answer SDP. Like the Offer SDP, the Answer SDP is set on both the local and remote peers.

```csharp
var op1 = localConnection.CreateOffer();
yield return op1;
var op2 = localConnection.SetLocalDescription(ref op1.desc);
yield return op2;
var op3 = remoteConnection.SetRemoteDescription(ref op1.desc);
yield return op3;
var op4 = remoteConnection.CreateAnswer();
yield return op4;
var op5 = remoteConnection.setLocalDescription(op4.desc);
yield return op5;
var op6 = localConnection.setRemoteDescription(op4.desc);
yield return op6;
```

### Check the ICE connection status

When SDP exchanges happen between peers, ICE exchanges begin. Use the [`OnIceConnectionChange`](../api/Unity.WebRTC.RTCPeerConnection.html#Unity_WebRTC_RTCPeerConnection_OnIceConnectionChange) callback to check the ICE connection status.

```CSharp
localConnection.OnIceConnectionChange = state => {
    Debug.Log(state);
}
```

### The DataChannel connection

When the ICE exchange is finished, [`OnDataChannel`](../api/Unity.WebRTC.RTCPeerConnection.html#Unity_WebRTC_RTCPeerConnection_OnDataChannel) is called and a one-way peer Data Channel is created.
Register the `OnMessage` callback and describe the procedure for when a message is received.

```CSharp
RTCDataChannel receiveChannel;
void ReceiveChannelCallback(RTCDataChannel channel) 
{
    receiveChannel = channel;
    receiveChannel.OnMessage = HandleReceiveMessage;  
}
```

### Sending messages

When both peers' [`RTCDataChannel`](../api/Unity.WebRTC.RTCDataChannel.html) is open, it's possible to exchange messages. `string` or `byte[]` message types can be sent. 

```csharp
void SendMessage(string message)
{
  sendChannel.Send(message);
}

void SendBinary(byte[] bytes)
{
  sendChannel.Send(bytes);
}
```

### Receiving messages

When a message is received, the callback registered to [`OnMessage`](../api/Unity.WebRTC.RTCDataChannel.html#Unity_WebRTC_RTCDataChannel_OnMessage) is called. `byte[]` type messages can be received, and when treated like character strings they are converted as shown below.

```csharp
void HandleReceiveMessage(byte[] bytes)
{
  var message = System.Text.Encoding.UTF8.GetString(bytes);
  Debug.Log(message);
}
```

### The end process

When finished, `Close` method must be called for [`RTCDataChannel`](../api/Unity.WebRTC.RTCDataChannel.html) and [`RTCPeerConnection`](../api/Unity.WebRTC.RTCPeerConnection.html).

```csharp
private void OnDestroy()
{
  sendChannel.Close();
  receiveChannel.Close();
  
  localConnection.Close();
  remoteConnection.Close();
}
```

## Next step

This package provides sample scenes which demonstrate features like video/audio streaming. Please try them following [this page](sample.md).
