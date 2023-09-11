using System;
using System.Collections.Generic;
using System.Linq;

namespace Unity.WebRTC
{
    /// <summary>
    ///
    /// </summary>
    public class StringValueAttribute : Attribute
    {
        /// <summary>
        ///
        /// </summary>
        public string StringValue { get; protected set; }

        /// <summary>
        ///
        /// </summary>
        /// <param name="value"></param>
        public StringValueAttribute(string value)
        {
            this.StringValue = value;
        }
    }

    /// <summary>
    ///
    /// </summary>
    public enum RTCStatsType
    {
        /// <summary>
        ///
        /// </summary>
        [StringValue("codec")]
        Codec = 0,

        /// <summary>
        ///
        /// </summary>
        [StringValue("inbound-rtp")]
        InboundRtp = 1,

        /// <summary>
        ///
        /// </summary>
        [StringValue("outbound-rtp")]
        OutboundRtp = 2,

        /// <summary>
        ///
        /// </summary>
        [StringValue("remote-inbound-rtp")]
        RemoteInboundRtp = 3,

        /// <summary>
        ///
        /// </summary>
        [StringValue("remote-outbound-rtp")]
        RemoteOutboundRtp = 4,

        /// <summary>
        ///
        /// </summary>
        [StringValue("media-source")]
        MediaSource = 5,

        /// <summary>
        ///
        /// </summary>
        [StringValue("media-playout")]
        MediaPlayOut = 6,

        /// <summary>
        ///
        /// </summary>
        [StringValue("peer-connection")]
        PeerConnection = 7,

        /// <summary>
        ///
        /// </summary>
        [StringValue("data-channel")]
        DataChannel = 8,

        /// <summary>
        ///
        /// </summary>
        [StringValue("transport")]
        Transport = 9,

        /// <summary>
        ///
        /// </summary>
        [StringValue("candidate-pair")]
        CandidatePair = 10,

        /// <summary>
        ///
        /// </summary>
        [StringValue("local-candidate")]
        LocalCandidate = 11,

        /// <summary>
        ///
        /// </summary>
        [StringValue("remote-candidate")]
        RemoteCandidate = 12,

        /// <summary>
        ///
        /// </summary>
        [StringValue("certificate")]
        Certificate = 13,
    }

    internal enum StatsMemberType
    {
        Bool, // bool
        Int32, // int32_t
        Uint32, // uint32_t
        Int64, // int64_t
        Uint64, // uint64_t
        Double, // double
        String, // std::string

        SequenceBool, // std::vector<bool>
        SequenceInt32, // std::vector<int32_t>
        SequenceUint32, // std::vector<uint32_t>
        SequenceInt64, // std::vector<int64_t>
        SequenceUint64, // std::vector<uint64_t>
        SequenceDouble, // std::vector<double>
        SequenceString, // std::vector<std::string>

        MapStringUint64, // std::map<std::string, uint64_t>
        MapStringDouble // std::map<std::string, double>
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

            IntPtr values;
            ulong length = 0;
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
                    return NativeMethods.StatsMemberGetBoolArray(self, out length).AsArray<bool>((int)length);
                case StatsMemberType.SequenceInt32:
                    return NativeMethods.StatsMemberGetIntArray(self, out length).AsArray<int>((int)length);
                case StatsMemberType.SequenceUint32:
                    return NativeMethods.StatsMemberGetUnsignedIntArray(self, out length).AsArray<uint>((int)length);
                case StatsMemberType.SequenceInt64:
                    return NativeMethods.StatsMemberGetLongArray(self, out length).AsArray<long>((int)length);
                case StatsMemberType.SequenceUint64:
                    return NativeMethods.StatsMemberGetUnsignedLongArray(self, out length).AsArray<ulong>((int)length);
                case StatsMemberType.SequenceDouble:
                    return NativeMethods.StatsMemberGetDoubleArray(self, out length).AsArray<double>((int)length);
                case StatsMemberType.SequenceString:
                    return NativeMethods.StatsMemberGetStringArray(self, out length).AsArray<string>((int)length);
                case StatsMemberType.MapStringUint64:
                    return NativeMethods.StatsMemberGetMapStringUint64(self, out values, out length).AsMap<ulong>(values, (int)length);
                case StatsMemberType.MapStringDouble:
                    return NativeMethods.StatsMemberGetMapStringDouble(self, out values, out length).AsMap<double>(values, (int)length);
                default:
                    throw new ArgumentException();
            }
        }
    }

    /// <summary>
    ///
    /// </summary>
    public class RTCStats
    {
        private IntPtr self;
        internal Dictionary<string, RTCStatsMember> m_members;
        internal Dictionary<string, object> m_dict;

        /// <summary>
        ///
        /// </summary>
        public RTCStatsType Type
        {
            get
            {
                return NativeMethods.StatsGetType(self);
            }
        }

        /// <summary>
        ///
        /// </summary>
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

        /// <summary>
        ///
        /// </summary>
        public DateTime UtcTimeStamp
        {
            get { return DateTimeOffset.FromUnixTimeMilliseconds(Timestamp / 1000).UtcDateTime; }
        }

        /// <summary>
        ///
        /// </summary>
        public IDictionary<string, object> Dict
        {
            get
            {
                if (m_dict == null)
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

            return NativeMethods.StatsMemberGetBoolArray(m_members[key].self, out ulong length)
                .AsArray<bool>((int)length);
        }

        internal int[] GetIntArray(string key)
        {
            if (!NativeMethods.StatsMemberIsDefined(m_members[key].self))
            {
                return default;
            }

            return NativeMethods.StatsMemberGetIntArray(m_members[key].self, out ulong length)
                .AsArray<int>((int)length);
        }

        internal uint[] GetUnsignedIntArray(string key)
        {
            if (!NativeMethods.StatsMemberIsDefined(m_members[key].self))
            {
                return default;
            }

            return NativeMethods.StatsMemberGetUnsignedIntArray(m_members[key].self, out ulong length)
                .AsArray<uint>((int)length);
        }

        internal long[] GetLongArray(string key)
        {
            if (!NativeMethods.StatsMemberIsDefined(m_members[key].self))
            {
                return default;
            }

            return NativeMethods.StatsMemberGetLongArray(m_members[key].self, out ulong length)
                .AsArray<long>((int)length);
        }

        internal ulong[] GetUnsignedLongArray(string key)
        {
            if (!NativeMethods.StatsMemberIsDefined(m_members[key].self))
            {
                return default;
            }

            return NativeMethods.StatsMemberGetUnsignedLongArray(m_members[key].self, out ulong length)
                .AsArray<ulong>((int)length);
        }

        internal double[] GetDoubleArray(string key)
        {
            if (!NativeMethods.StatsMemberIsDefined(m_members[key].self))
            {
                return default;
            }

            return NativeMethods.StatsMemberGetDoubleArray(m_members[key].self, out ulong length)
                .AsArray<double>((int)length);
        }

        internal string[] GetStringArray(string key)
        {
            if (!NativeMethods.StatsMemberIsDefined(m_members[key].self))
            {
                return default;
            }

            return NativeMethods.StatsMemberGetStringArray(m_members[key].self, out ulong length)
                .AsArray<string>((int)length);
        }

        internal Dictionary<string, double> GetMapStringDouble(string key)
        {
            if (!NativeMethods.StatsMemberIsDefined(m_members[key].self))
            {
                return default;
            }

            return NativeMethods.StatsMemberGetMapStringDouble(m_members[key].self, out IntPtr values, out ulong length)
                .AsMap<double>(values, (int)length);
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
            IntPtr ptr = NativeMethods.StatsGetMembers(self, out ulong length);
            IntPtr[] array = ptr.AsArray<IntPtr>((int)length);

            RTCStatsMember[] members = new RTCStatsMember[length];
            for (int i = 0; i < (int)length; i++)
            {
                members[i] = new RTCStatsMember(array[i]);
            }

            return members;
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public string ToJson()
        {
            return NativeMethods.StatsGetJson(self).AsAnsiStringWithFreeMem();
        }
    }

    /// <summary>
    ///
    /// </summary>
    public class RTCCertificateStats : RTCStats
    {
        /// <summary>
        ///
        /// </summary>
        public string fingerprint { get { return GetString("fingerprint"); } }

        /// <summary>
        ///
        /// </summary>
        public string fingerprintAlgorithm { get { return GetString("fingerprintAlgorithm"); } }

        /// <summary>
        ///
        /// </summary>
        public string base64Certificate { get { return GetString("base64Certificate"); } }

        /// <summary>
        ///
        /// </summary>
        public string issuerCertificateId { get { return GetString("issuerCertificateId"); } }

        internal RTCCertificateStats(IntPtr ptr) : base(ptr)
        {
        }
    }

    /// <summary>
    ///
    /// </summary>
    public class RTCCodecStats : RTCStats
    {
        /// <summary>
        ///
        /// </summary>
        public string transportId { get { return GetString("transportId"); } }

        /// <summary>
        ///
        /// </summary>
        public uint payloadType { get { return GetUnsignedInt("payloadType"); } }

        /// <summary>
        ///
        /// </summary>
        public string mimeType { get { return GetString("mimeType"); } }

        /// <summary>
        ///
        /// </summary>
        public uint clockRate { get { return GetUnsignedInt("clockRate"); } }

        /// <summary>
        ///
        /// </summary>
        public uint channels { get { return GetUnsignedInt("channels"); } }

        /// <summary>
        ///
        /// </summary>
        public string sdpFmtpLine { get { return GetString("sdpFmtpLine"); } }

        internal RTCCodecStats(IntPtr ptr) : base(ptr)
        {
        }
    }

    /// <summary>
    ///
    /// </summary>
    public class RTCDataChannelStats : RTCStats
    {
        /// <summary>
        ///
        /// </summary>
        public string label { get { return GetString("label"); } }

        /// <summary>
        ///
        /// </summary>
        public string protocol { get { return GetString("protocol"); } }

        /// <summary>
        ///
        /// </summary>
        public int dataChannelIdentifier { get { return GetInt("dataChannelIdentifier"); } }

        /// <summary>
        ///
        /// </summary>
        public string state { get { return GetString("state"); } }

        /// <summary>
        ///
        /// </summary>
        public uint messagesSent { get { return GetUnsignedInt("messagesSent"); } }

        /// <summary>
        ///
        /// </summary>
        public ulong bytesSent { get { return GetUnsignedLong("bytesSent"); } }

        /// <summary>
        ///
        /// </summary>
        public uint messagesReceived { get { return GetUnsignedInt("messagesReceived"); } }

        /// <summary>
        ///
        /// </summary>
        public ulong bytesReceived { get { return GetUnsignedLong("bytesReceived"); } }

        internal RTCDataChannelStats(IntPtr ptr) : base(ptr)
        {
        }
    }

    /// <summary>
    ///
    /// </summary>
    public class RTCIceCandidatePairStats : RTCStats
    {
        /// <summary>
        ///
        /// </summary>
        public string transportId { get { return GetString("transportId"); } }

        /// <summary>
        ///
        /// </summary>
        public string localCandidateId { get { return GetString("localCandidateId"); } }

        /// <summary>
        ///
        /// </summary>
        public string remoteCandidateId { get { return GetString("remoteCandidateId"); } }

        /// <summary>
        ///
        /// </summary>
        public string state { get { return GetString("state"); } }

        /// <summary>
        ///
        /// </summary>
        public bool nominated { get { return GetBool("nominated"); } }

        /// <summary>
        ///
        /// </summary>
        public ulong packetsSent { get { return GetUnsignedLong("packetsSent"); } }

        /// <summary>
        ///
        /// </summary>
        public ulong packetsReceived { get { return GetUnsignedLong("packetsReceived"); } }

        /// <summary>
        ///
        /// </summary>
        public ulong bytesSent { get { return GetUnsignedLong("bytesSent"); } }

        /// <summary>
        ///
        /// </summary>
        public ulong bytesReceived { get { return GetUnsignedLong("bytesReceived"); } }

        /// <summary>
        ///
        /// </summary>
        public double lastPacketSentTimestamp { get { return GetDouble("lastPacketSentTimestamp"); } }

        /// <summary>
        ///
        /// </summary>
        public double lastPacketReceivedTimestamp { get { return GetDouble("lastPacketReceivedTimestamp"); } }

        /// <summary>
        ///
        /// </summary>
        public double totalRoundTripTime { get { return GetDouble("totalRoundTripTime"); } }

        /// <summary>
        ///
        /// </summary>
        public double currentRoundTripTime { get { return GetDouble("currentRoundTripTime"); } }

        /// <summary>
        ///
        /// </summary>
        public double availableOutgoingBitrate { get { return GetDouble("availableOutgoingBitrate"); } }

        /// <summary>
        ///
        /// </summary>
        public double availableIncomingBitrate { get { return GetDouble("availableIncomingBitrate"); } }

        /// <summary>
        ///
        /// </summary>
        public ulong requestsReceived { get { return GetUnsignedLong("requestsReceived"); } }

        /// <summary>
        ///
        /// </summary>
        public ulong requestsSent { get { return GetUnsignedLong("requestsSent"); } }

        /// <summary>
        ///
        /// </summary>
        public ulong responsesReceived { get { return GetUnsignedLong("responsesReceived"); } }

        /// <summary>
        ///
        /// </summary>
        public ulong responsesSent { get { return GetUnsignedLong("responsesSent"); } }

        /// <summary>
        ///
        /// </summary>
        public ulong consentRequestsSent { get { return GetUnsignedLong("consentRequestsSent"); } }

        /// <summary>
        ///
        /// </summary>
        public ulong packetsDiscardedOnSend { get { return GetUnsignedLong("packetsDiscardedOnSend"); } }

        /// <summary>
        ///
        /// </summary>
        public ulong bytesDiscardedOnSend { get { return GetUnsignedLong("bytesDiscardedOnSend"); } }

        internal RTCIceCandidatePairStats(IntPtr ptr) : base(ptr)
        {
        }
    }

    /// <summary>
    ///
    /// </summary>
    public class RTCIceCandidateStats : RTCStats
    {
        /// <summary>
        ///
        /// </summary>
        public string transportId { get { return GetString("transportId"); } }

        /// <summary>
        ///
        /// </summary>
        [Obsolete]
        public bool isRemote { get { return GetBool("isRemote"); } }

        /// <summary>
        ///
        /// </summary>
        public string networkType { get { return GetString("networkType"); } }

        /// <summary>
        ///
        /// </summary>
        public string ip { get { return GetString("ip"); } }

        /// <summary>
        ///
        /// </summary>
        public string address { get { return GetString("address"); } }

        /// <summary>
        ///
        /// </summary>
        public int port { get { return GetInt("port"); } }

        /// <summary>
        ///
        /// </summary>
        public string protocol { get { return GetString("protocol"); } }

        /// <summary>
        ///
        /// </summary>
        public string relayProtocol { get { return GetString("relayProtocol"); } }

        /// <summary>
        ///
        /// </summary>
        public string candidateType { get { return GetString("candidateType"); } }

        /// <summary>
        ///
        /// </summary>
        public int priority { get { return GetInt("priority"); } }

        /// <summary>
        ///
        /// </summary>
        public string url { get { return GetString("url"); } }

        /// <summary>
        ///
        /// </summary>
        public string foundation { get { return GetString("foundation"); } }

        /// <summary>
        ///
        /// </summary>
        public string relatedAddress { get { return GetString("relatedAddress"); } }

        /// <summary>
        ///
        /// </summary>
        public int relatedPort { get { return GetInt("relatedPort"); } }

        /// <summary>
        ///
        /// </summary>
        public string usernameFragment { get { return GetString("usernameFragment"); } }

        /// <summary>
        ///
        /// </summary>
        public string tcpType { get { return GetString("tcpType"); } }

        /// <summary>
        ///
        /// </summary>
        public bool vpn { get { return GetBool("vpn"); } }

        /// <summary>
        ///
        /// </summary>
        public string networkAdapterType { get { return GetString("networkAdapterType"); } }

        internal RTCIceCandidateStats(IntPtr ptr) : base(ptr)
        {
        }
    }

    /// <summary>
    ///
    /// </summary>
    public class RTCPeerConnectionStats : RTCStats
    {

        /// <summary>
        ///
        /// </summary>
        public uint dataChannelsOpened { get { return GetUnsignedInt("dataChannelsOpened"); } }

        /// <summary>
        ///
        /// </summary>
        public uint dataChannelsClosed { get { return GetUnsignedInt("dataChannelsClosed"); } }

        internal RTCPeerConnectionStats(IntPtr ptr) : base(ptr)
        {
        }
    }

    /// <summary>
    ///
    /// </summary>
    public class RTCRTPStreamStats : RTCStats
    {

        /// <summary>
        ///
        /// </summary>
        public uint ssrc { get { return GetUnsignedInt("ssrc"); } }

        /// <summary>
        ///
        /// </summary>
        public string kind { get { return GetString("kind"); } }

        /// <summary>
        ///
        /// </summary>
        public string transportId { get { return GetString("transportId"); } }

        /// <summary>
        ///
        /// </summary>
        public string codecId { get { return GetString("codecId"); } }

        internal RTCRTPStreamStats(IntPtr ptr) : base(ptr)
        {
        }
    }

    /// <summary>
    ///
    /// </summary>
    public class RTCReceivedRtpStreamStats : RTCRTPStreamStats
    {
        /// <summary>
        ///
        /// </summary>
        public double jitter { get { return GetDouble("jitter"); } }

        /// <summary>
        ///
        /// </summary>
        public int packetsLost { get { return GetInt("packetsLost"); } }

        internal RTCReceivedRtpStreamStats(IntPtr ptr) : base(ptr)
        {
        }
    }

    /// <summary>
    ///
    /// </summary>
    public class RTCSentRtpStreamStats : RTCRTPStreamStats
    {
        /// <summary>
        ///
        /// </summary>
        public ulong packetsSent { get { return GetUnsignedLong("packetsSent"); } }

        /// <summary>
        ///
        /// </summary>
        public ulong bytesSent { get { return GetUnsignedLong("bytesSent"); } }

        internal RTCSentRtpStreamStats(IntPtr ptr) : base(ptr)
        {
        }
    }

    /// <summary>
    ///
    /// </summary>
    public class RTCInboundRTPStreamStats : RTCReceivedRtpStreamStats
    {
        /// <summary>
        ///
        /// </summary>
        public string playoutId { get { return GetString("playoutId"); } }

        /// <summary>
        ///
        /// </summary>
        public string trackIdentifier { get { return GetString("trackIdentifier"); } }

        /// <summary>
        ///
        /// </summary>
        public string mid { get { return GetString("mid"); } }

        /// <summary>
        ///
        /// </summary>
        public string remoteId { get { return GetString("remoteId"); } }

        /// <summary>
        ///
        /// </summary>
        public uint packetsReceived { get { return GetUnsignedInt("packetsReceived"); } }

        /// <summary>
        ///
        /// </summary>
        public ulong packetsDiscarded { get { return GetUnsignedLong("packetsDiscarded"); } }

        /// <summary>
        ///
        /// </summary>
        public ulong fecPacketsReceived { get { return GetUnsignedLong("fecPacketsReceived"); } }

        /// <summary>
        ///
        /// </summary>
        public ulong fecPacketsDiscarded { get { return GetUnsignedLong("fecPacketsDiscarded"); } }

        /// <summary>
        ///
        /// </summary>
        public ulong bytesReceived { get { return GetUnsignedLong("bytesReceived"); } }

        /// <summary>
        ///
        /// </summary>
        public ulong headerBytesReceived { get { return GetUnsignedLong("headerBytesReceived"); } }

        /// <summary>
        /// 
        /// </summary>
        public ulong retransmittedPacketsReceived { get { return GetUnsignedLong("retransmittedPacketsReceived"); } }

        /// <summary>
        /// 
        /// </summary>
        public ulong retransmittedBytesReceived { get { return GetUnsignedLong("retransmittedBytesReceived"); } }

        /// <summary>
        ///
        /// </summary>
        public double lastPacketReceivedTimestamp { get { return GetDouble("lastPacketReceivedTimestamp"); } }

        /// <summary>
        ///
        /// </summary>
        public double jitterBufferDelay { get { return GetDouble("jitterBufferDelay"); } }

        /// <summary>
        ///
        /// </summary>
        public double jitterBufferTargetDelay { get { return GetDouble("jitterBufferTargetDelay"); } }

        /// <summary>
        ///
        /// </summary>
        public double jitterBufferMinimumDelay { get { return GetDouble("jitterBufferMinimumDelay"); } }

        /// <summary>
        ///
        /// </summary>
        public ulong jitterBufferEmittedCount { get { return GetUnsignedLong("jitterBufferEmittedCount"); } }

        /// <summary>
        ///
        /// </summary>
        public ulong totalSamplesReceived { get { return GetUnsignedLong("totalSamplesReceived"); } }

        /// <summary>
        ///
        /// </summary>
        public ulong concealedSamples { get { return GetUnsignedLong("concealedSamples"); } }

        /// <summary>
        ///
        /// </summary>
        public ulong silentConcealedSamples { get { return GetUnsignedLong("silentConcealedSamples"); } }

        /// <summary>
        ///
        /// </summary>
        public ulong concealmentEvents { get { return GetUnsignedLong("concealmentEvents"); } }

        /// <summary>
        ///
        /// </summary>
        public ulong insertedSamplesForDeceleration { get { return GetUnsignedLong("insertedSamplesForDeceleration"); } }

        /// <summary>
        ///
        /// </summary>
        public ulong removedSamplesForAcceleration { get { return GetUnsignedLong("removedSamplesForAcceleration"); } }

        /// <summary>
        ///
        /// </summary>
        public double audioLevel { get { return GetDouble("audioLevel"); } }

        /// <summary>
        ///
        /// </summary>
        public double totalAudioEnergy { get { return GetDouble("totalAudioEnergy"); } }

        /// <summary>
        ///
        /// </summary>
        public double totalSamplesDuration { get { return GetDouble("totalSamplesDuration"); } }

        /// <summary>
        ///
        /// </summary>
        public uint framesReceived { get { return GetUnsignedInt("framesReceived"); } }

        /// <summary>
        ///
        /// </summary>
        public uint frameWidth { get { return GetUnsignedInt("frameWidth"); } }

        /// <summary>
        ///
        /// </summary>
        public uint frameHeight { get { return GetUnsignedInt("frameHeight"); } }

        /// <summary>
        ///
        /// </summary>
        public double framesPerSecond { get { return GetDouble("framesPerSecond"); } }

        /// <summary>
        ///
        /// </summary>
        public uint framesDecoded { get { return GetUnsignedInt("framesDecoded"); } }

        /// <summary>
        ///
        /// </summary>
        public uint keyFramesDecoded { get { return GetUnsignedInt("keyFramesDecoded"); } }

        /// <summary>
        ///
        /// </summary>
        public uint framesDropped { get { return GetUnsignedInt("framesDropped"); } }

        /// <summary>
        ///
        /// </summary>
        public double totalDecodeTime { get { return GetDouble("totalDecodeTime"); } }

        /// <summary>
        ///
        /// </summary>
        public double totalProcessingDelay { get { return GetDouble("totalProcessingDelay"); } }

        /// <summary>
        ///
        /// </summary>
        public double totalAssemblyTime { get { return GetDouble("totalAssemblyTime"); } }

        /// <summary>
        ///
        /// </summary>
        public uint framesAssembledFromMultiplePackets { get { return GetUnsignedInt("framesAssembledFromMultiplePackets"); } }

        /// <summary>
        ///
        /// </summary>
        public double totalInterFrameDelay { get { return GetDouble("totalInterFrameDelay"); } }

        /// <summary>
        ///
        /// </summary>
        public double totalSquaredInterFrameDelay { get { return GetDouble("totalSquaredInterFrameDelay"); } }

        /// <summary>
        ///
        /// </summary>
        public uint pauseCount { get { return GetUnsignedInt("pauseCount"); } }

        /// <summary>
        ///
        /// </summary>
        public double totalPausesDuration { get { return GetDouble("totalPausesDuration"); } }

        /// <summary>
        ///
        /// </summary>
        public uint freezeCount { get { return GetUnsignedInt("freezeCount"); } }

        /// <summary>
        ///
        /// </summary>
        public double totalFreezesDuration { get { return GetDouble("totalFreezesDuration"); } }

        /// <summary>
        ///
        /// </summary>
        public string contentType { get { return GetString("contentType"); } }

        /// <summary>
        ///
        /// </summary>
        public double estimatedPlayoutTimestamp { get { return GetDouble("estimatedPlayoutTimestamp"); } }

        /// <summary>
        ///
        /// </summary>
        public string decoderImplementation { get { return GetString("decoderImplementation"); } }

        /// <summary>
        ///
        /// </summary>
        public uint firCount { get { return GetUnsignedInt("firCount"); } }

        /// <summary>
        ///
        /// </summary>
        public uint pliCount { get { return GetUnsignedInt("pliCount"); } }

        /// <summary>
        ///
        /// </summary>
        public uint nackCount { get { return GetUnsignedInt("nackCount"); } }

        /// <summary>
        ///
        /// </summary>
        public ulong qpSum { get { return GetUnsignedLong("qpSum"); } }

        /// <summary>
        /// 
        /// </summary>
        public string googTimingFrameInfo { get { return GetString("googTimingFrameInfo"); } }

        /// <summary>
        /// 
        /// </summary>
        public bool powerEfficientDecoder { get { return GetBool("powerEfficientDecoder"); } }

        /// <summary>
        /// 
        /// </summary>
        public ulong jitterBufferFlushes { get { return GetUnsignedLong("jitterBufferFlushes"); } }

        /// <summary>
        /// 
        /// </summary>
        public ulong delayedPacketOutageSamples { get { return GetUnsignedLong("delayedPacketOutageSamples"); } }

        /// <summary>
        /// 
        /// </summary>
        public double relativePacketArrivalDelay { get { return GetDouble("relativePacketArrivalDelay"); } }

        /// <summary>
        /// 
        /// </summary>
        public ulong interruptionCount { get { return GetUnsignedLong("interruptionCount"); } }

        /// <summary>
        /// 
        /// </summary>
        public double totalInterruptionDuration { get { return GetDouble("totalInterruptionDuration"); } }

        /// <summary>
        /// 
        /// </summary>
        public double minPlayoutDelay { get { return GetDouble("minPlayoutDelay"); } }


        internal RTCInboundRTPStreamStats(IntPtr ptr) : base(ptr)
        {
        }
    }

    /// <summary>
    ///
    /// </summary>
    public class RTCOutboundRTPStreamStats : RTCSentRtpStreamStats
    {

        /// <summary>
        ///
        /// </summary>
        public string mediaSourceId { get { return GetString("mediaSourceId"); } }

        /// <summary>
        ///
        /// </summary>
        public string remoteId { get { return GetString("remoteId"); } }

        /// <summary>
        ///
        /// </summary>
        public string mid { get { return GetString("mid"); } }

        /// <summary>
        ///
        /// </summary>
        public string rid { get { return GetString("rid"); } }

        /// <summary>
        ///
        /// </summary>
        public ulong retransmittedPacketsSent { get { return GetUnsignedLong("retransmittedPacketsSent"); } }

        /// <summary>
        ///
        /// </summary>
        public ulong headerBytesSent { get { return GetUnsignedLong("headerBytesSent"); } }

        /// <summary>
        ///
        /// </summary>
        public ulong retransmittedBytesSent { get { return GetUnsignedLong("retransmittedBytesSent"); } }

        /// <summary>
        ///
        /// </summary>
        public double targetBitrate { get { return GetDouble("targetBitrate"); } }

        /// <summary>
        ///
        /// </summary>
        public uint framesEncoded { get { return GetUnsignedInt("framesEncoded"); } }

        /// <summary>
        ///
        /// </summary>
        public uint keyFramesEncoded { get { return GetUnsignedInt("keyFramesEncoded"); } }

        /// <summary>
        ///
        /// </summary>
        public double totalEncodeTime { get { return GetDouble("totalEncodeTime"); } }

        /// <summary>
        ///
        /// </summary>
        public ulong totalEncodedBytesTarget { get { return GetUnsignedLong("totalEncodedBytesTarget"); } }

        /// <summary>
        ///
        /// </summary>
        public uint frameWidth { get { return GetUnsignedInt("frameWidth"); } }

        /// <summary>
        ///
        /// </summary>
        public uint frameHeight { get { return GetUnsignedInt("frameHeight"); } }

        /// <summary>
        ///
        /// </summary>
        public double framesPerSecond { get { return GetDouble("framesPerSecond"); } }

        /// <summary>
        ///
        /// </summary>
        public uint framesSent { get { return GetUnsignedInt("framesSent"); } }

        /// <summary>
        ///
        /// </summary>
        public uint hugeFramesSent { get { return GetUnsignedInt("hugeFramesSent"); } }

        /// <summary>
        ///
        /// </summary>
        public double totalPacketSendDelay { get { return GetDouble("totalPacketSendDelay"); } }

        /// <summary>
        ///
        /// </summary>
        public string qualityLimitationReason { get { return GetString("qualityLimitationReason"); } }

        /// <summary>
        /// 
        /// </summary>
        public Dictionary<string, double> qualityLimitationDurations { get { return GetMapStringDouble("qualityLimitationDurations"); } }

        /// <summary>
        ///
        /// </summary>
        public uint qualityLimitationResolutionChanges { get { return GetUnsignedInt("qualityLimitationResolutionChanges"); } }

        /// <summary>
        ///
        /// </summary>
        public string contentType { get { return GetString("contentType"); } }

        /// <summary>
        ///
        /// </summary>
        public string encoderImplementation { get { return GetString("encoderImplementation"); } }

        /// <summary>
        ///
        /// </summary>
        public uint firCount { get { return GetUnsignedInt("firCount"); } }

        /// <summary>
        ///
        /// </summary>
        public uint pliCount { get { return GetUnsignedInt("pliCount"); } }

        /// <summary>
        ///
        /// </summary>
        public uint nackCount { get { return GetUnsignedInt("nackCount"); } }

        /// <summary>
        ///
        /// </summary>
        public ulong qpSum { get { return GetUnsignedLong("qpSum"); } }

        /// <summary>
        ///
        /// </summary>
        public bool active { get { return GetBool("active"); } }

        /// <summary>
        /// 
        /// </summary>
        public bool powerEfficientEncoder { get { return GetBool("powerEfficientEncoder"); } }

        /// <summary>
        /// 
        /// </summary>
        public string scalabilityMode { get { return GetString("scalabilityMode"); } }


        internal RTCOutboundRTPStreamStats(IntPtr ptr) : base(ptr)
        {
        }
    }

    /// <summary>
    ///
    /// </summary>
    public class RTCRemoteInboundRtpStreamStats : RTCReceivedRtpStreamStats
    {
        /// <summary>
        ///
        /// </summary>
        public string localId { get { return GetString("localId"); } }

        /// <summary>
        ///
        /// </summary>
        public double roundTripTime { get { return GetDouble("roundTripTime"); } }

        /// <summary>
        ///
        /// </summary>
        public double fractionLost { get { return GetDouble("fractionLost"); } }

        /// <summary>
        ///
        /// </summary>
        public double totalRoundTripTime { get { return GetDouble("totalRoundTripTime"); } }

        /// <summary>
        ///
        /// </summary>
        public int roundTripTimeMeasurements { get { return GetInt("roundTripTimeMeasurements"); } }

        internal RTCRemoteInboundRtpStreamStats(IntPtr ptr) : base(ptr)
        {
        }
    }

    /// <summary>
    ///
    /// </summary>
    public class RTCRemoteOutboundRtpStreamStats : RTCSentRtpStreamStats
    {
        /// <summary>
        ///
        /// </summary>
        public string localId { get { return GetString("localId"); } }

        /// <summary>
        ///
        /// </summary>
        public double remoteTimestamp { get { return GetDouble("remoteTimestamp"); } }

        /// <summary>
        ///
        /// </summary>
        public ulong reportsSent { get { return GetUnsignedLong("reportsSent"); } }

        /// <summary>
        /// 
        /// </summary>
        public double roundTripTime { get { return GetDouble("roundTripTime"); } }

        /// <summary>
        /// 
        /// </summary>
        public ulong roundTripTimeMeasurements { get { return GetUnsignedLong("roundTripTimeMeasurements"); } }

        /// <summary>
        /// 
        /// </summary>
        public double totalRoundTripTime { get { return GetDouble("totalRoundTripTime"); } }

        internal RTCRemoteOutboundRtpStreamStats(IntPtr ptr) : base(ptr)
        {
        }
    }

    /// <summary>
    ///
    /// </summary>
    public class RTCMediaSourceStats : RTCStats
    {
        /// <summary>
        ///
        /// </summary>
        public string trackIdentifier { get { return GetString("trackIdentifier"); } }

        /// <summary>
        ///
        /// </summary>
        public string kind { get { return GetString("kind"); } }

        internal RTCMediaSourceStats(IntPtr ptr) : base(ptr)
        {
        }
    }

    /// <summary>
    ///
    /// </summary>
    public class RTCAudioSourceStats : RTCMediaSourceStats
    {
        /// <summary>
        ///
        /// </summary>
        public double audioLevel { get { return GetDouble("audioLevel"); } }

        /// <summary>
        ///
        /// </summary>
        public double totalAudioEnergy { get { return GetDouble("totalAudioEnergy"); } }

        /// <summary>
        ///
        /// </summary>
        public double totalSamplesDuration { get { return GetDouble("totalSamplesDuration"); } }

        /// <summary>
        /// 
        /// </summary>
        public double echoReturnLoss { get { return GetDouble("echoReturnLoss"); } }

        /// <summary>
        /// 
        /// </summary>
        public double echoReturnLossEnhancement { get { return GetDouble("echoReturnLossEnhancement"); } }

        internal RTCAudioSourceStats(IntPtr ptr) : base(ptr)
        {
        }
    }

    /// <summary>
    ///
    /// </summary>
    public class RTCVideoSourceStats : RTCMediaSourceStats
    {
        /// <summary>
        ///
        /// </summary>
        public uint width { get { return GetUnsignedInt("width"); } }

        /// <summary>
        ///
        /// </summary>
        public uint height { get { return GetUnsignedInt("height"); } }

        /// <summary>
        ///
        /// </summary>
        public uint frames { get { return GetUnsignedInt("frames"); } }

        /// <summary>
        ///
        /// </summary>
        /// <remarks>
        /// RFC define double but chromium define uint32_t
        /// https://source.chromium.org/chromium/chromium/src/+/main:third_party/webrtc/api/stats/rtcstats_objects.h;l=645;bpv=0;bpt=1
        /// </remarks>
        public double framesPerSecond { get { return GetDouble("framesPerSecond"); } }

        internal RTCVideoSourceStats(IntPtr ptr) : base(ptr)
        {
        }
    }

    /// <summary>
    ///
    /// </summary>
    public class RTCAudioPlayoutStats : RTCStats
    {
        /// <summary>
        ///
        /// </summary>
        public string kind { get { return GetString("kind"); } }

        /// <summary>
        ///
        /// </summary>
        public double synthesizedSamplesDuration { get { return GetDouble("synthesizedSamplesDuration"); } }

        /// <summary>
        ///
        /// </summary>
        public ulong synthesizedSamplesEvents { get { return GetUnsignedLong("synthesizedSamplesEvents"); } }

        /// <summary>
        ///
        /// </summary>
        public double totalSamplesDuration { get { return GetDouble("totalSamplesDuration"); } }

        /// <summary>
        ///
        /// </summary>
        public double totalPlayoutDelay { get { return GetDouble("totalPlayoutDelay"); } }

        /// <summary>
        ///
        /// </summary>
        public ulong totalSamplesCount { get { return GetUnsignedLong("totalSamplesCount"); } }

        internal RTCAudioPlayoutStats(IntPtr ptr) : base(ptr)
        {
        }
    }

    /// <summary>
    ///
    /// </summary>
    public class RTCTransportStats : RTCStats
    {
        /// <summary>
        ///
        /// </summary>
        public ulong bytesSent { get { return GetUnsignedLong("bytesSent"); } }

        /// <summary>
        ///
        /// </summary>
        public ulong packetsSent { get { return GetUnsignedLong("packetsSent"); } }

        /// <summary>
        ///
        /// </summary>
        public ulong bytesReceived { get { return GetUnsignedLong("bytesReceived"); } }

        /// <summary>
        ///
        /// </summary>
        public ulong packetsReceived { get { return GetUnsignedLong("packetsReceived"); } }

        /// <summary>
        ///
        /// </summary>
        public string rtcpTransportStatsId { get { return GetString("rtcpTransportStatsId"); } }

        /// <summary>
        ///
        /// </summary>
        public string dtlsState { get { return GetString("dtlsState"); } }

        /// <summary>
        ///
        /// </summary>
        public string selectedCandidatePairId { get { return GetString("selectedCandidatePairId"); } }

        /// <summary>
        ///
        /// </summary>
        public string localCertificateId { get { return GetString("localCertificateId"); } }

        /// <summary>
        ///
        /// </summary>
        public string remoteCertificateId { get { return GetString("remoteCertificateId"); } }

        /// <summary>
        ///
        /// </summary>
        public string tlsVersion { get { return GetString("tlsVersion"); } }

        /// <summary>
        ///
        /// </summary>
        public string dtlsCipher { get { return GetString("dtlsCipher"); } }

        /// <summary>
        /// 
        /// </summary>
        public string dtlsRole { get { return GetString("dtlsRole"); } }

        /// <summary>
        ///
        /// </summary>
        public string srtpCipher { get { return GetString("srtpCipher"); } }

        /// <summary>
        ///
        /// </summary>
        public uint selectedCandidatePairChanges { get { return GetUnsignedInt("selectedCandidatePairChanges"); } }

        /// <summary>
        /// 
        /// </summary>
        public string iceRole { get { return GetString("iceRole"); } }

        /// <summary>
        /// 
        /// </summary>
        public string iceLocalUsernameFragment { get { return GetString("iceLocalUsernameFragment"); } }

        /// <summary>
        /// 
        /// </summary>
        public string iceState { get { return GetString("iceState"); } }

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
                {RTCStatsType.Codec, ptr => new RTCCodecStats(ptr)},
                {RTCStatsType.InboundRtp, ptr => new RTCInboundRTPStreamStats(ptr)},
                {RTCStatsType.OutboundRtp, ptr => new RTCOutboundRTPStreamStats(ptr)},
                {RTCStatsType.RemoteInboundRtp, ptr => new RTCRemoteInboundRtpStreamStats(ptr)},
                {RTCStatsType.RemoteOutboundRtp, ptr => new RTCRemoteOutboundRtpStreamStats(ptr)},
                {
                    RTCStatsType.MediaSource, ptr =>
                    {
                        var @base = new RTCMediaSourceStats(ptr);
                        if (@base.kind == "audio")
                        {
                            return new RTCAudioSourceStats(ptr);
                        }

                        return new RTCVideoSourceStats(ptr);
                    }
                },
                {RTCStatsType.MediaPlayOut, ptr => new RTCAudioPlayoutStats(ptr)},
                {RTCStatsType.PeerConnection, ptr => new RTCPeerConnectionStats(ptr)},
                {RTCStatsType.DataChannel, ptr => new RTCDataChannelStats(ptr)},
                {RTCStatsType.Transport, ptr => new RTCTransportStats(ptr)},
                {RTCStatsType.CandidatePair, ptr => new RTCIceCandidatePairStats(ptr)},
                {RTCStatsType.LocalCandidate, ptr => new RTCIceCandidateStats(ptr)},
                {RTCStatsType.RemoteCandidate, ptr => new RTCIceCandidateStats(ptr)},
                {RTCStatsType.Certificate, ptr => new RTCCertificateStats(ptr)},
            };
        }

        public static RTCStats Create(RTCStatsType type, IntPtr ptr)
        {
            return m_map.TryGetValue(type, out var constructor) ? constructor(ptr) : null;
        }
    }

    /// <summary>
    ///
    /// </summary>
    public class RTCStatsReport : IDisposable
    {
        private IntPtr self;
        private readonly Dictionary<string, RTCStats> m_dictStats;

        private bool disposed;

        internal RTCStatsReport(IntPtr ptr)
        {
            self = ptr;
            IntPtr ptrStatsTypeArray = IntPtr.Zero;
            IntPtr ptrStatsArray = WebRTC.Context.GetStatsList(self, out ulong length, ref ptrStatsTypeArray);
            if (ptrStatsArray == IntPtr.Zero)
                throw new ArgumentException("Invalid pointer.", "ptr");

            IntPtr[] array = ptrStatsArray.AsArray<IntPtr>((int)length);
            uint[] types = ptrStatsTypeArray.AsArray<uint>((int)length);

            m_dictStats = new Dictionary<string, RTCStats>();
            for (int i = 0; i < (int)length; i++)
            {
                RTCStatsType type = (RTCStatsType)types[i];
                RTCStats stats = StatsFactory.Create(type, array[i]);
                if (stats == null)
                {
                    continue;
                }
                m_dictStats[stats.Id] = stats;
            }

            WebRTC.Table.Add(self, this);
        }

        /// <summary>
        ///
        /// </summary>
        ~RTCStatsReport()
        {
            this.Dispose();
        }

        /// <summary>
        ///
        /// </summary>
        public void Dispose()
        {
            if (this.disposed)
            {
                return;
            }

            if (self != IntPtr.Zero && !WebRTC.Context.IsNull)
            {
                WebRTC.Context.DeleteStatsReport(self);
                WebRTC.Table.Remove(self);
                self = IntPtr.Zero;
            }

            this.disposed = true;
            GC.SuppressFinalize(this);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public RTCStats Get(string id)
        {
            return m_dictStats[id];
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="id"></param>
        /// <param name="stats"></param>
        /// <returns></returns>
        public bool TryGetValue(string id, out RTCStats stats)
        {
            return m_dictStats.TryGetValue(id, out stats);
        }

        /// <summary>
        ///
        /// </summary>
        public IDictionary<string, RTCStats> Stats
        {
            get { return m_dictStats; }
        }
    }
}
