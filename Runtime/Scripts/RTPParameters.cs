using System;
using System.Runtime.InteropServices;

namespace Unity.WebRTC
{
    public class RTCRtpEncodingParameters
    {
        public bool active;
        public ulong? maxBitrate;
        public ulong? minBitrate;
        public uint? maxFramerate;
        public double? scaleResolutionDownBy;
        public string rid;

        internal RTCRtpEncodingParameters(ref RTCRtpEncodingParametersInternal parameter)
        {
            active = parameter.active;
            maxBitrate = parameter.maxBitrate;
            minBitrate = parameter.minBitrate;
            maxFramerate = parameter.maxFramerate;
            scaleResolutionDownBy = parameter.scaleResolutionDownBy;
            if(parameter.rid != IntPtr.Zero)
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
    }

    public class RTCRtpCodecParameters
    {
        public readonly int payloadType;
        public readonly string mimeType;
        public readonly ulong? clockRate;
        public readonly ushort? channels;
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

    public class RTCRtpHeaderExtensionParameters
    {
        public readonly string uri;
        public readonly ushort id;
        public readonly bool encrypted;
        internal RTCRtpHeaderExtensionParameters(ref RTCRtpHeaderExtensionParametersInternal src)
        {
            if (src.uri != IntPtr.Zero)
                uri = src.uri.AsAnsiStringWithFreeMem();
            id = src.id;
            encrypted = src.encrypted;
        }
    }

    public class RTCRtcpParameters
    {
        public readonly string cname;
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
        public readonly RTCRtpHeaderExtensionParameters[] headerExtensions;
        public readonly RTCRtcpParameters rtcp;
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
        public RTCRtpEncodingParameters[] encodings;
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
            for(int i = 0; i < this.encodings.Length; i++)
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
        SendRecv = 0,
        SendOnly = 1,
        RecvOnly = 2,
        Inactive = 3,
        Stopped = 4
    }

    /// <summary>
    /// 
    /// </summary>
    public class RTCRtpCodecCapability
    {
        public int? channels;
        public int? clockRate;
        public string mimeType;
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
        public readonly RTCRtpCodecCapability[] codecs;
        public readonly RTCRtpHeaderExtensionCapability[] headerExtensions;

        internal RTCRtpCapabilities(RTCRtpCapabilitiesInternal capabilities)
        {
            codecs = Array.ConvertAll(capabilities.codecs.ToArray(),
                v => new RTCRtpCodecCapability(ref v));
            headerExtensions = Array.ConvertAll(capabilities.extensionHeaders.ToArray(),
                v => new RTCRtpHeaderExtensionCapability(ref v));
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
}
