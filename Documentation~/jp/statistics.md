# 統計情報の取得

PeerConnection を介して送信されたオーディオやビデオ、データパケットは、ネットワーク状況によっては途中で失われたり、遅延が生じる可能性があります。統計情報 API を利用することで、ネットワークとメディアパイプラインのパフォーマンスを監視し、問題の原因を特定する手がかりを得ることができます。

## PeerConnection の統計情報の取得

統計情報は、PeerConnection 毎に取得することができます。
`RTCPeerConnection` の `GetStats` メソッドを呼び出してください。

```CSharp
// 統計情報の取得
var statsOperation = peerConnection.GetStats();
yield return statsOperation;
var statsReport = statisOperation.Value;
```

`GetStats` メソッドを呼び出した時点の統計情報を取得することができます。
取得した結果に含まれる統計情報は `RTCStatsReport` に格納されます。統計情報の内容は、対象の PeerConnection でやり取りされているメディアまたはデータにより異なります。
`RTCStatsReport.Stats` により、 `RTCStatsType` と `Id` のペアをキーに、 `RTCStats` を値とした辞書にアクセスできます。

## RTCStatsTypeとRTCStats

取得できる統計情報の種類は `RTCStatsType` で定義されています。

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

統計情報の種類毎に `RTCStats` を継承したクラスを定義しています。例えば `RTCStatusType.Codec` の場合は `RTCCodecStats` を定義しています。
各クラスは、種類毎に保持している統計情報をメンバー変数として公開します。どのような統計情報が取得できるかについては、継承された各クラスを確認してください。メンバー変数にアクセス場合は、`RTCStatsReport.Stats` 経由で取得した `RTCStats` を種類毎にキャストします。

## 統計情報ツール
エディタ上での動作確認のために、現在の統計情報を表示するツールを提供しています。詳しくは、 [こちら](webrtcstats.md) をご覧ください。