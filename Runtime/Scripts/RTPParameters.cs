using System;
using System.Linq;
using System.Runtime.InteropServices;

namespace Unity.WebRTC
{
    /// <summary>
    /// 
    /// </summary>
    public class RTCRtpEncodingParameters
    {
        /// <summary>
        /// 
        /// </summary>
        public bool active;

        /// <summary>
        /// 
        /// </summary>
        public ulong? maxBitrate;

        /// <summary>
        /// 
        /// </summary>
        public ulong? minBitrate;

        /// <summary>
        /// 
        /// </summary>
        public uint? maxFramerate;

        /// <summary>
        /// 
        /// </summary>
        public double? scaleResolutionDownBy;

        /// <summary>
        /// 
        /// </summary>
        public string rid;

        /// <summary>
        /// 
        /// </summary>
        public RTCRtpEncodingParameters() { }

        internal RTCRtpEncodingParameters(ref RTCRtpEncodingParametersInternal parameter)
        {
            active = parameter.active;
            maxBitrate = parameter.maxBitrate;
            minBitrate = parameter.minBitrate;
            maxFramerate = parameter.maxFramerate;
            scaleResolutionDownBy = parameter.scaleResolutionDownBy;
            if (parameter.rid != IntPtr.Zero)
                rid = parameter.rid.AsAnsiStringWithFreeMem();
        }

        internal void CopyInternal(ref RTCRtpEncodingParametersInternal instance)
        {
            instance.active = active;
            instance.maxBitrate = maxBitrate;
            instance.minBitrate = minBitrate;
            instance.maxFramerate = maxFramerate;
            instance.scaleResolutionDownBy = scaleResolutionDownBy;
            instance.rid = string.IsNullOrEmpty(rid) ? IntPtr.Zero : Marshal.StringToCoTaskMemAnsi(rid);
        }

        internal RTCRtpEncodingParametersInternal Cast()
        {
            return new RTCRtpEncodingParametersInternal
            {
                active = this.active,
                maxBitrate = this.maxBitrate,
                minBitrate = this.minBitrate,
                maxFramerate = this.maxFramerate,
                scaleResolutionDownBy = this.scaleResolutionDownBy,
                rid = string.IsNullOrEmpty(this.rid) ? IntPtr.Zero : Marshal.StringToCoTaskMemAnsi(this.rid)
            };
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class RTCRtpCodecParameters
    {
        /// <summary>
        /// 
        /// </summary>
        public readonly int payloadType;

        /// <summary>
        /// 
        /// </summary>
        public readonly string mimeType;

        /// <summary>
        /// 
        /// </summary>
        public readonly ulong? clockRate;

        /// <summary>
        /// 
        /// </summary>
        public readonly ushort? channels;

        /// <summary>
        /// 
        /// </summary>
        public readonly string sdpFmtpLine;

        internal RTCRtpCodecParameters(ref RTCRtpCodecParametersInternal src)
        {
            payloadType = src.payloadType;
            if (src.mimeType != IntPtr.Zero)
                mimeType = src.mimeType.AsAnsiStringWithFreeMem();
            clockRate = src.clockRate;
            channels = src.channels;
            if (src.sdpFmtpLine != IntPtr.Zero)
                sdpFmtpLine = src.sdpFmtpLine.AsAnsiStringWithFreeMem();
        }
    };

    /// <summary>
    /// 
    /// </summary>
    public class RTCRtpHeaderExtensionParameters
    {
        /// <summary>
        /// 
        /// </summary>
        public readonly string uri;

        /// <summary>
        /// 
        /// </summary>
        public readonly ushort id;

        /// <summary>
        /// 
        /// </summary>
        public readonly bool encrypted;

        internal RTCRtpHeaderExtensionParameters(ref RTCRtpHeaderExtensionParametersInternal src)
        {
            if (src.uri != IntPtr.Zero)
                uri = src.uri.AsAnsiStringWithFreeMem();
            id = src.id;
            encrypted = src.encrypted;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class RTCRtcpParameters
    {
        /// <summary>
        /// 
        /// </summary>
        public readonly string cname;

        /// <summary>
        /// 
        /// </summary>
        public readonly bool reducedSize;

        internal RTCRtcpParameters(ref RTCRtcpParametersInternal src)
        {
            if (src.cname != IntPtr.Zero)
                cname = src.cname.AsAnsiStringWithFreeMem();
            reducedSize = src.reducedSize;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class RTCRtpParameters
    {
        /// <summary>
        /// 
        /// </summary>
        public readonly RTCRtpHeaderExtensionParameters[] headerExtensions;

        /// <summary>
        /// 
        /// </summary>
        public readonly RTCRtcpParameters rtcp;

        /// <summary>
        /// 
        /// </summary>
        public readonly RTCRtpCodecParameters[] codecs;

        internal RTCRtpParameters(ref RTCRtpSendParametersInternal src)
        {
            headerExtensions = Array.ConvertAll(src.headerExtensions.ToArray(),
                v => new RTCRtpHeaderExtensionParameters(ref v));
            rtcp = new RTCRtcpParameters(ref src.rtcp);
            codecs = Array.ConvertAll(src.codecs.ToArray(),
                v => new RTCRtpCodecParameters(ref v));
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class RTCRtpSendParameters : RTCRtpParameters
    {
        /// <summary>
        /// 
        /// </summary>
        public RTCRtpEncodingParameters[] encodings;

        /// <summary>
        /// 
        /// </summary>
        public readonly string transactionId;

        internal RTCRtpSendParameters(ref RTCRtpSendParametersInternal src)
            : base(ref src)
        {
            this.encodings = Array.ConvertAll(src.encodings.ToArray(),
                v => new RTCRtpEncodingParameters(ref v));
            transactionId = src.transactionId.AsAnsiStringWithFreeMem();
        }

        internal void CreateInstance(out RTCRtpSendParametersInternal instance)
        {
            instance = default;
            RTCRtpEncodingParametersInternal[] encodings =
                new RTCRtpEncodingParametersInternal[this.encodings.Length];
            for (int i = 0; i < this.encodings.Length; i++)
            {
                this.encodings[i].CopyInternal(ref encodings[i]);
            }
            instance.encodings = encodings;
            instance.transactionId = Marshal.StringToCoTaskMemAnsi(transactionId);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public enum RTCRtpTransceiverDirection
    {
        /// <summary>
        /// 
        /// </summary>
        SendRecv = 0,

        /// <summary>
        /// 
        /// </summary>
        SendOnly = 1,

        /// <summary>
        /// 
        /// </summary>
        RecvOnly = 2,

        /// <summary>
        /// 
        /// </summary>
        Inactive = 3,

        /// <summary>
        /// 
        /// </summary>
        Stopped = 4
    }

    /// <summary>
    /// 
    /// </summary>
    public class RTCRtpCodecCapability
    {
        /// <summary>
        /// 
        /// </summary>
        public int? channels;

        /// <summary>
        /// 
        /// </summary>
        public int? clockRate;

        /// <summary>
        /// 
        /// </summary>
        public string mimeType;

        /// <summary>
        /// 
        /// </summary>
        public string sdpFmtpLine;

        internal RTCRtpCodecCapability(ref RTCRtpCodecCapabilityInternal v)
        {
            mimeType = v.mimeType.AsAnsiStringWithFreeMem();
            clockRate = v.clockRate;
            channels = v.channels;
            sdpFmtpLine =
                v.sdpFmtpLine != IntPtr.Zero ? v.sdpFmtpLine.AsAnsiStringWithFreeMem() : null;
        }

        internal RTCRtpCodecCapabilityInternal Cast()
        {
            RTCRtpCodecCapabilityInternal instance = new RTCRtpCodecCapabilityInternal
            {
                channels = this.channels,
                clockRate = this.clockRate,
                mimeType = this.mimeType.ToPtrAnsi(),
                sdpFmtpLine = this.sdpFmtpLine.ToPtrAnsi()
            };
            return instance;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class RTCRtpHeaderExtensionCapability
    {
        /// <summary>
        /// 
        /// </summary>
        public string uri;

        internal RTCRtpHeaderExtensionCapability(ref RTCRtpHeaderExtensionCapabilityInternal v)
        {
            uri = v.uri.AsAnsiStringWithFreeMem();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class RTCRtpCapabilities
    {
        /// <summary>
        /// 
        /// </summary>
        public readonly RTCRtpCodecCapability[] codecs;

        /// <summary>
        /// 
        /// </summary>
        public readonly RTCRtpHeaderExtensionCapability[] headerExtensions;

        internal RTCRtpCapabilities(RTCRtpCapabilitiesInternal capabilities)
        {
            codecs = Array.ConvertAll(capabilities.codecs.ToArray(),
                v => new RTCRtpCodecCapability(ref v));
            headerExtensions = Array.ConvertAll(capabilities.extensionHeaders.ToArray(),
                v => new RTCRtpHeaderExtensionCapability(ref v));
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class RTCRtpTransceiverInit
    {
        /// <summary>
        /// 
        /// </summary>
        public RTCRtpTransceiverDirection? direction;

        /// <summary>
        /// 
        /// </summary>
        public RTCRtpEncodingParameters[] sendEncodings;

        /// <summary>
        /// 
        /// </summary>
        public MediaStream[] streams;

        internal RTCRtpTransceiverInitInternal Cast()
        {
            return new RTCRtpTransceiverInitInternal
            {
                direction = direction.GetValueOrDefault(RTCRtpTransceiverDirection.SendRecv),
                sendEncodings = sendEncodings == null ? default(MarshallingArray<RTCRtpEncodingParametersInternal>) : sendEncodings.Select(_ => _.Cast()).ToArray(),
                streams = streams == null ? default(MarshallingArray<IntPtr>) : streams.Select(_ => _.self).ToArray(),
            };
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct RTCRtpCodecCapabilityInternal
    {
        public IntPtr mimeType;
        public OptionalInt clockRate;
        public OptionalInt channels;
        public IntPtr sdpFmtpLine;

        public void Dispose()
        {
            Marshal.FreeCoTaskMem(mimeType);
            mimeType = IntPtr.Zero;
            Marshal.FreeCoTaskMem(sdpFmtpLine);
            sdpFmtpLine = IntPtr.Zero;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct RTCRtpHeaderExtensionCapabilityInternal
    {
        public IntPtr uri;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct RTCRtpCapabilitiesInternal
    {
        public MarshallingArray<RTCRtpCodecCapabilityInternal> codecs;
        public MarshallingArray<RTCRtpHeaderExtensionCapabilityInternal> extensionHeaders;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct RTCRtpCodecParametersInternal
    {
        public int payloadType;
        public IntPtr mimeType;
        public OptionalUlong clockRate;
        public OptionalUshort channels;
        public IntPtr sdpFmtpLine;

    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct RTCRtpHeaderExtensionParametersInternal
    {
        public IntPtr uri;
        public ushort id;
        [MarshalAs(UnmanagedType.U1)]
        public bool encrypted;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct RTCRtcpParametersInternal
    {
        public IntPtr cname;
        [MarshalAs(UnmanagedType.U1)]
        public bool reducedSize;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct RTCRtpSendParametersInternal
    {
        public MarshallingArray<RTCRtpEncodingParametersInternal> encodings;
        public IntPtr transactionId;
        public MarshallingArray<RTCRtpCodecParametersInternal> codecs;
        public MarshallingArray<RTCRtpHeaderExtensionParametersInternal> headerExtensions;
        public RTCRtcpParametersInternal rtcp;

        public void Dispose()
        {
            encodings.Dispose();
            Marshal.FreeCoTaskMem(transactionId);
            transactionId = IntPtr.Zero;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct RTCRtpEncodingParametersInternal
    {
        [MarshalAs(UnmanagedType.U1)]
        public bool active;
        public OptionalUlong maxBitrate;
        public OptionalUlong minBitrate;
        public OptionalUint maxFramerate;
        public OptionalDouble scaleResolutionDownBy;
        public IntPtr rid;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct RTCRtpTransceiverInitInternal
    {
        public RTCRtpTransceiverDirection direction;
        public MarshallingArray<RTCRtpEncodingParametersInternal> sendEncodings;
        public MarshallingArray<IntPtr> streams;

        public void Dispose()
        {
            sendEncodings.Dispose();
            streams.Dispose();
        }
    }

}
