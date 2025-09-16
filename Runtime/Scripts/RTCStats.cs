using System;
using System.Collections.Generic;
using System.Linq;

namespace Unity.WebRTC
{
    /// <summary>
    /// Associates a string value with an enum field for RTCStatsType.
    /// </summary>
    public class StringValueAttribute : Attribute
    {
        /// <summary>
        /// String value associated with the enum field.
        /// </summary>
        public string StringValue { get; protected set; }

        /// <summary>
        /// Constructor for StringValueAttribute.
        /// </summary>
        /// <param name="value">String value to associate with the enum field.</param>
        public StringValueAttribute(string value)
        {
            this.StringValue = value;
        }
    }

    /// <summary>
    /// Identifies the type of RTC statistics object.
    /// </summary>
    public enum RTCStatsType
    {
        /// <summary>
        /// Identifies the type of codec.
        /// </summary>
        [StringValue("codec")]
        Codec = 0,

        /// <summary>
        /// Identifies inbound RTP stream statistics.
        /// </summary>
        [StringValue("inbound-rtp")]
        InboundRtp = 1,

        /// <summary>
        /// Identifies outbound RTP stream statistics.
        /// </summary>
        [StringValue("outbound-rtp")]
        OutboundRtp = 2,

        /// <summary>
        /// Identifies statistics related to remote inbound RTP streams.
        /// </summary>
        [StringValue("remote-inbound-rtp")]
        RemoteInboundRtp = 3,

        /// <summary>
        /// Identifies statistics related to remote outbound RTP streams.
        /// </summary>
        [StringValue("remote-outbound-rtp")]
        RemoteOutboundRtp = 4,

        /// <summary>
        /// Identifies statistics related to media sources.
        /// </summary>
        [StringValue("media-source")]
        MediaSource = 5,

        /// <summary>
        /// Identifies statistics related to media playout.
        /// </summary>
        [StringValue("media-playout")]
        MediaPlayOut = 6,

        /// <summary>
        /// Identifies statistics related to a peer connection.
        /// </summary>
        [StringValue("peer-connection")]
        PeerConnection = 7,

        /// <summary>
        /// Identifies statistics related to a data channel.
        /// </summary>
        [StringValue("data-channel")]
        DataChannel = 8,

        /// <summary>
        /// Identifies statistics related to a transport.
        /// </summary>
        [StringValue("transport")]
        Transport = 9,

        /// <summary>
        /// Identifies statistics related to a candidate pair.
        /// </summary>
        [StringValue("candidate-pair")]
        CandidatePair = 10,

        /// <summary>
        /// Identifies statistics related to a local ICE candidate.
        /// </summary>
        [StringValue("local-candidate")]
        LocalCandidate = 11,

        /// <summary>
        /// Identifies statistics related to a remote ICE candidate.
        /// </summary>
        [StringValue("remote-candidate")]
        RemoteCandidate = 12,

        /// <summary>
        /// Identifies statistics related to a certificate.
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
    /// Base class for all RTC statistics objects.
    /// </summary>
    public class RTCStats
    {
        private IntPtr self;
        internal Dictionary<string, RTCStatsMember> m_members;
        internal Dictionary<string, object> m_dict;

        /// <summary>
        /// Gets the type of this statistics object.
        /// </summary>
        public RTCStatsType Type
        {
            get
            {
                return NativeMethods.StatsGetType(self);
            }
        }

        /// <summary>
        /// Returns the unique identifier for this stats entry.
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
        /// Converts the timestamp to a UTC DateTime value.
        /// </summary>
        public DateTime UtcTimeStamp
        {
            get { return DateTimeOffset.FromUnixTimeMilliseconds(Timestamp / 1000).UtcDateTime; }
        }

        /// <summary>
        /// Returns a dictionary of all stats members and their values.
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
        /// Serializes the stats object to a JSON string.
        /// </summary>
        /// <returns>JSON representation of the stats.</returns>
        public string ToJson()
        {
            return NativeMethods.StatsGetJson(self).AsAnsiStringWithFreeMem();
        }
    }

    /// <summary>
    /// Contains certificate-related statistics for a peer connection.
    /// </summary>
    public class RTCCertificateStats : RTCStats
    {
        /// <summary>
        /// The fingerprint of the certificate.
        /// </summary>
        public string fingerprint { get { return GetString("fingerprint"); } }

        /// <summary>
        /// The algorithm used to generate the fingerprint.
        /// </summary>
        public string fingerprintAlgorithm { get { return GetString("fingerprintAlgorithm"); } }

        /// <summary>
        /// The base64-encoded certificate.
        /// </summary>
        public string base64Certificate { get { return GetString("base64Certificate"); } }

        /// <summary>
        /// The ID of the certificate that issued this certificate, if any.
        /// </summary>
        public string issuerCertificateId { get { return GetString("issuerCertificateId"); } }

        internal RTCCertificateStats(IntPtr ptr) : base(ptr)
        {
        }
    }

    /// <summary>
    /// Provides codec-specific statistics for RTP streams.
    /// </summary>
    public class RTCCodecStats : RTCStats
    {
        /// <summary>
        /// Identifies the transport used by this codec.
        /// </summary>
        public string transportId { get { return GetString("transportId"); } }

        /// <summary>
        /// Identifies the payload type for this codec.
        /// </summary>
        public uint payloadType { get { return GetUnsignedInt("payloadType"); } }

        /// <summary>
        /// Identifies the codec's MIME type and subtype.
        /// </summary>
        public string mimeType { get { return GetString("mimeType"); } }

        /// <summary>
        /// Identifies the codec's clock rate.
        /// </summary>
        public uint clockRate { get { return GetUnsignedInt("clockRate"); } }

        /// <summary>
        /// Identifies the codec's number of channels.
        /// </summary>
        public uint channels { get { return GetUnsignedInt("channels"); } }

        /// <summary>
        /// The SDP format parameters associated with this codec.
        /// </summary>
        public string sdpFmtpLine { get { return GetString("sdpFmtpLine"); } }

        internal RTCCodecStats(IntPtr ptr) : base(ptr)
        {
        }
    }

    /// <summary>
    /// Reports statistics for a data channel.
    /// </summary>
    public class RTCDataChannelStats : RTCStats
    {
        /// <summary>
        /// The label of the data channel.
        /// </summary>
        public string label { get { return GetString("label"); } }

        /// <summary>
        /// The protocol used by the data channel.
        /// </summary>
        public string protocol { get { return GetString("protocol"); } }

        /// <summary>
        /// The identifier for the data channel.
        /// </summary>
        public int dataChannelIdentifier { get { return GetInt("dataChannelIdentifier"); } }

        /// <summary>
        /// The state of the data channel.
        /// </summary>
        public string state { get { return GetString("state"); } }

        /// <summary>
        /// The number of messages sent over the data channel.
        /// </summary>
        public uint messagesSent { get { return GetUnsignedInt("messagesSent"); } }

        /// <summary>
        /// The number of bytes sent over the data channel.
        /// </summary>
        public ulong bytesSent { get { return GetUnsignedLong("bytesSent"); } }

        /// <summary>
        /// The number of messages received over the data channel.
        /// </summary>
        public uint messagesReceived { get { return GetUnsignedInt("messagesReceived"); } }

        /// <summary>
        /// The number of bytes received over the data channel.
        /// </summary>
        public ulong bytesReceived { get { return GetUnsignedLong("bytesReceived"); } }

        internal RTCDataChannelStats(IntPtr ptr) : base(ptr)
        {
        }
    }

    /// <summary>
    /// Details statistics for a pair of ICE candidates.
    /// </summary>
    public class RTCIceCandidatePairStats : RTCStats
    {
        /// <summary>
        /// Identifier for the transport used by this candidate pair.
        /// </summary>
        public string transportId { get { return GetString("transportId"); } }

        /// <summary>
        /// ID of the local ICE candidate in this pair.
        /// </summary>
        public string localCandidateId { get { return GetString("localCandidateId"); } }

        /// <summary>
        /// ID of the remote ICE candidate in this pair.
        /// </summary>
        public string remoteCandidateId { get { return GetString("remoteCandidateId"); } }

        /// <summary>
        /// Current state of the candidate pair (e.g., succeeded, failed).
        /// </summary>
        public string state { get { return GetString("state"); } }

        /// <summary>
        /// Indicates if this pair is nominated for use.
        /// </summary>
        public bool nominated { get { return GetBool("nominated"); } }

        /// <summary>
        /// Number of packets sent over this candidate pair.
        /// </summary>
        public ulong packetsSent { get { return GetUnsignedLong("packetsSent"); } }

        /// <summary>
        /// Number of packets received over this candidate pair.
        /// </summary>
        public ulong packetsReceived { get { return GetUnsignedLong("packetsReceived"); } }

        /// <summary>
        /// Total bytes sent over this candidate pair.
        /// </summary>
        public ulong bytesSent { get { return GetUnsignedLong("bytesSent"); } }

        /// <summary>
        /// Total bytes received over this candidate pair.
        /// </summary>
        public ulong bytesReceived { get { return GetUnsignedLong("bytesReceived"); } }

        /// <summary>
        /// Timestamp of the last packet sent.
        /// </summary>
        public double lastPacketSentTimestamp { get { return GetDouble("lastPacketSentTimestamp"); } }

        /// <summary>
        /// Timestamp of the last packet received.
        /// </summary>
        public double lastPacketReceivedTimestamp { get { return GetDouble("lastPacketReceivedTimestamp"); } }

        /// <summary>
        /// Round-trip time for this candidate pair.
        /// </summary>
        public double totalRoundTripTime { get { return GetDouble("totalRoundTripTime"); } }

        /// <summary>
        /// Most recent round-trip time measurement.
        /// </summary>
        public double currentRoundTripTime { get { return GetDouble("currentRoundTripTime"); } }

        /// <summary>
        /// Estimated available outgoing bitrate.
        /// </summary>
        public double availableOutgoingBitrate { get { return GetDouble("availableOutgoingBitrate"); } }

        /// <summary>
        /// Estimated available incoming bitrate.
        /// </summary>
        public double availableIncomingBitrate { get { return GetDouble("availableIncomingBitrate"); } }

        /// <summary>
        /// Number of STUN requests received.
        /// </summary>
        public ulong requestsReceived { get { return GetUnsignedLong("requestsReceived"); } }

        /// <summary>
        /// Number of STUN requests sent.
        /// </summary>
        public ulong requestsSent { get { return GetUnsignedLong("requestsSent"); } }

        /// <summary>
        /// Number of STUN responses received.
        /// </summary>
        public ulong responsesReceived { get { return GetUnsignedLong("responsesReceived"); } }

        /// <summary>
        /// Number of STUN responses sent.
        /// </summary>
        public ulong responsesSent { get { return GetUnsignedLong("responsesSent"); } }

        /// <summary>
        /// Number of consent requests sent.
        /// </summary>
        public ulong consentRequestsSent { get { return GetUnsignedLong("consentRequestsSent"); } }

        /// <summary>
        /// Number of packets discarded during sending.
        /// </summary>
        public ulong packetsDiscardedOnSend { get { return GetUnsignedLong("packetsDiscardedOnSend"); } }

        /// <summary>
        /// Number of bytes discarded during sending.
        /// </summary>
        public ulong bytesDiscardedOnSend { get { return GetUnsignedLong("bytesDiscardedOnSend"); } }

        internal RTCIceCandidatePairStats(IntPtr ptr) : base(ptr)
        {
        }
    }

    /// <summary>
    /// Contains statistics for a single ICE candidate.
    /// </summary>
    public class RTCIceCandidateStats : RTCStats
    {
        /// <summary>
        /// Identifier for the transport associated with this candidate.
        /// </summary>
        public string transportId { get { return GetString("transportId"); } }

        /// <summary>
        /// Indicates if this is a remote candidate.
        /// </summary>
        [Obsolete]
        public bool isRemote { get { return GetBool("isRemote"); } }

        /// <summary>
        /// The network type of the candidate (e.g., "wifi", "ethernet").
        /// </summary>
        public string networkType { get { return GetString("networkType"); } }

        /// <summary>
        /// The IP address of the candidate.
        /// </summary>
        public string ip { get { return GetString("ip"); } }

        /// <summary>
        /// The port number of the candidate.
        /// </summary>
        public int port { get { return GetInt("port"); } }

        /// <summary>
        /// The address of the candidate.
        /// </summary>
        public string address { get { return GetString("address"); } }

        /// <summary>
        /// The candidate's priority.
        /// </summary>
        public int priority { get { return GetInt("priority"); } }

        /// <summary>
        /// The protocol used by the candidate (e.g., "udp", "tcp").
        /// </summary>
        public string protocol { get { return GetString("protocol"); } }

        /// <summary>
        /// The transport protocol used by the relay candidate (e.g., "udp", "tcp", "tls").
        /// </summary>
        public string relayProtocol { get { return GetString("relayProtocol"); } }

        /// <summary>
        /// The type of the candidate (e.g., "host", "srflx", "prflx", "relay").
        /// </summary>
        public string candidateType { get { return GetString("candidateType"); } }

        /// <summary>
        /// The URL of the TURN server used by the candidate, if applicable.
        /// </summary>
        public string url { get { return GetString("url"); } }

        /// <summary>
        /// The string which uniquely identifies the candidate.
        /// </summary>
        public string foundation { get { return GetString("foundation"); } }

        /// <summary>
        /// The related address of the candidate, if applicable.
        /// </summary>
        public string relatedAddress { get { return GetString("relatedAddress"); } }

        /// <summary>
        /// The related port of the candidate, if applicable.
        /// </summary>
        public int relatedPort { get { return GetInt("relatedPort"); } }

        /// <summary>
        /// The username fragment used in ICE negotiation.
        /// </summary>
        public string usernameFragment { get { return GetString("usernameFragment"); } }

        /// <summary>
        /// The TCP type of the candidate (e.g., "active", "passive", "so").
        /// </summary>
        public string tcpType { get { return GetString("tcpType"); } }

        /// <summary>
        /// Indicates if the candidate is on a VPN.
        /// </summary>
        public bool vpn { get { return GetBool("vpn"); } }

        /// <summary>
        /// The type of network adapter used by the candidate (e.g., "ethernet", "wifi").
        /// </summary>
        public string networkAdapterType { get { return GetString("networkAdapterType"); } }

        internal RTCIceCandidateStats(IntPtr ptr) : base(ptr)
        {
        }
    }

    /// <summary>
    /// Provides statistics for the peer connection itself.
    /// </summary>
    public class RTCPeerConnectionStats : RTCStats
    {

        /// <summary>
        /// The number of data channels opened by the peer connection.
        /// </summary>
        public uint dataChannelsOpened { get { return GetUnsignedInt("dataChannelsOpened"); } }

        /// <summary>
        /// The number of data channels closed by the peer connection.
        /// </summary>
        public uint dataChannelsClosed { get { return GetUnsignedInt("dataChannelsClosed"); } }

        internal RTCPeerConnectionStats(IntPtr ptr) : base(ptr)
        {
        }
    }

    /// <summary>
    /// Base class for RTP stream statistics.
    /// </summary>
    public class RTCRTPStreamStats : RTCStats
    {

        /// <summary>
        /// The SSRC identifier for the RTP stream.
        /// </summary>
        public uint ssrc { get { return GetUnsignedInt("ssrc"); } }

        /// <summary>
        /// The media type of the RTP stream (e.g., "audio", "video").
        /// </summary>
        public string kind { get { return GetString("kind"); } }

        /// <summary>
        /// The identifier for the transport used by this RTP stream.
        /// </summary>
        public string transportId { get { return GetString("transportId"); } }

        /// <summary>
        /// The identifier for the RTCCodecStats used by this RTP stream.
        /// </summary>
        public string codecId { get { return GetString("codecId"); } }

        internal RTCRTPStreamStats(IntPtr ptr) : base(ptr)
        {
        }
    }

    /// <summary>
    /// Statistics for received RTP streams.
    /// </summary>
    public class RTCReceivedRtpStreamStats : RTCRTPStreamStats
    {
        /// <summary>
        /// The amount of jitter experienced by the RTP stream.
        /// </summary>
        public double jitter { get { return GetDouble("jitter"); } }

        /// <summary>
        /// The number of packets lost in the RTP stream.
        /// </summary>
        public int packetsLost { get { return GetInt("packetsLost"); } }

        internal RTCReceivedRtpStreamStats(IntPtr ptr) : base(ptr)
        {
        }
    }

    /// <summary>
    /// Statistics for sent RTP streams.
    /// </summary>
    public class RTCSentRtpStreamStats : RTCRTPStreamStats
    {
        /// <summary>
        /// The number of packets sent in the RTP stream.
        /// </summary>
        public ulong packetsSent { get { return GetUnsignedLong("packetsSent"); } }

        /// <summary>
        /// The total number of bytes sent in the RTP stream.
        /// </summary>
        public ulong bytesSent { get { return GetUnsignedLong("bytesSent"); } }

        internal RTCSentRtpStreamStats(IntPtr ptr) : base(ptr)
        {
        }
    }

    /// <summary>
    /// Reports inbound RTP stream statistics.
    /// </summary>
    public class RTCInboundRTPStreamStats : RTCReceivedRtpStreamStats
    {
        /// <summary>
        /// Identifier for the media source of this RTP stream.
        /// </summary>
        public string playoutId { get { return GetString("playoutId"); } }

        /// <summary>
        /// Identifier for the id value of the <see cref="MediaStreamTrack"/> of this RTP stream.
        /// </summary>
        public string trackIdentifier { get { return GetString("trackIdentifier"); } }

        /// <summary>
        /// The type of media being transmitted (e.g., "audio", "video").
        /// </summary>
        public string mid { get { return GetString("mid"); } }

        /// <summary>
        /// Identifier for the remote media source of this RTP stream.
        /// </summary>
        public string remoteId { get { return GetString("remoteId"); } }

        /// <summary>
        /// The total number of RTP packets received.
        /// </summary>
        public uint packetsReceived { get { return GetUnsignedInt("packetsReceived"); } }

        /// <summary>
        /// The total number of RTP packets discarded.
        /// </summary>
        public ulong packetsDiscarded { get { return GetUnsignedLong("packetsDiscarded"); } }

        /// <summary>
        /// The total number of received Forward Error Correction (FEC) packets.
        /// </summary>
        public ulong fecPacketsReceived { get { return GetUnsignedLong("fecPacketsReceived"); } }

        /// <summary>
        /// The total number of discarded Forward Error Correction (FEC) packets.
        /// </summary>
        public ulong fecPacketsDiscarded { get { return GetUnsignedLong("fecPacketsDiscarded"); } }

        /// <summary>
        /// The total number of bytes received in the RTP stream.
        /// </summary>
        public ulong bytesReceived { get { return GetUnsignedLong("bytesReceived"); } }

        /// <summary>
        /// The total number of header bytes received in the RTP stream.
        /// </summary>
        public ulong headerBytesReceived { get { return GetUnsignedLong("headerBytesReceived"); } }

        /// <summary>
        /// The total number of payload bytes received in the RTP stream.
        /// </summary>
        public ulong retransmittedPacketsReceived { get { return GetUnsignedLong("retransmittedPacketsReceived"); } }

        /// <summary>
        /// The total number of retransmitted bytes received in the RTP stream.
        /// </summary>
        public ulong retransmittedBytesReceived { get { return GetUnsignedLong("retransmittedBytesReceived"); } }

        /// <summary>
        /// Timestamp of the last packet received.
        /// </summary>
        public double lastPacketReceivedTimestamp { get { return GetDouble("lastPacketReceivedTimestamp"); } }

        /// <summary>
        /// The total number of packets that arrived late and were discarded.
        /// </summary>
        public double jitterBufferDelay { get { return GetDouble("jitterBufferDelay"); } }

        /// <summary>
        /// The target delay for the jitter buffer.
        /// </summary>
        public double jitterBufferTargetDelay { get { return GetDouble("jitterBufferTargetDelay"); } }

        /// <summary>
        /// The minimum delay experienced by the jitter buffer.
        /// </summary>
        public double jitterBufferMinimumDelay { get { return GetDouble("jitterBufferMinimumDelay"); } }

        /// <summary>
        /// The total number of packets emitted from the jitter buffer.
        /// </summary>
        public ulong jitterBufferEmittedCount { get { return GetUnsignedLong("jitterBufferEmittedCount"); } }

        /// <summary>
        /// The total number of samples received on this stream, including concealedSamples.
        /// </summary>
        public ulong totalSamplesReceived { get { return GetUnsignedLong("totalSamplesReceived"); } }

        /// <summary>
        /// The total number of concealed samples for the received audio track
        /// </summary>
        public ulong concealedSamples { get { return GetUnsignedLong("concealedSamples"); } }

        /// <summary>
        /// The total number of silent concealed samples for the received audio track
        /// </summary>
        public ulong silentConcealedSamples { get { return GetUnsignedLong("silentConcealedSamples"); } }

        /// <summary>
        /// The total number of concealment events for the received audio track
        /// </summary>
        public ulong concealmentEvents { get { return GetUnsignedLong("concealmentEvents"); } }

        /// <summary>
        /// The total number of inserted samples for the received audio track
        /// </summary>
        public ulong insertedSamplesForDeceleration { get { return GetUnsignedLong("insertedSamplesForDeceleration"); } }

        /// <summary>
        /// The total number of removed samples for the received audio track
        /// </summary>
        public ulong removedSamplesForAcceleration { get { return GetUnsignedLong("removedSamplesForAcceleration"); } }

        /// <summary>
        /// The audio level for the received audio track, between 0.0 and 1.0.
        /// </summary>
        public double audioLevel { get { return GetDouble("audioLevel"); } }

        /// <summary>
        /// The total audio energy for the received audio track.
        /// </summary>
        public double totalAudioEnergy { get { return GetDouble("totalAudioEnergy"); } }

        /// <summary>
        /// The total duration of audio samples received on this stream.
        /// </summary>
        public double totalSamplesDuration { get { return GetDouble("totalSamplesDuration"); } }

        /// <summary>
        /// The total number of video frames received on this stream.
        /// </summary>
        public uint framesReceived { get { return GetUnsignedInt("framesReceived"); } }

        /// <summary>
        /// The width of the last decoded frame.
        /// </summary>
        public uint frameWidth { get { return GetUnsignedInt("frameWidth"); } }

        /// <summary>
        /// The height of the last decoded frame.
        /// </summary>
        public uint frameHeight { get { return GetUnsignedInt("frameHeight"); } }

        /// <summary>
        /// The frame rate of the received video stream.
        /// </summary>
        public double framesPerSecond { get { return GetDouble("framesPerSecond"); } }

        /// <summary>
        /// The total number of frames dropped for the received video stream.
        /// </summary>
        public uint framesDecoded { get { return GetUnsignedInt("framesDecoded"); } }

        /// <summary>
        /// The total number of key frames decoded for the received video stream.
        /// </summary>
        public uint keyFramesDecoded { get { return GetUnsignedInt("keyFramesDecoded"); } }

        /// <summary>
        /// The total number of frames dropped for the received video stream.
        /// </summary>
        public uint framesDropped { get { return GetUnsignedInt("framesDropped"); } }

        /// <summary>
        /// The total time spent decoding frames for the received video stream.
        /// </summary>
        public double totalDecodeTime { get { return GetDouble("totalDecodeTime"); } }

        /// <summary>
        /// The total time spent processing frames for the received video stream.
        /// </summary>
        public double totalProcessingDelay { get { return GetDouble("totalProcessingDelay"); } }

        /// <summary>
        /// The total time spent assembling frames for the received video stream.
        /// </summary>
        public double totalAssemblyTime { get { return GetDouble("totalAssemblyTime"); } }

        /// <summary>
        /// The total number of frames that were assembled from multiple packets.
        /// </summary>
        public uint framesAssembledFromMultiplePackets { get { return GetUnsignedInt("framesAssembledFromMultiplePackets"); } }

        /// <summary>
        /// The total number of frames that were decoded from multiple packets.
        /// </summary>
        public double totalInterFrameDelay { get { return GetDouble("totalInterFrameDelay"); } }

        /// <summary>
        /// The total squared inter-frame delay for the received video stream.
        /// </summary>
        public double totalSquaredInterFrameDelay { get { return GetDouble("totalSquaredInterFrameDelay"); } }

        /// <summary>
        /// The total number of pauses in the received video stream.
        /// </summary>
        public uint pauseCount { get { return GetUnsignedInt("pauseCount"); } }

        /// <summary>
        /// The total duration of pauses in the received video stream.
        /// </summary>
        public double totalPausesDuration { get { return GetDouble("totalPausesDuration"); } }

        /// <summary>
        /// The total number of freezes in the received video stream.
        /// </summary>
        public uint freezeCount { get { return GetUnsignedInt("freezeCount"); } }

        /// <summary>
        /// The total duration of freezes in the received video stream.
        /// </summary>
        public double totalFreezesDuration { get { return GetDouble("totalFreezesDuration"); } }

        /// <summary>
        /// the video-content-type of the last key frame sent.
        /// </summary>
        public string contentType { get { return GetString("contentType"); } }

        /// <summary>
        /// The estimated playout timestamp for the last frame.
        /// </summary>
        public double estimatedPlayoutTimestamp { get { return GetDouble("estimatedPlayoutTimestamp"); } }

        /// <summary>
        /// Identifies the decoder implementation used.
        /// </summary>
        public string decoderImplementation { get { return GetString("decoderImplementation"); } }

        /// <summary>
        /// The total number of Full Intra Request (FIR) packets sent by this receiver. 
        /// </summary>
        public uint firCount { get { return GetUnsignedInt("firCount"); } }

        /// <summary>
        /// The total number of Picture Loss Indication (PLI) packets sent by this receiver.
        /// </summary>
        public uint pliCount { get { return GetUnsignedInt("pliCount"); } }

        /// <summary>
        /// The total number of Negative Acknowledgement (NACK) packets sent by this receiver.
        /// </summary>
        public uint nackCount { get { return GetUnsignedInt("nackCount"); } }

        /// <summary>
        /// The sum of the QP values of frames decoded by this receiver.
        /// </summary>
        public ulong qpSum { get { return GetUnsignedLong("qpSum"); } }

        /// <summary>
        /// Google-specific timing frame information.
        /// </summary>
        public string googTimingFrameInfo { get { return GetString("googTimingFrameInfo"); } }

        /// <summary>
        ///  Indicates whether the decoder is power efficient.
        /// </summary>
        public bool powerEfficientDecoder { get { return GetBool("powerEfficientDecoder"); } }

        /// <summary>
        /// The total number of times the jitter buffer was flushed.
        /// </summary>
        public ulong jitterBufferFlushes { get { return GetUnsignedLong("jitterBufferFlushes"); } }

        /// <summary>
        /// The total number of samples that were lost due to a delayed packet.
        /// </summary>
        public ulong delayedPacketOutageSamples { get { return GetUnsignedLong("delayedPacketOutageSamples"); } }

        /// <summary>
        /// The total number of samples that were lost due to a delayed packet.
        /// </summary>
        public double relativePacketArrivalDelay { get { return GetDouble("relativePacketArrivalDelay"); } }

        /// <summary>
        /// The total number of interruptions in the RTP stream.
        /// </summary>
        public ulong interruptionCount { get { return GetUnsignedLong("interruptionCount"); } }

        /// <summary>
        /// The total duration of interruptions in the RTP stream.
        /// </summary>
        public double totalInterruptionDuration { get { return GetDouble("totalInterruptionDuration"); } }

        /// <summary>
        /// The minimum playout delay for the RTP stream.
        /// </summary>
        public double minPlayoutDelay { get { return GetDouble("minPlayoutDelay"); } }


        internal RTCInboundRTPStreamStats(IntPtr ptr) : base(ptr)
        {
        }
    }

    /// <summary>
    /// Reports outbound RTP stream statistics.
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
    /// Statistics for remote inbound RTP streams.
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
    /// Statistics for remote outbound RTP streams.
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
    /// Base class for media source statistics.
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
    /// Audio source-specific statistics.
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
    /// Video source-specific statistics.
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
    /// Reports audio playout statistics.
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
    /// Provides transport-level statistics for a connection.
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
    /// Represents a report containing multiple RTCStats objects.
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
        /// Finalizer to ensure resources are released.
        /// </summary>
        ~RTCStatsReport()
        {
            this.Dispose();
        }

        /// <summary>
        /// Releases resources held by this report.
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
        /// Retrieves a stats object by its identifier.
        /// </summary>
        /// <param name="id">Stats identifier.</param>
        /// <returns>The corresponding RTCStats object.</returns>
        public RTCStats Get(string id)
        {
            return m_dictStats[id];
        }

        /// <summary>
        /// Attempts to get a stats object by its identifier.
        /// </summary>
        /// <param name="id">Stats identifier.</param>
        /// <param name="stats">Output parameter for the stats object.</param>
        /// <returns>True if found, otherwise false.</returns>
        public bool TryGetValue(string id, out RTCStats stats)
        {
            return m_dictStats.TryGetValue(id, out stats);
        }

        /// <summary>
        /// Returns all stats objects in this report.
        /// </summary>
        public IDictionary<string, RTCStats> Stats
        {
            get { return m_dictStats; }
        }
    }
}
