# 統計情報の取得

PeerConnectionを介して送信されたオーディオ、ビデオまたはデータパケットは途中で失われる可能性があり、またネットワークによる遅延が発生したりします。
そのためWebRTCを実装したアプリケーションでは、ネットワークとメディアパイプラインのパフォーマンスを監視することができるようになっています。

## PeerConnectionごとの統計情報の取得

統計情報は、PeerConnectionごとに取得することができます。
`RTCPeerConnection` の `GetStats` メソッドを呼び出してください。

```CSharp
// 統計情報の取得
var statsOperation = peerConnection.GetStats();
yield return statsOperation;
var statsReport = statisOperation.Value;
```

`GetStats`メソッドを実行したタイミングでの、PeerConnectionで取得できる統計情報が取得することができます。
取得した結果(`RTCStatsReport`)に含まれる統計情報は、対象のPeerConnectionでやり取りされているメディアまたはデータにより異なります。
`RTCStatsReport.Stats` により、`RTCStatsType`と`Id` のペアをキーに`RTCStats`をバリューとしたディクショナリにアクセスできます。

## RTCStatsTypeとRTCStats

取得できる統計情報の種類は`RTCStatsType`のEnumで定義されています。

```CSharp
public enum RTCStatsType
{
    [StringValue("codec")]
    Codec = 0,
    [StringValue("inbound-rtp")]
    InboundRtp = 1,
    [StringValue("outbound-rtp")]
    OutboundRtp = 2,
    [StringValue("remote-inbound-rtp")]
    RemoteInboundRtp = 3,
    [StringValue("remote-outbound-rtp")]
    RemoteOutboundRtp = 4,
    [StringValue("media-source")]
    MediaSource = 5,
    [StringValue("csrc")]
    Csrc = 6,
    [StringValue("peer-connection")]
    PeerConnection = 7,
    [StringValue("data-channel")]
    DataChannel = 8,
    [StringValue("stream")]
    Stream = 9,
    [StringValue("track")]
    Track = 10,
    [StringValue("transceiver")]
    Transceiver = 11,
    [StringValue("sender")]
    Sender = 12,
    [StringValue("receiver")]
    Receiver = 13,
    [StringValue("transport")]
    Transport = 14,
    [StringValue("sctp-transport")]
    SctpTransport = 15,
    [StringValue("candidate-pair")]
    CandidatePair = 16,
    [StringValue("local-candidate")]
    LocalCandidate = 17,
    [StringValue("remote-candidate")]
    RemoteCandidate = 18,
    [StringValue("certificate")]
    Certificate = 19,
    [StringValue("ice-server")]
    IceServer = 20,
}
```

各タイプごとに`RTCStats`を継承したクラスがあります。
例えば、`RTCStatusType.Codec`の場合は、`RTCCodecStats` になります。

この継承した各クラスは、タイプごとの保持している統計情報をメンバー変数として公開しています。
タイプごとにどのような統計情報が取得できるかは、継承された各クラスを確認してください。

使用する場合は、`RTCStatsReport.Stats` 経由で取得した `RTCStats` をタイプごとにアップキャストしてください。
