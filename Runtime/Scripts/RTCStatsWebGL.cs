#if UNITY_WEBGL
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Unity.WebRTC
{
    internal enum StatsMemberType
    {
        Bool,    // bool
        Int32,   // int32_t
        Uint32,  // uint32_t
        Int64,   // int64_t
        Uint64,  // uint64_t
        Double,  // double
        String,  // std::string

        SequenceBool,    // std::vector<bool>
        SequenceInt32,   // std::vector<int32_t>
        SequenceUint32,  // std::vector<uint32_t>
        SequenceInt64,   // std::vector<int64_t>
        SequenceUint64,  // std::vector<uint64_t>
        SequenceDouble,  // std::vector<double>
        SequenceString,  // std::vector<std::string>
    }

    public class RTCStatsReport : IDisposable
    {
        private IntPtr self;
        private readonly Dictionary<string, RTCStats> m_dictStats;

        private bool disposed;

        private Dictionary<string, Func<string, RTCStats>> statsDeserializeDictionary = new Dictionary<string, Func<string, RTCStats>>{
            { "codec", json => JsonConvert.DeserializeObject<RTCCodecStats>(json) },
            { "inbound-rtp", json => JsonConvert.DeserializeObject<RTCInboundRtpStreamStats>(json) },
            { "outbound-rtp", json => JsonConvert.DeserializeObject<RTCOutboundRtpStreamStats>(json) },
            { "remote-inbound-rtp",json => JsonConvert.DeserializeObject<RTCRemoteInboundRtpStreamStats>(json) },
            { "remote-outbound-rtp",json => JsonConvert.DeserializeObject<RTCRemoteOutboundRtpStreamStats>(json) },
            { "media-source",json => JsonConvert.DeserializeObject<RTCMediaSourceStats>(json) }, // kind
            { "csrc",json => JsonConvert.DeserializeObject<RTCRtpContributingSourceStats>(json) },
            { "peer-connection", json => JsonConvert.DeserializeObject<RTCPeerConnectionStats>(json) },
            { "data-channel", json => JsonConvert.DeserializeObject<RTCDataChannelStats>(json) },
            //Stream,             // obsolete
            //Track,              // obsolete
            { "transceiver",json => JsonConvert.DeserializeObject<RTCRtpTransceiverStats>(json) },
            { "sender", json => JsonConvert.DeserializeObject<RTCMediaHandlerStats>(json) }, // kind
            { "receiver", json => JsonConvert.DeserializeObject<RTCMediaHandlerStats>(json) }, // kind
            { "transport", json => JsonConvert.DeserializeObject<RTCTransportStats>(json) },
            { "sctp-transport", json => JsonConvert.DeserializeObject<RTCSctpTransportStats>(json) },
            { "candidate-pair", json => JsonConvert.DeserializeObject<RTCIceCandidatePairStats>(json) },
            { "local-candidate",json => JsonConvert.DeserializeObject<RTCIceCandidateStats>(json) },
            { "remote-candidate",json => JsonConvert.DeserializeObject<RTCIceCandidateStats>(json) },
            { "certificate",json => JsonConvert.DeserializeObject<RTCCertificateStats>(json) },
            { "ice-server", json => JsonConvert.DeserializeObject<RTCIceServerStats>(json) }
        };

        internal RTCStatsReport(IntPtr ptr)
        {
            var statsDataJson = ptr.AsAnsiStringWithFreeMem();
            var tmp = JsonConvert.DeserializeObject<string[][]>(statsDataJson);
            for (var i = 0; i < tmp[0].Length; i++)
            {
                var statsType = tmp[0][i];

                var stat = statsDeserializeDictionary[statsType](tmp[1][i]);
                if (statsType == "media-source")
                {
                    if (((RTCMediaSourceStats)stat).kind == RTCTrackKind.Audio)
                        stat = JsonConvert.DeserializeObject<RTCAudioSourceStats>(tmp[1][i]);
                    else
                        stat = JsonConvert.DeserializeObject<RTCVideoSourceStats>(tmp[1][i]);
                }
                else if (statsType == "sender")
                {
                    if (((RTCMediaHandlerStats)stat).kind == RTCTrackKind.Audio)
                        stat = JsonConvert.DeserializeObject<RTCAudioSenderStats>(tmp[1][i]);
                    else
                        stat = JsonConvert.DeserializeObject<RTCVideoSenderStats>(tmp[1][i]);
                }
                else if (statsType == "receiver")
                {
                    if (((RTCMediaHandlerStats)stat).kind == RTCTrackKind.Audio)
                        stat = JsonConvert.DeserializeObject<RTCAudioReceiverStats>(tmp[1][i]);
                    else
                        stat = JsonConvert.DeserializeObject<RTCVideoReceiverStats>(tmp[1][i]);
                }
                m_dictStats.Add(stat.id, stat);
            }
        }

        ~RTCStatsReport()
        {
            this.Dispose();
        }

        public void Dispose()
        {
            if (this.disposed)
            {
                return;
            }

            if (self != IntPtr.Zero && !WebRTC.Context.IsNull)
            {
                //WebRTC.Context.DeleteStatsReport(self);
                WebRTC.Table.Remove(self);
                self = IntPtr.Zero;
            }

            this.disposed = true;
            GC.SuppressFinalize(this);
        }

        public RTCStats Get(string id)
        {
            return m_dictStats[id];
        }

        public bool TryGetValue(string id, out RTCStats stats)
        {
            return m_dictStats.TryGetValue(id, out stats);
        }

        public IDictionary<string, RTCStats> Stats
        {
            get { return m_dictStats; }
        }
    }

    public enum RTCStatsType
    {
        Codec,
        InboundRtp,
        OutboundRtp,
        RemoteInboundRtp,
        RemoteOutboundRtp,
        MediaSource,
        Csrc,
        PeerConnection,
        DataChannel,
        //Stream,             // obsolete
        //Track,              // obsolete
        Transceiver,
        Sender,
        Receiver,
        Transport,
        SctpTransport,
        CandidatePair,
        LocalCandidate,
        RemoteCandidate,
        Certificate,
        IceServer
    };

    public enum RTCCodecType
    {
        Encode,
        Decode
    };

    public enum RTCTrackKind
    {
        Audio,
        Video
    };

    public enum RTCQualityLimitationReason
    {
        None,
        Cpu,
        Bandwidth,
        Other
    };

    public enum RTCStatsIceCandidatePairState
    {
        Frozen,
        Waiting,
        InProgress,
        Failed,
        Succeeded
    };

    public enum RTCIceTransportState
    {
        New,
        Checking,
        Connected,
        Completed,
        Disconnected,
        Failed,
        Closed
    };

    //public enum RTCDataChannelState
    //{
    //    Connecting,
    //    Open,
    //    Closing,
    //    Closed
    //};

    public enum RTCPriorityType
    {
        VeryLow,
        Low,
        Medium,
        High
    };

    public enum RTCIceRole
    {
        Unknown,
        Controlling,
        Controlled
    };

    public enum RTCDtlsTransportState
    {
        New,
        Connecting,
        Connected,
        Closed,
        Failed
    };

    //public enum RTCIceCandidateType
    //{
    //    Host,
    //    Srflx,
    //    Prflx,
    //    Relay
    //};

    [Serializable]
    public class RTCStats
    {
        public double timestamp;
        public RTCStatsType type;
        public string id;
    }

    /// <summary>
    /// inbound-rtp or remote-inbound-rtp
    /// outbound-rtp or remote-outbound-rtp
    /// </summary>
    [Serializable]
    public class RTCRtpStreamStats : RTCStats
    {
        public ulong ssrc;
        public string kind;
        public string transportId;
        public string codecId;

        public string mediaType;
        public double averageRTCPInterval;
    }

    // ==========================================
    /// <summary>
    /// codec 
    /// </summary>
    [Serializable]
    public class RTCCodecStats : RTCStats
    {
        public ulong payloadType;
        public RTCCodecType codecType;
        public string transportId;
        public string mimeType;
        public ulong clockRate;
        public ulong channels;
        public string sdpFmtpLine;

        public string implementation;
    }

    /// <summary>
    /// inbound-rtp or remote-inbound-rtp
    /// </summary>
    [Serializable]
    public class RTCReceivedRtpStreamStats : RTCRtpStreamStats
    {
        public ulong packetsReceived;
        public long packetsLost;
        public double jitter;
        public ulong packetsDiscarded;
        public ulong packetsRepaired;
        public ulong burstPacketsLost;
        public ulong burstPacketsDiscarded;
        public ulong burstLossCount;
        public ulong burstDiscardCount;
        public double burstLossRate;
        public double burstDiscardRate;
        public double gapLossRate;
        public double gapDiscardRate;
        public ulong framesDropped;
        public ulong partialFramesLost;
        public ulong fullFramesLost;
    }

    // ==========================================
    /// <summary>
    /// inbound-rtp
    /// </summary>
    [Serializable]
    public class RTCInboundRtpStreamStats : RTCReceivedRtpStreamStats
    {
        public string receiverId;
        public string remoteId;
        public ulong framesDecoded;
        public ulong keyFramesDecoded;
        public ulong frameWidth;
        public ulong frameHeight;
        public ulong frameBitDepth;
        public double framesPerSecond;
        public ulong qpSum;
        public double totalDecodeTime;
        public double totalInterFrameDelay;
        public double totalSquaredInterFrameDelay;
        public bool voiceActivityFlag;
        public double lastPacketReceivedTimestamp;
        public double averageRtcpInterval;
        public ulong headerBytesReceived;
        public ulong fecPacketsReceived;
        public ulong fecPacketsDiscarded;
        public ulong bytesReceived;
        public ulong packetsFailedDecryption;
        public ulong packetsDuplicated;
        public Dictionary<string, ulong> perDscpPacketsReceived;
        public ulong nackCount;
        public ulong firCount;
        public ulong pliCount;
        public ulong sliCount;
        public double totalProcessingDelay;
        public double estimatedPlayoutTimestamp;
        public double jitterBufferDelay;
        public ulong jitterBufferEmittedCount;
        public ulong totalSamplesReceived;
        public ulong totalSamplesDecoded;
        public ulong samplesDecodedWithSilk;
        public ulong samplesDecodedWithCelt;
        public ulong concealedSamples;
        public ulong silentConcealedSamples;
        public ulong concealmentEvents;
        public ulong insertedSamplesForDeceleration;
        public ulong removedSamplesForAcceleration;
        public double audioLevel;
        public double totalAudioEnergy;
        public double totalSamplesDuration;
        public ulong framesReceived;
        public string decoderImplementation;

        public string trackId;
        public double fractionLost;
    }

    // ==========================================
    /// <summary>
    /// remote-inbound-rtp
    /// </summary>
    [Serializable]
    public class RTCRemoteInboundRtpStreamStats : RTCReceivedRtpStreamStats
    {
        public string localId;
        public double roundTripTime;
        public double totalRoundTripTime;
        public double fractionLost;
        public ulong reportsReceived;
        public ulong roundTripTimeMeasurements;
    }

    /// <summary>
    /// outbound-rtp or remote-outbound-rtp
    /// </summary>
    [Serializable]
    public class RTCSentRtpStreamStats : RTCRtpStreamStats
    {
        public ulong packetsSent;
        public ulong bytesSent;
    }

    // ==========================================
    /// <summary>
    /// outbound-rtp
    /// </summary>
    [Serializable]
    public class RTCOutboundRtpStreamStats : RTCSentRtpStreamStats
    {
        public ulong rtxSsrc;
        public string mediaSourceId;
        public string senderId;
        public string remoteId;
        public string rid;
        public double lastPacketSentTimestamp;
        public ulong headerBytesSent;
        public ulong packetsDiscardedOnSend;
        public ulong bytesDiscardedOnSend;
        public ulong fecPacketsSent;
        public ulong retransmittedPacketsSent;
        public ulong retransmittedBytesSent;
        public double targetBitrate;
        public ulong totalEncodedBytesTarget;
        public ulong frameWidth;
        public ulong frameHeight;
        public ulong frameBitDepth;
        public double framesPerSecond;
        public ulong framesSent;
        public ulong hugeFramesSent;
        public ulong framesEncoded;
        public ulong keyFramesEncoded;
        public ulong framesDiscardedOnSend;
        public ulong qpSum;
        public ulong totalSamplesSent;
        public ulong samplesEncodedWithSilk;
        public ulong samplesEncodedWithCelt;
        public bool voiceActivityFlag;
        public double totalEncodeTime;
        public double totalPacketSendDelay;
        public double averageRtcpInterval;
        public RTCQualityLimitationReason qualityLimitationReason;
        public Dictionary<string, double> qualityLimitationDurations;
        public ulong qualityLimitationResolutionChanges;
        public Dictionary<string, ulong> perDscpPacketsSent;
        public ulong nackCount;
        public ulong firCount;
        public ulong pliCount;
        public ulong sliCount;
        public string encoderImplementation;

        public string trackId;
    }

    // ==========================================
    /// <summary>
    /// remote-outbound-rtp
    /// </summary>
    [Serializable]
    public class RTCRemoteOutboundRtpStreamStats : RTCSentRtpStreamStats
    {
        public string localId;
        public double remoteTimestamp;
        public ulong reportsSent;
    }

    /// <summary>
    /// media-source
    /// </summary>
    [Serializable]
    public class RTCMediaSourceStats : RTCStats
    {
        public string trackIdentifier;
        public RTCTrackKind kind;
        public bool relayedSource;
    }

    // ==========================================
    /// <summary>
    /// media-source (kind = audio)
    /// </summary>
    [Serializable]
    public class RTCAudioSourceStats : RTCMediaSourceStats
    {
        public double audioLevel;
        public double totalAudioEnergy;
        public double totalSamplesDuration;
        public double echoReturnLoss;
        public double echoReturnLossEnhancement;
    }

    // ==========================================
    /// <summary>
    /// media-source (kind = video)
    /// </summary>
    [Serializable]
    public class RTCVideoSourceStats : RTCMediaSourceStats
    {
        public ulong width;
        public ulong height;
        public ulong bitDepth;
        public ulong frames;
        public double framesPerSecond;
    }

    // ==========================================
    /// <summary>
    /// csrc
    /// </summary>
    [Serializable]
    public class RTCRtpContributingSourceStats : RTCStats
    {
        public ulong contributorSsrc;
        public string inboundRtpStreamId;
        public ulong packetsContributedTo;
        public double audioLevel;
    }

    // ==========================================
    /// <summary>
    /// peer-connection
    /// </summary>
    [Serializable]
    public class RTCPeerConnectionStats : RTCStats
    {
        public ulong dataChannelsOpened;
        public ulong dataChannelsClosed;
        public ulong dataChannelsRequested;
        public ulong dataChannelsAccepted;
    }

    // ==========================================
    /// <summary>
    /// transceiver
    /// </summary>
    [Serializable]
    public class RTCRtpTransceiverStats : RTCStats
    {
        public string senderId;
        public string receiverId;
        public string mid;
    }

    /// <summary>
    /// outbound-rtp or sender
    /// </summary>
    [Serializable]
    public class RTCMediaHandlerStats : RTCStats
    {
        public string trackIdentifier;
        public bool ended;
        public RTCTrackKind kind;

        public RTCPriorityType priority;
        public bool remoteSource;
    }

    /// <summary>
    /// outbound-rtp or sender (kind = video)
    /// </summary>
    [Serializable]
    public class RTCVideoHandlerStats : RTCMediaHandlerStats
    {
        public ulong frameWidth;
        public ulong frameHeight;
        public double framesPerSecond;
    }

    // ==========================================
    /// <summary>
    /// outbound-rtp or sender (kind = video)
    /// </summary>
    [Serializable]
    public class RTCVideoSenderStats : RTCVideoHandlerStats
    {
        public string mediaSourceId;

        public ulong keyFramesSent;
        public ulong framesCaptured;
        public ulong framesSent;
        public ulong hugeFramesSent;
    }

    // ==========================================
    /// <summary>
    /// receiver (kind = video)
    /// </summary>
    [Serializable]
    public class RTCVideoReceiverStats : RTCVideoHandlerStats
    {
        public ulong keyFramesReceived;
        public double estimatedPlayoutTimestamp;
        public double jitterBufferDelay;
        public ulong jitterBufferEmittedCount;
        public ulong framesReceived;
        public ulong framesDecoded;
        public ulong framesDropped;
        public ulong partialFramesLost;
        public ulong fullFramesLost;
    }

    /// <summary>
    /// outbound-rtp (kind = audio)
    /// </summary>
    [Serializable]
    public class RTCAudioHandlerStats : RTCMediaHandlerStats
    {
        public double audioLevel;
        public double totalAudioEnergy;
        public double totalSamplesDuration;
        public bool voiceActivityFlag;
    }

    // ==========================================
    /// <summary>
    /// outbound-rtp or sender (kind = audio)
    /// </summary>
    [Serializable]
    public class RTCAudioSenderStats : RTCAudioHandlerStats
    {
        public string mediaSourceId;

        public ulong totalSamplesSent;
        public double echoReturnLoss;
        public double echoReturnLossEnhancement;
    }

    // ==========================================
    /// <summary>
    /// receiver (kind = audio)
    /// </summary>
    [Serializable]
    public class RTCAudioReceiverStats : RTCAudioHandlerStats
    {
        public double estimatedPlayoutTimestamp;
        public double jitterBufferDelay;
        public ulong jitterBufferEmittedCount;
        public ulong totalSamplesReceived;
        public ulong concealedSamples;
        public ulong silentConcealedSamples;
        public ulong concealmentEvents;
        public ulong insertedSamplesForDeceleration;
        public ulong removedSamplesForAcceleration;
    }

    // ==========================================
    /// <summary>
    /// data-channel
    /// </summary>
    [Serializable]
    public class RTCDataChannelStats : RTCStats
    {
        public string label;
        public string protocol;
        public ushort dataChannelIdentifier;
        public RTCDataChannelState state;
        public ulong messagesSent;
        public ulong bytesSent;
        public ulong messagesReceived;
        public ulong bytesReceived;
    }

    // ==========================================
    /// <summary>
    /// transport
    /// </summary>
    [Serializable]
    public class RTCTransportStats : RTCStats
    {
        public ulong packetsSent;
        public ulong packetsReceived;
        public ulong bytesSent;
        public ulong bytesReceived;
        public string rtcpTransportStatsId;
        public RTCIceRole iceRole;
        public string iceLocalUsernameFragment;
        public RTCDtlsTransportState dtlsState;
        public RTCIceTransportState iceState;
        public string selectedCandidatePairId;
        public string localCertificateId;
        public string remoteCertificateId;
        public string tlsVersion;
        public string dtlsCipher;
        public string srtpCipher;
        public string tlsGroup;
        public ulong selectedCandidatePairChanges;
    }

    // ==========================================
    /// <summary>
    /// sctp-transport
    /// </summary>
    [Serializable]
    public class RTCSctpTransportStats : RTCStats
    {
        public string transportId;
        public double smoothedRoundTripTime;
        public ulong congestionWindow;
        public ulong receiverWindow;
        public ulong mtu;
        public ulong unackData;
    }

    // ==========================================
    /// <summary>
    /// local-candidate or remote-candidate
    /// </summary>
    [Serializable]
    public class RTCIceCandidateStats : RTCStats
    {
        public string transportId;
        public string address;
        public long port;
        public string protocol;
        public RTCIceCandidateType candidateType;
        public long priority;
        public string url;
        public string relayProtocol;

        public bool deleted = false;
        public bool isRemote;
    }

    // ==========================================
    /// <summary>
    /// candidate-pair
    /// </summary>
    [Serializable]
    public class RTCIceCandidatePairStats : RTCStats
    {
        public string transportId;
        public string localCandidateId;
        public string remoteCandidateId;
        public RTCStatsIceCandidatePairState state;
        public bool nominated;
        public ulong packetsSent;
        public ulong packetsReceived;
        public ulong bytesSent;
        public ulong bytesReceived;
        public double lastPacketSentTimestamp;
        public double lastPacketReceivedTimestamp;
        public double firstRequestTimestamp;
        public double lastRequestTimestamp;
        public double lastResponseTimestamp;
        public double totalRoundTripTime;
        public double currentRoundTripTime;
        public double availableOutgoingBitrate;
        public double availableIncomingBitrate;
        public ulong circuitBreakerTriggerCount;
        public ulong requestsReceived;
        public ulong requestsSent;
        public ulong responsesReceived;
        public ulong responsesSent;
        public ulong retransmissionsReceived;
        public ulong retransmissionsSent;
        public ulong consentRequestsSent;
        public double consentExpiredTimestamp;
        public ulong packetsDiscardedOnSend;
        public ulong bytesDiscardedOnSend;
        public ulong requestBytesSent;
        public ulong consentRequestBytesSent;
        public ulong responseBytesSent;

        public double totalRtt;
        public double currentRtt;
        public ulong priority;
    }

    // ==========================================
    /// <summary>
    /// certificate
    /// </summary>
    [Serializable]
    public class RTCCertificateStats : RTCStats
    {
        public string fingerprint;
        public string fingerprintAlgorithm;
        public string base64Certificate;
        public string issuerCertificateId;
    }

    // ==========================================
    /// <summary>
    /// ice-server
    /// </summary>
    [Serializable]
    public class RTCIceServerStats : RTCStats
    {
        public string url;
        public long port;
        public string relayProtocol;
        public ulong totalRequestsSent;
        public ulong totalResponsesReceived;
        public double totalRoundTripTime;
    }

    // obsolete
    // ==========================================
    ///// <summary>
    ///// stream
    ///// </summary>
    //[Serializable]
    //public class RTCMediaStreamStats : RTCStats
    //{
    //    public string streamIdentifier;
    //    public string[] trackIds;
    //}
}
#endif