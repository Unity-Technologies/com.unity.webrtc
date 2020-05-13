using System;
using System.Runtime.InteropServices;

namespace Unity.WebRTC
{
    public class RTCRtpEncodingParameters
    {
        public bool active;
        public ulong? maxBitrate;
        public uint? maxFramerate;
        public string rid;

        internal RTCRtpEncodingParameters(RTCRtpEncodingParametersInternal parameter)
        {
            active = parameter.active;
            if (parameter.hasValueMaxBitrate)
                maxBitrate = parameter.maxBitrate;
            if (parameter.hasValueMaxFramerate)
                maxFramerate = parameter.maxFramerate;
            rid = parameter.rid.AsAnsiStringWithFreeMem();
        }

        internal RTCRtpEncodingParametersInternal Create()
        {
            RTCRtpEncodingParametersInternal instance = default;
            instance.active = active;
            instance.hasValueMaxBitrate = maxBitrate.HasValue;
            if(maxBitrate.HasValue)
                instance.maxBitrate = maxBitrate.Value;
            if (maxFramerate.HasValue)
                instance.hasValueMaxFramerate = maxFramerate.HasValue;
            instance.rid = Marshal.StringToCoTaskMemAnsi(rid);
            return instance;
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
            RTCRtpEncodingParametersInternal[] encodings = Array.ConvertAll(_encodings, _ => _.Create());
            RTCRtpSendParametersInternal instance = default;
            instance.encodingsLength = _encodings.Length;
            instance.encodings = IntPtrExtension.ToPtr(encodings);
            instance.transactionId = Marshal.StringToCoTaskMemAnsi(_transactionId);
            IntPtr ptr = Marshal.AllocCoTaskMem(Marshal.SizeOf(instance));
            Marshal.StructureToPtr(instance, ptr, false);
            return ptr;
        }

        static internal void FreePtr(IntPtr ptr)
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
        internal bool active;
        internal bool hasValueMaxBitrate;
        internal ulong maxBitrate;
        internal bool hasValueMaxFramerate;
        internal uint maxFramerate;
        internal IntPtr rid;
    }
}
