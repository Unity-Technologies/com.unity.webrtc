using System;
using System.Runtime.InteropServices;

namespace Unity.WebRTC
{
    public class RTCRtpEncodingParameters
    {
        public bool active;
        public ulong? maxBitrate;
        public uint? maxFramerate;
        public double? scaleResolutionDownBy;
        public string rid;

        internal RTCRtpEncodingParameters(RTCRtpEncodingParametersInternal parameter)
        {
            active = parameter.active;
            if (parameter.hasValueMaxBitrate)
                maxBitrate = parameter.maxBitrate;
            if (parameter.hasValueMaxFramerate)
                maxFramerate = parameter.maxFramerate;
            if (parameter.hasValueScaleResolutionDownBy)
                scaleResolutionDownBy = parameter.scaleResolutionDownBy;
            rid = parameter.rid.AsAnsiStringWithFreeMem();
        }

        internal void CopyInternal(ref RTCRtpEncodingParametersInternal instance)
        {
            instance.active = active;
            instance.hasValueMaxBitrate = maxBitrate.HasValue;
            if(maxBitrate.HasValue)
                instance.maxBitrate = maxBitrate.Value;
            instance.hasValueMaxFramerate = maxFramerate.HasValue;
            if (maxFramerate.HasValue)
                instance.maxFramerate = maxFramerate.Value;
            instance.hasValueScaleResolutionDownBy = scaleResolutionDownBy.HasValue;
            if (scaleResolutionDownBy.HasValue)
                instance.scaleResolutionDownBy = scaleResolutionDownBy.Value;
            instance.rid = Marshal.StringToCoTaskMemAnsi(rid);
        }
    }

    public class RTCRtpSendParameters
    {
        readonly RTCRtpEncodingParameters[] _encodings;
        readonly string _transactionId;

        internal RTCRtpSendParameters(IntPtr ptr)
        {
            var parameters = Marshal.PtrToStructure<RTCRtpSendParametersInternal>(ptr);
            Marshal.FreeHGlobal(ptr);

            RTCRtpEncodingParametersInternal[] encodings = new RTCRtpEncodingParametersInternal[parameters.encodingsLength];
            parameters.encodings.ToArray(encodings);
            _encodings = Array.ConvertAll(encodings, _ => new RTCRtpEncodingParameters(_));
            _transactionId = parameters.transactionId.AsAnsiStringWithFreeMem();
        }

        internal IntPtr CreatePtr()
        {
            RTCRtpEncodingParametersInternal[] encodings = new RTCRtpEncodingParametersInternal[_encodings.Length];
            for(int i = 0; i < _encodings.Length; i++)
            {
                _encodings[i].CopyInternal(ref encodings[i]);
            }
            RTCRtpSendParametersInternal instance = default;
            instance.encodingsLength = _encodings.Length;
            instance.encodings = IntPtrExtension.ToPtr(encodings);
            instance.transactionId = Marshal.StringToCoTaskMemAnsi(_transactionId);
            IntPtr ptr = Marshal.AllocCoTaskMem(Marshal.SizeOf(instance));
            Marshal.StructureToPtr(instance, ptr, false);
            return ptr;
        }

        static internal void DeletePtr(IntPtr ptr)
        {
            var instance = Marshal.PtrToStructure<RTCRtpSendParametersInternal>(ptr);
            Marshal.FreeCoTaskMem(instance.encodings);
            Marshal.FreeCoTaskMem(ptr);
        }

        public string TransactionId => _transactionId;

        public RTCRtpEncodingParameters[] Encodings => _encodings;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct RTCRtpSendParametersInternal
    {
        internal int encodingsLength;
        internal IntPtr encodings;
        internal IntPtr transactionId;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct RTCRtpEncodingParametersInternal
    {
        [MarshalAs(UnmanagedType.U1)]
        internal bool active;
        [MarshalAs(UnmanagedType.U1)]
        internal bool hasValueMaxBitrate;
        internal ulong maxBitrate;
        [MarshalAs(UnmanagedType.U1)]
        internal bool hasValueMaxFramerate;
        internal uint maxFramerate;
        [MarshalAs(UnmanagedType.U1)]
        internal bool hasValueScaleResolutionDownBy;
        internal double scaleResolutionDownBy;
        internal IntPtr rid;
    }
}
