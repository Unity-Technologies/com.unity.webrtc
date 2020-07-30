# Get Statistics

Audio, video and data packets sent over PeerConnection can be lost or delayed along the way, depending on network conditions. By using the statistics API, you can monitor the performance of your network and media pipeline and get clues as to the cause of the problem.

## Get PeerConnection statistics.

Statistics can be obtained for each PeerConnection.
Call the `GetStats` method of the `RTCPeerConnection`.

```CSharp
// Get Statistics
var statsOperation = peerConnection.GetStats();
yield return statsOperation;
var statsReport = statisOperation.Value;
```

You can get the statistics at the time of calling the `GetStats` method.
The statistics contained in the result are stored in the `RTCStatsReport`. The content of the statistics depends on the media or data that is being communicated on the target PeerConnection.
The `RTCStatsReport.Stats` gives access to a dictionary with `RTCStats` as a value, using the `RTCStatsType` and `Id` pairs as keys.

## RTCStatsType and RTCStats

The types of statistics available are defined by the `RTCStatsType`.

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

This class extends the `RTCStats` for each type of statistics. For example, in the case of `RTCStatusType.Codec`, it defines the class `RTCCodecStats`.
Each class exposes the statistics of each type as a member variable. Check each inherited class to see what statistics are available. To access the member variables, cast the `RTCStats` obtained via `RTCStatsReport.Stats` for each type.

## Statistics Tool.
A tool is provided to display the current statistics for testing on the editor. See [here](webrtcstats.md) for more information.

Translated with www.DeepL.com/Translator (free version)