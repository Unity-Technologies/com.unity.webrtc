# データチャネル

データチャネル（`DataChannel`）は、文字列やバイナリをピア間で送受信するための機能です。WebSocket と同等の機能を持ちつつ、プロトコルに UDP を利用しているためハイパフォーマンスであるという特徴があります。

## <a id="videotrack"/> データチャネルの作成

データチャネルは、1つのピアに対して複数作成できます。データチャネルの作成には、まず `RTCPeerConnection` の `CreateDataChannel` メソッドを呼び出す方法があります。

```CSharp
// データチャネルの作成
var option = new RTCDataChannelInit(true);
var channel = peerConnection.CreateDataChannel("test", ref option);
```

また、他方のピアがデータチャネルを作成した場合に、コールバックとして `RTCPeerConnection.OnDataChannel` デリゲートが実行されます。

```CSharp
// OnDataChannel デリゲートの登録
peerConnnection.OnDataChannel = channel => 
{
    // ...
}
```

データチャネルがピア間で通信可能になったとき、 `RTCDataChannel.OnOpen` デリゲートが実行されます。また、切断したときは `RTCDataChannel.OnClose` が実行されます。

## <a id="send-message"/> メッセージの送信

メッセージの送信には文字列もしくはバイナリを利用できます。 `RTCDataChannel.Send` メソッドを実行してください。

```CSharp
// 文字列を送信する
string text = "hello";
channel.Send(text);

// バイト列を送信する
byte[] data = System.Text.Encoding.ASCII.GetBytes(text);
channel.Send(data);

```

## <a id="recv-message"/> メッセージの受信

メッセージ受信は `RTCDataChannel.OnMessage` デリゲートを利用します。

```CSharp
// OnMessage デリゲートの登録
channel.OnMessage = bytes => 
{
    // ...
}
```
