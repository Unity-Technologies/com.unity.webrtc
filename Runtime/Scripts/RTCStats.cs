using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

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
        Codec,
        [StringValue("inbound-rtp")]
        InboundRtp,
        [StringValue("outbound-rtp")]
        OutboundRtp,
        [StringValue("remote-inbound-rtp")]
        RemoteInboundRtp,
        [StringValue("remote-outbound-rtp")]
        RemoteOutboundRtp,
        [StringValue("media-source")]
        MediaSource,
        [StringValue("csrc")]
        Csrc,
        [StringValue("peer-connection")]
        PeerConnection,
        [StringValue("data-channel")]
        DataChannel,
        [StringValue("stream")]
        Stream,
        [StringValue("track")]
        Track,
        [StringValue("transceiver")]
        Transceiver,
        [StringValue("sender")]
        Sender,
        [StringValue("receiver")]
        Receiver,
        [StringValue("transport")]
        Transport,
        [StringValue("sctp-transport")]
        SctpTransport,
        [StringValue("candidate-pair")]
        CandidatePair,
        [StringValue("local-candidate")]
        LocalCandidate,
        [StringValue("remote-candidate")]
        RemoteCandidate,
        [StringValue("certificate")]
        Certificate,
        [StringValue("ice-server")]
        IceServer
    }

    static class RTCStatsTypeUtils
    {
        private static Dictionary<string, RTCStatsType> s_table;
        private static string[] s_list;

        public static string GetString(this RTCStatsType type)
        {
            return s_list[(int)type];
        }

        public static RTCStatsType Parse(string str)
        {
            return s_table[str];
        }

        static RTCStatsTypeUtils()
        {
            s_table = new Dictionary<string, RTCStatsType>();
            s_list = new string[Enum.GetValues(typeof(RTCStatsType)).Length];
            foreach (RTCStatsType value in Enum.GetValues(typeof(RTCStatsType)))
            {
                Type type = value.GetType();
                System.Reflection.FieldInfo fieldInfo = type.GetField(value.ToString());
                if (fieldInfo == null) continue;
                StringValueAttribute[] attribute = fieldInfo.GetCustomAttributes(typeof(StringValueAttribute), false) as StringValueAttribute[];

                // Return the first if there was a match.
                string str = attribute != null && attribute.Length > 0 ? attribute[0].StringValue : null;
                s_table.Add(str, value);
                s_list[(int)value] = str;
            }
        }
    }

    internal class RTCStatsMember
    {
        private IntPtr self;

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

        internal object GetValue(StatsMemberType type)
        {
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
                    return NativeMethods.StatsMemberGetBoolArray(self, ref length).AsBoolArray((int)length);
                case StatsMemberType.SequenceInt32:
                    return NativeMethods.StatsMemberGetIntArray(self, ref length).AsIntArray((int)length);
                case StatsMemberType.SequenceUint32:
                    return NativeMethods.StatsMemberGetUnsignedIntArray(self, ref length).AsUnsignedIntArray((int)length);
                case StatsMemberType.SequenceInt64:
                    return NativeMethods.StatsMemberGetLongArray(self, ref length).AsLongArray((int)length);
                case StatsMemberType.SequenceUint64:
                    return NativeMethods.StatsMemberGetUnsignedLongArray(self, ref length).AsUnsignedLongArray((int)length);
                case StatsMemberType.SequenceDouble:
                    return NativeMethods.StatsMemberGetDoubleArray(self, ref length).AsDoubleArray((int)length);
                case StatsMemberType.SequenceString:
                    return NativeMethods.StatsMemberGetStringArray(self, ref length).AsStringArray((int)length);
                default:
                    throw new ArgumentException();
            }
        }
    }

    public class RTCStats
    {
        private IntPtr self;
        internal Dictionary<string, RTCStatsMember> m_members;

        public RTCStatsType Type
        {
            get
            {
                string str = NativeMethods.StatsGetType(self).AsAnsiStringWithFreeMem();
                return RTCStatsTypeUtils.Parse(str);
            }
        }

        public string Id
        {
            get { return NativeMethods.StatsGetId(self).AsAnsiStringWithFreeMem(); }
        }

        public long Timestamp
        {
            get { return NativeMethods.StatsGetTimestamp(self); }
        }

        public object this[string key]
        {
            get
            {
                if (!m_members.TryGetValue(key, out var member))
                {
                    throw new KeyNotFoundException(key);
                }

                StatsMemberType type = member.GetValueType();
                return member.GetValue(type);
            }
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
            IntPtr buf = NativeMethods.StatsGetMembers(self, ref length);
            var array = new IntPtr[length];
            Marshal.Copy(buf, array, 0, (int)length);
            Marshal.FreeCoTaskMem(buf);

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

    public class RTCStatsReport : IEnumerable<RTCStats>
    {
        private IntPtr self;
        private readonly Dictionary<RTCStatsType, RTCStats> m_dictStats;

        internal RTCStatsReport(IntPtr ptr)
        {
            self = ptr;
            uint length = 0;
            IntPtr[] array = NativeMethods.StatsReportGetList(self, ref length).AsIntPtrArray((int)length);

            m_dictStats = new Dictionary<RTCStatsType, RTCStats>();
            for (int i = 0; i < length; i++)
            {
                RTCStats stats = new RTCStats(array[i]);
                m_dictStats[stats.Type] = stats;
            }
        }

        public RTCStats this[RTCStatsType key]
        {
            get
            {
                return m_dictStats[key];
            }
        }

        public IEnumerator<RTCStats> GetEnumerator()
        {
            return m_dictStats.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public class RTCPeerConnectionStats : RTCStats
    {
        public ulong dataChannelsOpened { get { return (ulong) base["dataChannelsOpened"]; } }
        public ulong dataChannelsClosed { get { return (ulong) base["dataChannelsClosed"]; } }
        public ulong dataChannelsRequested { get { return (ulong) base["dataChannelsRequested"]; } }
        public ulong dataChannelsAccepted { get { return (ulong) base["dataChannelsAccepted"]; } }

        internal RTCPeerConnectionStats(IntPtr ptr) : base(ptr)
        {
        }
    }
}
