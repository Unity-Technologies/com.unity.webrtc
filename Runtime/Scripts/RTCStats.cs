using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace Unity.WebRTC
{
    public class StringValueAttribute : Attribute
    {
        public string StringValue { get; protected set; }

        public StringValueAttribute(string value)
        {
            this.StringValue = value;
        }
    }

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

    internal class RTCStatsMember
    {
        internal IntPtr self;

        internal RTCStatsMember(IntPtr ptr)
        {
            self = ptr;
        }

        internal string GetName()
        {
            return NativeMethods.StatsMemberGetName(self).AsAnsiStringWithFreeMem();
        }

        internal StatsMemberType GetValueType()
        {
            return NativeMethods.StatsMemberGetType(self);
        }

        internal object GetValue()
        {
            StatsMemberType type = this.GetValueType();

            if (!NativeMethods.StatsMemberIsDefined(self))
            {
                return null;
            }

            uint length = 0;
            switch (type)
            {
                case StatsMemberType.Bool:
                    return NativeMethods.StatsMemberGetBool(self);
                case StatsMemberType.Int32:
                    return NativeMethods.StatsMemberGetInt(self);
                case StatsMemberType.Uint32:
                    return NativeMethods.StatsMemberGetUnsignedInt(self);
                case StatsMemberType.Int64:
                    return NativeMethods.StatsMemberGetLong(self);
                case StatsMemberType.Uint64:
                    return NativeMethods.StatsMemberGetUnsignedLong(self);
                case StatsMemberType.Double:
                    return NativeMethods.StatsMemberGetDouble(self);
                case StatsMemberType.String:
                    return NativeMethods.StatsMemberGetString(self).AsAnsiStringWithFreeMem();
                case StatsMemberType.SequenceBool:
                    return NativeMethods.StatsMemberGetBoolArray(self, ref length).AsArray<bool>((int)length);
                case StatsMemberType.SequenceInt32:
                    return NativeMethods.StatsMemberGetIntArray(self, ref length).AsArray<int>((int)length);
                case StatsMemberType.SequenceUint32:
                    return NativeMethods.StatsMemberGetUnsignedIntArray(self, ref length).AsArray<uint>((int)length);
                case StatsMemberType.SequenceInt64:
                    return NativeMethods.StatsMemberGetLongArray(self, ref length).AsArray<long>((int)length);
                case StatsMemberType.SequenceUint64:
                    return NativeMethods.StatsMemberGetUnsignedLongArray(self, ref length).AsArray<ulong>((int)length);
                case StatsMemberType.SequenceDouble:
                    return NativeMethods.StatsMemberGetDoubleArray(self, ref length).AsArray<double>((int)length);
                case StatsMemberType.SequenceString:
                    return NativeMethods.StatsMemberGetStringArray(self, ref length).AsArray<string>((int)length);
                default:
                    throw new ArgumentException();
            }
        }
    }

    public class RTCStats
    {
        private IntPtr self;
        internal Dictionary<string, RTCStatsMember> m_members;
        internal Dictionary<string, object> m_dict;

        public RTCStatsType Type
        {
            get
            {
                return NativeMethods.StatsGetType(self);
            }
        }

        public string Id
        {
            get { return NativeMethods.StatsGetId(self).AsAnsiStringWithFreeMem(); }
        }

        /// <summary>
        /// this timestamp is utc epoch time micro seconds.
        /// </summary>
        public long Timestamp
        {
            get { return NativeMethods.StatsGetTimestamp(self); }
        }

        public DateTime UtcTimeStamp
        {
            get { return DateTimeOffset.FromUnixTimeMilliseconds(Timestamp / 1000).UtcDateTime; }
        }

        public IDictionary<string, object> Dict
        {
            get
            {
                if(m_dict == null)
                {
                    m_dict = m_members.ToDictionary(member => member.Key, member => member.Value.GetValue());
                }
                return m_dict;
            }
        }

        internal bool GetBool(string key)
        {
            if (!NativeMethods.StatsMemberIsDefined(m_members[key].self))
            {
                return default;
            }
            return NativeMethods.StatsMemberGetBool(m_members[key].self);
        }
        internal int GetInt(string key)
        {
            if (!NativeMethods.StatsMemberIsDefined(m_members[key].self))
            {
                return default;
            }
            return NativeMethods.StatsMemberGetInt(m_members[key].self);
        }
        internal uint GetUnsignedInt(string key)
        {
            if (!NativeMethods.StatsMemberIsDefined(m_members[key].self))
            {
                return default;
            }
            return NativeMethods.StatsMemberGetUnsignedInt(m_members[key].self);
        }
        internal long GetLong(string key)
        {
            if (!NativeMethods.StatsMemberIsDefined(m_members[key].self))
            {
                return default;
            }
            return NativeMethods.StatsMemberGetLong(m_members[key].self);
        }
        internal ulong GetUnsignedLong(string key)
        {
            if (!NativeMethods.StatsMemberIsDefined(m_members[key].self))
            {
                return default;
            }
            return NativeMethods.StatsMemberGetUnsignedLong(m_members[key].self);
        }
        internal double GetDouble(string key)
        {
            if (!NativeMethods.StatsMemberIsDefined(m_members[key].self))
            {
                return default;
            }
            return NativeMethods.StatsMemberGetDouble(m_members[key].self);
        }
        internal string GetString(string key)
        {
            if (!NativeMethods.StatsMemberIsDefined(m_members[key].self))
            {
                return default;
            }
            return NativeMethods.StatsMemberGetString(m_members[key].self).AsAnsiStringWithFreeMem();
        }
        internal bool[] GetBoolArray(string key)
        {
            if (!NativeMethods.StatsMemberIsDefined(m_members[key].self))
            {
                return default;
            }
            uint length = 0;
            return NativeMethods.StatsMemberGetBoolArray(m_members[key].self, ref length).AsArray<bool>((int)length);
        }
        internal int[] GetIntArray(string key)
        {
            if (!NativeMethods.StatsMemberIsDefined(m_members[key].self))
            {
                return default;
            }
            uint length = 0;
            return NativeMethods.StatsMemberGetIntArray(m_members[key].self, ref length).AsArray<int>((int)length);
        }

        internal uint[] GetUnsignedIntArray(string key)
        {
            if (!NativeMethods.StatsMemberIsDefined(m_members[key].self))
            {
                return default;
            }
            uint length = 0;
            return NativeMethods.StatsMemberGetUnsignedIntArray(m_members[key].self, ref length).AsArray<uint>((int)length);
        }
        internal long[] GetLongArray(string key)
        {
            if (!NativeMethods.StatsMemberIsDefined(m_members[key].self))
            {
                return default;
            }
            uint length = 0;
            return NativeMethods.StatsMemberGetLongArray(m_members[key].self, ref length).AsArray<long>((int)length);
        }
        internal ulong[] GetUnsignedLongArray(string key)
        {
            if (!NativeMethods.StatsMemberIsDefined(m_members[key].self))
            {
                return default;
            }
            uint length = 0;
            return NativeMethods.StatsMemberGetUnsignedLongArray(m_members[key].self, ref length).AsArray<ulong>((int)length);
        }
        internal double[] GetDoubleArray(string key)
        {
            if (!NativeMethods.StatsMemberIsDefined(m_members[key].self))
            {
                return default;
            }
            uint length = 0;
            return NativeMethods.StatsMemberGetDoubleArray(m_members[key].self, ref length).AsArray<double>((int)length);
        }
        internal string[] GetStringArray(string key)
        {
            if (!NativeMethods.StatsMemberIsDefined(m_members[key].self))
            {
                return default;
            }
            uint length = 0;
            return NativeMethods.StatsMemberGetStringArray(m_members[key].self, ref length).AsArray<string>((int)length);
        }

        internal RTCStats(IntPtr ptr)
        {
            self = ptr;
            RTCStatsMember[] array = GetMembers();
            m_members = new Dictionary<string, RTCStatsMember>();
            foreach (var member in array)
            {
                m_members.Add(member.GetName(), member);
            }
        }

        RTCStatsMember[] GetMembers()
        {
            uint length = 0;
            IntPtr ptr = NativeMethods.StatsGetMembers(self, ref length);
            IntPtr[] array = ptr.AsArray<IntPtr>((int)length);

            RTCStatsMember[] members = new RTCStatsMember[length];
            for (int i = 0; i < length; i++)
            {
                members[i] = new RTCStatsMember(array[i]);
            }

            return members;
        }

        public string ToJson()
        {
            return NativeMethods.StatsGetJson(self).AsAnsiStringWithFreeMem();
        }
    }

    public class RTCRtpStreamStats : RTCStats
    {
        internal RTCRtpStreamStats(IntPtr ptr) : base(ptr)
        {
        }

        public string CodecId
        {
            get { return ""; }
        }
    }

    public class RTCCertificateStats : RTCStats
    {
        public string fingerprint { get { return GetString("fingerprint"); } }
        public string fingerprintAlgorithm { get { return GetString("fingerprintAlgorithm"); } }
        public string base64Certificate { get { return GetString("base64Certificate"); } }
        public string issuerCertificateId { get { return GetString("issuerCertificateId"); } }
        internal RTCCertificateStats(IntPtr ptr) : base(ptr)
        {
        }
    }

    public class RTCCodecStats : RTCStats
    {
        public uint payloadType { get { return GetUnsignedInt("payloadType"); } }
        public string mimeType { get { return GetString("mimeType"); } }
        public uint clockRate { get { return GetUnsignedInt("clockRate"); } }
        public uint channels { get { return GetUnsignedInt("channels"); } }
        public string sdpFmtpLine { get { return GetString("sdpFmtpLine"); } }
        internal RTCCodecStats(IntPtr ptr) : base(ptr)
        {
        }
    }

    public class RTCIceCandidatePairStats : RTCStats
    {
        public string transportId { get { return GetString("transportId"); } }
        public string localCandidateId { get { return GetString("localCandidateId"); } }
        public string remoteCandidateId { get { return GetString("remoteCandidateId"); } }
        public string state { get { return GetString("state"); } }
        public ulong priority { get { return GetUnsignedLong("priority"); } }
        public bool nominated { get { return GetBool("nominated"); } }
        public bool writable { get { return GetBool("writable"); } }
        public bool readable { get { return GetBool("readable"); } }
        public ulong bytesSent { get { return GetUnsignedLong("bytesSent"); } }
        public ulong bytesReceived { get { return GetUnsignedLong("bytesReceived"); } }
        public double totalRoundTripTime { get { return GetDouble("totalRoundTripTime"); } }
        public double currentRoundTripTime { get { return GetDouble("currentRoundTripTime"); } }
        public double availableOutgoingBitrate { get { return GetDouble("availableOutgoingBitrate"); } }
        public double availableIncomingBitrate { get { return GetDouble("availableIncomingBitrate"); } }
        public ulong requestsReceived { get { return GetUnsignedLong("requestsReceived"); } }
        public ulong requestsSent { get { return GetUnsignedLong("requestsSent"); } }
        public ulong responsesReceived { get { return GetUnsignedLong("responsesReceived"); } }
        public ulong responsesSent { get { return GetUnsignedLong("responsesSent"); } }
        public ulong retransmissionsReceived { get { return GetUnsignedLong("retransmissionsReceived"); } }
        public ulong retransmissionsSent { get { return GetUnsignedLong("retransmissionsSent"); } }
        public ulong consentRequestsReceived { get { return GetUnsignedLong("consentRequestsReceived"); } }
        public ulong consentRequestsSent { get { return GetUnsignedLong("consentRequestsSent"); } }
        public ulong consentResponsesReceived { get { return GetUnsignedLong("consentResponsesReceived"); } }
        public ulong consentResponsesSent { get { return GetUnsignedLong("consentResponsesSent"); } }

        internal RTCIceCandidatePairStats(IntPtr ptr) : base(ptr)
        {
        }
    }

    public class RTCIceCandidateStats : RTCStats
    {
        public string transportId { get { return GetString("transportId"); } }
        public bool isRemote { get { return GetBool("isRemote"); } }
        public string networkType { get { return GetString("networkType"); } }
        public string ip { get { return GetString("ip"); } }
        public int port { get { return GetInt("port"); } }
        public string protocol { get { return GetString("protocol"); } }
        public string relayProtocol { get { return GetString("relayProtocol"); } }
        public string candidateType { get { return GetString("candidateType"); } }
        public int priority { get { return GetInt("priority"); } }
        public string url { get { return GetString("url"); } }
        public bool deleted { get { return GetBool("deleted"); } }

        internal RTCIceCandidateStats(IntPtr ptr) : base(ptr)
        {
        }

    }


    public class RTCInboundRTPStreamStats : RTCRTPStreamStats
    {
        public uint packetsReceived { get { return GetUnsignedInt("packetsReceived"); } }
        public ulong fecPacketsReceived { get { return GetUnsignedLong("fecPacketsReceived"); }}
        public ulong fecPacketsDiscarded { get { return GetUnsignedLong("fecPacketsDiscarded");  }}
        public ulong bytesReceived { get { return GetUnsignedLong("bytesReceived"); } }
        public ulong headerBytesReceived { get { return GetUnsignedLong("headerBytesReceived"); } }
        public int packetsLost { get { return GetInt("packetsLost"); } }
        public double lastPacketReceivedTimestamp { get { return GetDouble("lastPacketReceivedTimestamp"); } }
        public double jitter { get { return GetDouble("jitter"); } }
        public double roundTripTime { get { return GetDouble("roundTripTime"); } }
        public uint packetsDiscarded { get { return GetUnsignedInt("packetsDiscarded"); } }
        public uint packetsRepaired { get { return GetUnsignedInt("packetsRepaired"); } }
        public uint burstPacketsLost { get { return GetUnsignedInt("burstPacketsLost"); } }
        public uint burstPacketsDiscarded { get { return GetUnsignedInt("burstPacketsDiscarded"); } }
        public uint burstLossCount { get { return GetUnsignedInt("burstLossCount"); } }
        public uint burstDiscardCount { get { return GetUnsignedInt("burstDiscardCount"); } }
        public double burstLossRate { get { return GetDouble("burstLossRate"); } }
        public double burstDiscardRate { get { return GetDouble("burstDiscardRate"); } }
        public double gapLossRate { get { return GetDouble("gapLossRate"); } }
        public double gapDiscardRate { get { return GetDouble("gapDiscardRate"); } }
        public uint framesDecoded { get { return GetUnsignedInt("framesDecoded"); } }
        public uint keyFramesDecoded { get { return GetUnsignedInt("keyFramesDecoded"); } }
        public double totalDecodeTime { get { return GetDouble("totalDecodeTime"); } }
        public double totalInterFrameDelay{ get { return GetDouble("totalInterFrameDelay"); } }
        public double totalSquaredInterFrameDelay{ get { return GetDouble("totalSquaredInterFrameDelay"); } }

        public string contentType { get { return GetString("contentType"); } }
        public string decoderImplementation { get { return GetString("decoderImplementation"); } }

        internal RTCInboundRTPStreamStats(IntPtr ptr) : base(ptr)
        {
        }
    }
    public class RTCMediaStreamTrackStats : RTCStats
    {
        public string trackIdentifier { get { return GetString("trackIdentifier"); } }
        public string mediaSourceId { get { return GetString("mediaSourceId"); } }
        public bool remoteSource { get { return GetBool("remoteSource"); } }
        public bool ended { get { return GetBool("ended"); } }
        public bool detached { get { return GetBool("detached"); } }
        public string kind { get { return GetString("kind"); } }
        public double jitterBufferDelay { get { return GetDouble("jitterBufferDelay"); } }
        public ulong jitterBufferEmittedCount { get { return GetUnsignedLong("jitterBufferEmittedCount"); } }
        public uint frameWidth { get { return GetUnsignedInt("frameWidth"); } }
        public uint frameHeight { get { return GetUnsignedInt("frameHeight"); } }
        public double framesPerSecond { get { return GetDouble("framesPerSecond"); } }
        public uint framesSent { get { return GetUnsignedInt("framesSent"); } }
        public uint hugeFramesSent { get { return GetUnsignedInt("hugeFramesSent"); } }
        public uint framesReceived { get { return GetUnsignedInt("framesReceived"); } }
        public uint framesDecoded { get { return GetUnsignedInt("framesDecoded"); } }
        public uint framesDropped { get { return GetUnsignedInt("framesDropped"); } }
        public uint framesCorrupted { get { return GetUnsignedInt("framesCorrupted"); } }
        public uint partialFramesLost { get { return GetUnsignedInt("partialFramesLost"); } }
        public uint fullFramesLost { get { return GetUnsignedInt("fullFramesLost"); } }
        public double audioLevel { get { return GetDouble("audioLevel"); } }
        public double totalAudioEnergy { get { return GetDouble("totalAudioEnergy"); } }
        public double echoReturnLoss { get { return GetDouble("echoReturnLoss"); } }
        public double echoReturnLossEnhancement { get { return GetDouble("echoReturnLossEnhancement"); } }
        public ulong totalSamplesReceived { get { return GetUnsignedLong("totalSamplesReceived"); } }
        public double totalSamplesDuration { get { return GetDouble("totalSamplesDuration"); } }
        public ulong concealedSamples { get { return GetUnsignedLong("concealedSamples"); } }
        public ulong silentConcealedSamples { get { return GetUnsignedLong("silentConcealedSamples"); } }
        public ulong concealmentEvents { get { return GetUnsignedLong("concealmentEvents"); } }
        public ulong insertedSamplesForDeceleration { get { return GetUnsignedLong("insertedSamplesForDeceleration"); } }
        public ulong removedSamplesForAcceleration { get { return GetUnsignedLong("removedSamplesForAcceleration"); } }
        public ulong jitterBufferFlushes { get { return GetUnsignedLong("jitterBufferFlushes"); } }
        public ulong delayedPacketOutageSamples { get { return GetUnsignedLong("delayedPacketOutageSamples"); } }
        public double relativePacketArrivalDelay { get { return GetDouble("relativePacketArrivalDelay"); } }
        public double jitterBufferTargetDelay { get { return GetDouble("jitterBufferTargetDelay"); } }
        public uint interruptionCount { get { return GetUnsignedInt("interruptionCount"); } }
        public double totalInterruptionDuration { get { return GetDouble("totalInterruptionDuration"); } }
        public uint freezeCount { get { return GetUnsignedInt("freezeCount"); } }
        public uint pauseCount { get { return GetUnsignedInt("pauseCount"); } }
        public double totalFreezesDuration { get { return GetDouble("totalFreezesDuration"); } }
        public double totalPausesDuration { get { return GetDouble("totalPausesDuration"); } }
        public double totalFramesDuration { get { return GetDouble("totalFramesDuration"); } }
        public double sumOfSquaredFramesDuration { get { return GetDouble("sumOfSquaredFramesDuration"); } }

        internal RTCMediaStreamTrackStats(IntPtr ptr) : base(ptr)
        {
        }
    }

    public class RTCMediaStreamStats : RTCStats
    {
        public string streamIdentifier { get { return GetString("streamIdentifier"); } }
        public string[] trackIds { get { return GetStringArray("trackIds"); } }

        internal RTCMediaStreamStats(IntPtr ptr) : base(ptr)
        {
        }

    }

    public class RTCRTPStreamStats : RTCStats
    {
        public uint ssrc { get { return GetUnsignedInt("ssrc"); } }
        public double estimatedPlayoutTimestamp { get { return GetDouble("estimatedPlayoutTimestamp"); } }
        public bool isRemote { get { return GetBool("isRemote"); } }
        public string mediaType { get { return GetString("mediaType"); } }
        public string kind { get { return GetString("kind"); } }
        public string trackId { get { return GetString("trackId"); } }
        public string transportId { get { return GetString("transportId"); } }
        public string codecId { get { return GetString("codecId"); } }
        public uint firCount { get { return GetUnsignedInt("firCount"); } }
        public uint pliCount { get { return GetUnsignedInt("pliCount"); } }
        public uint nackCount { get { return GetUnsignedInt("nackCount"); } }
        public uint sliCount { get { return GetUnsignedInt("sliCount"); } }
        public ulong qpSum { get { return GetUnsignedLong("qpSum"); } }

        internal RTCRTPStreamStats(IntPtr ptr) : base(ptr)
        {
        }

    }

    public class RTCOutboundRTPStreamStats : RTCRTPStreamStats
    {
        public string mediaSourceId { get { return GetString("mediaSourceId"); } }
        public string remoteId { get { return GetString("remoteId"); } }
        public string rid { get { return GetString("rid"); } }
        public uint packetsSent { get { return GetUnsignedInt("packetsSent"); } }
        public ulong retransmittedPacketsSent { get { return GetUnsignedLong("retransmittedPacketsSent"); } }
        public ulong bytesSent { get { return GetUnsignedLong("bytesSent"); } }
        public ulong headerBytesSent { get { return GetUnsignedLong("headerBytesSent"); } }
        public ulong retransmittedBytesSent { get { return GetUnsignedLong("retransmittedBytesSent"); } }
        public double targetBitrate { get { return GetDouble("targetBitrate"); } }
        public uint framesEncoded { get { return GetUnsignedInt("framesEncoded"); } }
        public uint keyFramesEncoded { get { return GetUnsignedInt("keyFramesEncoded"); } }
        public double totalEncodeTime { get { return GetDouble("totalEncodeTime"); } }
        public ulong totalEncodedBytesTarget { get { return GetUnsignedLong("totalEncodedBytesTarget"); } }
        public uint frameWidth { get { return GetUnsignedInt("frameWidth"); } }
        public uint frameHeight { get { return GetUnsignedInt("frameHeight"); } }
        public double framesPerSecond { get { return GetDouble("framesPerSecond"); } }
        public uint framesSent { get { return GetUnsignedInt("framesSent"); } }
        public uint hugeFramesSent { get { return GetUnsignedInt("hugeFramesSent"); } }
        public double totalPacketSendDelay { get { return GetDouble("totalPacketSendDelay"); } }
        public string qualityLimitationReason { get { return GetString("qualityLimitationReason"); } }
        public uint qualityLimitationResolutionChanges { get { return GetUnsignedInt("qualityLimitationResolutionChanges"); } }
        public string contentType { get { return GetString("contentType"); } }
        public string encoderImplementation { get { return GetString("encoderImplementation"); } }

        internal RTCOutboundRTPStreamStats(IntPtr ptr) : base(ptr)
        {
        }
    }
    public class RTCRemoteInboundRtpStreamStats : RTCStats
    {
        public uint ssrc { get { return GetUnsignedInt("ssrc"); } }
        public string kind { get { return GetString("kind"); } }
        public string transportId { get { return GetString("transportId"); } }
        public string codecId { get { return GetString("codecId"); } }

        public int packetsLost { get { return GetInt("packetsLost"); } }
        public double jitter { get { return GetDouble("jitter"); } }

        public string localId { get { return GetString("localId"); } }
        public double roundTripTime { get { return GetDouble("roundTripTime"); } }

        internal RTCRemoteInboundRtpStreamStats(IntPtr ptr) : base(ptr)
        {
        }
    }

    public class RTCRemoteOutboundRtpStreamStats : RTCStats
    {
        internal RTCRemoteOutboundRtpStreamStats(IntPtr ptr) : base(ptr)
        {
        }
    }

    public class RTCRemoteRTPStreamStats : RTCStats
    {
        internal RTCRemoteRTPStreamStats(IntPtr ptr) : base(ptr)
        {

        }
    }

    public class RTCMediaSourceStats : RTCStats
    {
        public string trackIdentifier { get { return GetString("trackIdentifier"); } }
        public string kind { get { return GetString("kind"); } }
        public uint width { get { return GetUnsignedInt("width"); } }
        public uint height { get { return GetUnsignedInt("height"); } }
        public uint frames { get { return GetUnsignedInt("frames"); } }
        public uint framesPerSecond { get { return GetUnsignedInt("framesPerSecond"); } }

        internal RTCMediaSourceStats(IntPtr ptr) : base(ptr)
        {
        }
    }

    public class RTCPeerConnectionStats : RTCStats
    {
        public uint dataChannelsOpened { get { return GetUnsignedInt("dataChannelsOpened"); } }
        public uint dataChannelsClosed { get { return GetUnsignedInt("dataChannelsClosed"); } }

        internal RTCPeerConnectionStats(IntPtr ptr) : base(ptr)
        {
        }
    }

    public class RTCDataChannelStats : RTCStats
    {
        public string label { get { return GetString("label"); } }
        public string protocol { get { return GetString("protocol"); } }
        public int dataChannelIdentifier { get { return GetInt("dataChannelIdentifier"); } }
        public string state { get { return GetString("state"); } }
        public uint messagesSent { get { return GetUnsignedInt("messagesSent"); } }
        public ulong bytesSent { get { return GetUnsignedLong("bytesSent"); } }
        public uint messagesReceived { get { return GetUnsignedInt("messagesReceived"); } }
        public ulong bytesReceived { get { return GetUnsignedLong("bytesReceived"); } }


        internal RTCDataChannelStats(IntPtr ptr) : base(ptr)
        {
        }
    }

    public class RTCTransceiverStats : RTCStats
    {
        internal RTCTransceiverStats(IntPtr ptr) : base(ptr)
        {
        }
    }
    public class RTCSenderStats : RTCStats
    {
        internal RTCSenderStats(IntPtr ptr) : base(ptr)
        {
        }
    }
    public class RTCReceiverStats : RTCStats
    {
        internal RTCReceiverStats(IntPtr ptr) : base(ptr)
        {
        }
    }

    public class RTCTransportStats : RTCStats
    {
        public ulong bytesSent { get { return GetUnsignedLong("bytesSent"); } }
        public ulong bytesReceived { get { return GetUnsignedLong("bytesReceived"); } }
        public string rtcpTransportStatsId { get { return GetString("rtcpTransportStatsId"); } }
        public string dtlsState { get { return GetString("dtlsState"); } }
        public string selectedCandidatePairId { get { return GetString("selectedCandidatePairId"); } }
        public string localCertificateId { get { return GetString("localCertificateId"); } }
        public string remoteCertificateId { get { return GetString("remoteCertificateId"); } }
        public string tlsVersion { get { return GetString("tlsVersion"); } }
        public string dtlsCipher { get { return GetString("dtlsCipher"); } }
        public string srtpCipher { get { return GetString("srtpCipher"); } }
        public uint selectedCandidatePairChanges { get { return GetUnsignedInt("selectedCandidatePairChanges"); } }

        internal RTCTransportStats(IntPtr ptr) : base(ptr)
        {
        }
    }



    internal class StatsFactory
    {
        static Dictionary<RTCStatsType, Func<IntPtr, RTCStats>> m_map;

        static StatsFactory()
        {
            m_map = new Dictionary<RTCStatsType, Func<IntPtr, RTCStats>>()
            {
                { RTCStatsType.Codec, ptr => new RTCCodecStats(ptr) },
                { RTCStatsType.InboundRtp, ptr => new RTCInboundRTPStreamStats(ptr) },
                { RTCStatsType.OutboundRtp, ptr => new RTCOutboundRTPStreamStats(ptr) },
                { RTCStatsType.RemoteInboundRtp, ptr => new RTCRemoteInboundRtpStreamStats(ptr) },
                { RTCStatsType.RemoteOutboundRtp, ptr => new RTCRemoteOutboundRtpStreamStats(ptr) },
                { RTCStatsType.MediaSource, ptr => new RTCMediaSourceStats(ptr) },
                { RTCStatsType.Csrc, ptr => new RTCCodecStats(ptr) },
                { RTCStatsType.PeerConnection, ptr => new RTCPeerConnectionStats(ptr) },
                { RTCStatsType.DataChannel, ptr => new RTCDataChannelStats(ptr) },
                { RTCStatsType.Stream, ptr => new RTCMediaStreamStats(ptr) },
                { RTCStatsType.Track, ptr => new RTCMediaStreamTrackStats(ptr) },
                { RTCStatsType.Transceiver, ptr => new RTCTransceiverStats(ptr) },
                { RTCStatsType.Sender, ptr => new RTCSenderStats(ptr) },
                { RTCStatsType.Receiver, ptr => new RTCReceiverStats(ptr) },
                { RTCStatsType.Transport, ptr => new RTCTransportStats(ptr) },
                { RTCStatsType.SctpTransport, ptr => new RTCTransportStats(ptr) },
                { RTCStatsType.CandidatePair, ptr => new RTCIceCandidatePairStats(ptr) },
                { RTCStatsType.LocalCandidate, ptr => new RTCIceCandidateStats(ptr) },
                { RTCStatsType.RemoteCandidate, ptr => new RTCIceCandidateStats(ptr) },
                { RTCStatsType.Certificate, ptr => new RTCCertificateStats(ptr) },
            };
        }

        public static RTCStats Create(RTCStatsType type, IntPtr ptr)
        {
            return m_map[type](ptr);
        }
    }

    public class RTCStatsReport : IDisposable
    {
        private IntPtr self;
        private readonly Dictionary<(RTCStatsType, string), RTCStats> m_dictStats;

        private bool disposed;

        internal RTCStatsReport(IntPtr ptr)
        {
            self = ptr;
            WebRTC.Table.Add(self, this);

            uint length = 0;
            IntPtr ptrStatsTypeArray = IntPtr.Zero;
            IntPtr ptrStatsArray = NativeMethods.StatsReportGetStatsList(self, ref length, ref ptrStatsTypeArray);

            IntPtr[] array = ptrStatsArray.AsArray<IntPtr>((int)length);
            byte[] types = ptrStatsTypeArray.AsArray<byte>((int)length);

            m_dictStats = new Dictionary<(RTCStatsType, string), RTCStats>();
            for (int i = 0; i < length; i++)
            {
                RTCStatsType type = (RTCStatsType)types[i];
                RTCStats stats = StatsFactory.Create(type, array[i]);
                m_dictStats[(type, stats.Id)] = stats;
            }
        }

        ~RTCStatsReport()
        {
            this.Dispose();
            WebRTC.Table.Remove(self);
        }

        public void Dispose()
        {
            if (this.disposed)
            {
                return;
            }

            if (self != IntPtr.Zero && !WebRTC.Context.IsNull)
            {
                WebRTC.Context.DeleteStatsReport(self);
                self = IntPtr.Zero;
            }

            this.disposed = true;
            GC.SuppressFinalize(this);
        }

        //internal

        public IDictionary<(RTCStatsType, string), RTCStats> Stats
        {
            get { return m_dictStats; }
        }
    }
}
