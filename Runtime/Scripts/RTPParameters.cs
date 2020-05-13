using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Unity.WebRTC
{
    [StructLayout(LayoutKind.Sequential)]
    public struct RTCRtpSendParameters
    {
        internal int encodingsLength;
        internal IntPtr encodings;
        internal IntPtr transactionId;

        RTCRtpEncodingParameters[] _encodings;

        public RTCRtpEncodingParameters[] Encodings
        {
            get
            {
                if (_encodings == null)
                {
                    Debug.Log(encodingsLength);
                    //_encodings = Marshal.PtrToStructure<RTCRtpEncodingParameters[]>(encodings);
                }
                return _encodings;
            }
            set
            {

            }
        }

        public string TransactionId
        {
            get
            {
                return transactionId.AsAnsiStringWithFreeMem();
            }
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct RTCRtpEncodingParameters
    {
        internal bool active;
        internal ulong maxBitrate;
        internal uint maxFramerate;
        internal IntPtr rid;
    }
}
