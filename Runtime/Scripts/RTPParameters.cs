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

        internal RTCRtpEncodingParameters(RTCRtpEncodingParametersInternal parameter)
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

    /// <summary>
    /// 
    /// </summary>
    public class RTCRtpSendParameters
    {
        public string TransactionId => _transactionId;

        public RTCRtpEncodingParameters[] Encodings => _encodings;

        readonly RTCRtpEncodingParameters[] _encodings;
        readonly string _transactionId;

        internal RTCRtpSendParameters(RTCRtpSendParametersInternal parameters)
        {
            RTCRtpEncodingParametersInternal[] encodings = parameters.encodings.ToArray();
            _encodings = Array.ConvertAll(encodings, _ => new RTCRtpEncodingParameters(_));
            _transactionId = parameters.transactionId.AsAnsiStringWithFreeMem();
        }

        internal IntPtr CreatePtr()
        {
            RTCRtpEncodingParametersInternal[] encodings =
                new RTCRtpEncodingParametersInternal[_encodings.Length];
            for(int i = 0; i < _encodings.Length; i++)
            {
                _encodings[i].CopyInternal(ref encodings[i]);
            }
            RTCRtpSendParametersInternal instance = default;
            instance.encodings.Set(encodings);
            instance.transactionId = Marshal.StringToCoTaskMemAnsi(_transactionId);
            IntPtr ptr = Marshal.AllocCoTaskMem(Marshal.SizeOf(instance));
            Marshal.StructureToPtr(instance, ptr, false);
            return ptr;
        }

        static internal void DeletePtr(IntPtr ptr)
        {
            var instance = Marshal.PtrToStructure<RTCRtpSendParametersInternal>(ptr);
            Marshal.FreeCoTaskMem(instance.encodings.ptr);
            Marshal.FreeCoTaskMem(ptr);
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

        internal RTCRtpCodecCapability(RTCRtpCodecCapabilityInternal v)
        {
            mimeType = v.mimeType.AsAnsiStringWithFreeMem();
            clockRate = v.clockRate;
            channels = v.channels;
            sdpFmtpLine =
                v.sdpFmtpLine != IntPtr.Zero ? v.sdpFmtpLine.AsAnsiStringWithFreeMem() : null;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class RTCRtpHeaderExtensionCapability
    {
        public string uri;

        internal RTCRtpHeaderExtensionCapability(RTCRtpHeaderExtensionCapabilityInternal v)
        {
            uri = v.uri.AsAnsiStringWithFreeMem();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class RTCRtpCapabilities
    {
        public RTCRtpCodecCapability[] codecs;
        public RTCRtpHeaderExtensionCapability[] headerExtensions;

        internal RTCRtpCapabilities(RTCRtpCapabilitiesInternal capabilities)
        {
            codecs = Array.ConvertAll(capabilities.codecs.ToArray(),
                v => new RTCRtpCodecCapability(v));
            headerExtensions = Array.ConvertAll(capabilities.extensionHeaders.ToArray(),
                v => new RTCRtpHeaderExtensionCapability(v));
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct RTCRtpCodecCapabilityInternal
    {
        public IntPtr mimeType;
        public OptionalInt clockRate;
        public OptionalInt channels;
        public IntPtr sdpFmtpLine;
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
    internal struct RTCRtpSendParametersInternal
    {
        public MarshallingArray<RTCRtpEncodingParametersInternal> encodings;
        public IntPtr transactionId;
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
